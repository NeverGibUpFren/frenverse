using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Networking.Transport;

using GameEvents;


// [BurstCompile]
struct ServerUpdateConnectionsJob : IJob
{
  public NetworkDriver Driver;
  public NativeList<NetworkConnection> Connections;

  public void Execute()
  {
    // Clean up connections.
    for (int i = 0; i < Connections.Length; i++)
    {
      if (!Connections[i].IsCreated)
      {
        Connections.RemoveAtSwapBack(i);
        i--;
      }
    }

    // Accept new connections.
    NetworkConnection c;
    while ((c = Driver.Accept()) != default)
    {
      Connections.Add(c);
      Debug.Log("Accepted a connection.");
    }
  }
}

// [BurstCompile]
struct ServerUpdateJob : IJobParallelForDefer
{
  public NetworkDriver.Concurrent Driver;

  [NativeDisableParallelForRestriction] // TODO: This might not be safe, probably do a seperate job for broadcasting
  public NativeArray<NetworkConnection> Connections;

  [NativeDisableParallelForRestriction]
  public NativeArray<ushort> indices;
  [NativeDisableParallelForRestriction]
  public NativeArray<byte> allMessageBytes;

  readonly (byte, byte) IdBytes(int i)
  {
    var id = Convert.ToUInt16(i);
    return ((byte)(id >> 0), (byte)(id >> 8));
  }

  public void Execute(int i)
  {
    NetworkEvent.Type cmd;
    while ((cmd = Driver.PopEventForConnection(Connections[i], out DataStreamReader stream)) != NetworkEvent.Type.Empty)
    {
      if (cmd == NetworkEvent.Type.Data)
      {
        var bytes = new NativeArray<byte>(stream.Length + 2, Allocator.Temp);

        var (b1, b2) = IdBytes(i);
        bytes[0] = b1; bytes[1] = b2;

        stream.ReadBytes(bytes.GetSubArray(2, bytes.Length - 2));

        switch ((GameEvent)bytes[2])
        {
          case GameEvent.PLAYER:
            {
              switch ((PlayerEvent)bytes[3])
              {
                case PlayerEvent.REQUEST:
                  {
                    // set the id to the request bytes, since ASSIGN is yet to happen
                    bytes[0] = b1;
                    bytes[1] = b2;

                    var req = new NativeArray<byte>(4, Allocator.Temp);
                    req[0] = b1;
                    req[1] = b2;
                    req[2] = (byte)GameEvent.PLAYER;
                    // req[3] = (byte)PlayerEvent.ASSIGN;

                    // Send(req, i);

                    req[3] = (byte)PlayerEvent.JOINED;
                    Broadcast(req, i);

                    req.Dispose();
                    break;
                  }
              }
              goto default;
            }
          default:
            Broadcast(bytes, i);
            break;
        }

        WriteToCurrentBytesSlot(bytes, i);

        bytes.Dispose();
      }
      else if (cmd == NetworkEvent.Type.Disconnect)
      {
        Debug.Log("Client disconnected from server.");

        var (b1, b2) = IdBytes(i);
        var bytes = new NativeArray<byte>(4, Allocator.Temp);
        bytes[0] = b1;
        bytes[1] = b2;
        bytes[2] = (byte)GameEvent.PLAYER;
        bytes[3] = (byte)PlayerEvent.LEFT;

        Broadcast(bytes, i);

        WriteToCurrentBytesSlot(bytes, i);

        bytes.Dispose();

        Connections[i] = default;
      }
    }
  }

  void WriteToCurrentBytesSlot(NativeArray<byte> bytes, int i)
  {
    var bytesCursor = 0;
    var freeIdxSlot = 0;
    for (int j = 0; j < indices.Length; j++)
    {
      if (indices[j] == 0)
      {
        freeIdxSlot = j;
        bytesCursor = j == 0 ? 0 : indices[j - i];
        break;
      }
    }

    indices[freeIdxSlot] = Convert.ToUInt16(bytesCursor + bytes.Length);
    var currentBytesSlot = allMessageBytes.GetSubArray(bytesCursor, bytes.Length);
    bytes.CopyTo(currentBytesSlot);
    Debug.Log($"{i}: bytes {bytesCursor} to {bytesCursor + bytes.Length}");
  }

  void Broadcast(NativeArray<byte> bytes, int self)
  {
    for (int i = 0; i < Connections.Length; i++)
    {
      if (self == i) continue; // don't broadcast to self
      Send(bytes, i);
    }
  }

  void Send(NativeArray<byte> bytes, int i)
  {
    Driver.BeginSend(Connections[i], out var writer);
    writer.WriteBytes(bytes);
    Driver.EndSend(writer);
  }
}


public class Server : MonoBehaviour
{
  NetworkDriver m_Driver;
  NativeList<NetworkConnection> m_Connections;

  NativeArray<ushort> indices;
  NativeArray<byte> allMessageBytes;

  JobHandle m_ServerJobHandle;

  GameEventHandler geh;

  void Start()
  {
    geh = GetComponent<GameEventHandler>();

    m_Driver = NetworkDriver.Create(new WebSocketNetworkInterface());

    var MAX_CONNECTIONS = 6;
    m_Connections = new NativeList<NetworkConnection>(MAX_CONNECTIONS, Allocator.Persistent);

    //                                      MAX_CONNS * safe MTU size
    allMessageBytes = new NativeArray<byte>(MAX_CONNECTIONS * 1000, Allocator.Persistent);

    indices = new NativeArray<ushort>(0, Allocator.TempJob);

    var endpoint = NetworkEndpoint.AnyIpv4.WithPort(7777);
    if (m_Driver.Bind(endpoint) != 0)
    {
      Debug.LogError("Failed to bind to port 7777.");
      return;
    }
    m_Driver.Listen();
  }

  void Update()
  {
    m_ServerJobHandle.Complete();

    HandleMessages();

    var connectionJob = new ServerUpdateConnectionsJob
    {
      Driver = m_Driver,
      Connections = m_Connections
    };

    indices = new NativeArray<ushort>(m_Connections.Length, Allocator.TempJob);

    var serverUpdateJob = new ServerUpdateJob
    {
      Driver = m_Driver.ToConcurrent(),
      Connections = m_Connections.AsDeferredJobArray(),
      indices = indices,
      allMessageBytes = allMessageBytes
    };

    m_ServerJobHandle = m_Driver.ScheduleUpdate();
    m_ServerJobHandle = connectionJob.Schedule(m_ServerJobHandle);
    m_ServerJobHandle = serverUpdateJob.Schedule(m_Connections, 1, m_ServerJobHandle);
  }

  void HandleMessages()
  {
    var lastIdx = 0;
    for (int i = 0; i < indices.Length; i++)
    {
      var idx = indices[i];
      if (idx == 0) break;

      var bytes = allMessageBytes.GetSubArray(lastIdx, idx - lastIdx);
      geh?.HandleEvent(bytes.ToArray());

      switch ((GameEvent)bytes[2])
      {
        case GameEvent.PLAYER:
          {
            switch ((PlayerEvent)bytes[3])
            {
              case PlayerEvent.REQUEST:
                // TODO: this is ugly since this runs on the main thread
                // but it's hard since we need to access to whole game state
                // so this is a place where future optimization could be made

                // send game state
                var b = new byte[] { 0x00, 0x00, (byte)GameEvent.PLAYER, (byte)PlayerEvent.LIST }.AsEnumerable();
                foreach (var fb in geh.frenHandler.GetFrens().Where(f => f != null).Select(f => BytesUtility.PackFren(f)))
                {
                  b = b.Concat(fb);
                }

                var id = BitConverter.ToUInt16(new byte[] { bytes[0], bytes[1] });
                m_Driver.BeginSend(m_Connections[id], out var writer);
                foreach (var rb in b)
                {
                  writer.WriteByte(rb);
                }
                m_Driver.EndSend(writer);

                break;
            }
            break;
          }
      }

      lastIdx = idx;
    }

    indices.Dispose();
  }

  void OnDestroy()
  {
    if (m_Driver.IsCreated)
    {
      m_ServerJobHandle.Complete();
      m_Driver.Dispose();
      m_Connections.Dispose();
      allMessageBytes.Dispose();
    }
  }

}