using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Networking.Transport;

using GameEvents;


// [BurstCompile]
struct ClientUpdateJob : IJob
{
  public NetworkDriver Driver;
  public NativeArray<NetworkConnection> Connection;

  public NativeArray<ushort> messageAvailable;
  public NativeArray<byte> messageBytes;

  public void Execute()
  {
    if (!Connection[0].IsCreated)
    {
      return;
    }

    NetworkEvent.Type cmd;
    while ((cmd = Connection[0].PopEvent(Driver, out DataStreamReader stream)) != NetworkEvent.Type.Empty)
    {
      if (cmd == NetworkEvent.Type.Connect)
      {
        Debug.Log("Connected, send REQUEST.");

        var bytes = new NativeArray<byte>(2, Allocator.Temp);
        bytes[0] = (byte)GameEvent.PLAYER;
        bytes[1] = (byte)PlayerEvent.REQUEST;
        Driver.BeginSend(Connection[0], out var writer);
        writer.WriteBytes(bytes);
        Driver.EndSend(writer);
      }
      else if (cmd == NetworkEvent.Type.Data)
      {
        messageAvailable[0] = Convert.ToUInt16(stream.Length);
        stream.ReadBytes(messageBytes.GetSubArray(0, stream.Length));
      }
      else if (cmd == NetworkEvent.Type.Disconnect)
      {
        Debug.Log("We disconnected from the server. ");
        Connection[0] = default;
      }
    }
  }
}

public class Client : MonoBehaviour
{
  public static Client main;

  NetworkDriver m_Driver;
  NativeArray<NetworkConnection> m_Connection;

  NativeArray<ushort> messageAvailable;
  NativeArray<byte> messageBytes;

  JobHandle m_ClientJobHandle;

  GameEventHandler geh;

  void Start()
  {
    geh = GetComponent<GameEventHandler>();

    m_Driver = NetworkDriver.Create(new WebSocketNetworkInterface());
    m_Connection = new NativeArray<NetworkConnection>(1, Allocator.Persistent);

    //                                  safe MTU
    messageBytes = new NativeArray<byte>(1000, Allocator.Persistent);

    var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(7777);
    m_Connection[0] = m_Driver.Connect(endpoint);

    main = this;
  }

  void Update()
  {
    m_ClientJobHandle.Complete();

    messageAvailable = new NativeArray<ushort>(1, Allocator.TempJob);

    var job = new ClientUpdateJob
    {
      Driver = m_Driver,
      Connection = m_Connection,
      messageAvailable = messageAvailable,
      messageBytes = messageBytes
    };

    m_ClientJobHandle = m_Driver.ScheduleUpdate();
    m_ClientJobHandle = job.Schedule(m_ClientJobHandle);
  }

  void LateUpdate()
  {
    m_ClientJobHandle.Complete();

    if (messageAvailable[0] > 0)
    {
      var bytes = messageBytes.GetSubArray(0, messageAvailable[0]);
      geh?.HandleEvent(bytes.ToArray());
    }

    messageAvailable.Dispose();
  }

  void OnDestroy()
  {
    m_ClientJobHandle.Complete();
    m_Driver.Dispose();
    m_Connection.Dispose();
    messageBytes.Dispose();
  }

  [ContextMenu("Test")]
  public void Test()
  {
    Send(new byte[] { (byte)GameEvent.PLAYER, (byte)PlayerEvent.JOINED });
  }

  public void Send(ReadOnlySpan<byte> bytes)
  {
    m_ClientJobHandle.Complete();
    m_Driver.BeginSend(m_Connection[0], out var writer);
    foreach (var b in bytes)
    {
      writer.WriteByte(b);
    }
    m_Driver.EndSend(writer);
  }
}
