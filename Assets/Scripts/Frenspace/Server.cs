using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using WebSocketSharp;
using WebSocketSharp.Server;

using GameEvents;

public class Server : MonoBehaviour
{
  WebSocketServer wss;

  GameEventHandler evntHndlr;

  private List<string> ids = new List<string>();

  public static void LogBytes(byte[] data)
  {
    foreach (var e in data)
    {
      Debug.Log(e);
    }
  }

  private class Broadcaster : WebSocketBehavior
  {
    public Server svr;

    public Broadcaster(Server sv)
    {
      this.svr = sv;
    }

    private ushort idIdx()
    {
      var idIdx = svr.ids.FindIndex(s => s == this.ID);
      return Convert.ToUInt16(idIdx);
    }

    private byte[] withID(byte[] bytes)
    {
      var idIdxBytes = BitConverter.GetBytes(idIdx());
      return idIdxBytes.Concat(bytes).ToArray();
    }

    protected override void OnMessage(MessageEventArgs e)
    {
      var ev = e.RawData;
      var evId = withID(ev);

      switch ((GameEvent)ev[0])
      {
        default:
          BroadcastBytes(evId);
          break;
      }

      svr.evntHndlr.HandleEvent(evId);
    }

    protected override void OnOpen()
    {
      var freeIdx = svr.ids.FindIndex(s => s == null);
      if (freeIdx >= 0)
      {
        svr.ids[freeIdx] = this.ID;
      }
      else
      {
        svr.ids.Add(this.ID);
      }

      Debug.Log(idIdx() + " joined (" + this.ID + ")");

      var idBytes = withID(new byte[] { });
      Send(idBytes);
      var joined = idBytes.Concat(new byte[] { (byte)GameEvent.PLAYER, (byte)PlayerEvent.JOINED }).ToArray();
      BroadcastBytes(joined);

      svr.evntHndlr.SpawnFren(idIdx());

      var list = new byte[] { 0x00, 0x00, (byte)GameEvent.PLAYER, (byte)PlayerEvent.LIST };
      var frens = svr.evntHndlr.frenHandler.GetFrens();
      for (int i = 0; i < frens.Count; i++)
      {
        var fren = frens[i];
        if (fren == null)
        {
          list = list
            .Concat(new byte[] { 0x00 })
            .Concat(new byte[] { (byte)MoveEvent.STOPPED })
            .Concat(BytesUtility.FromVector3(new Vector3()))
            .ToArray();
        }
        else
        {
          list = list
            .Concat(new byte[] { 0x01 })
            .Concat(new byte[] { (byte)fren.movement })
            .Concat(BytesUtility.FromVector3(fren.go.transform.position))
            .ToArray();
        }
      }
      Send(list);
    }

    protected override void OnClose(CloseEventArgs e)
    {
      var id = idIdx();
      svr.ids[id] = null;

      svr.evntHndlr.RemoveFren(id);

      Debug.Log(idIdx() + " left (" + this.ID + ")");
    }

    protected override void OnError(ErrorEventArgs e)
    {
      Debug.Log(idIdx() + " session error.");
      OnClose(null);
    }

    private void BroadcastBytes(params byte[] bytes)
    {
      foreach (var session in Sessions.Sessions)
      {
        if (session.State != WebSocketState.Open) continue;
        if (session.ID == this.ID) continue;
        session.Context.WebSocket.Send(bytes);
      }
    }

    private void SendBytes(params byte[] bytes)
    {
      Send(bytes);
    }
  }

  void Start()
  {
    StartCoroutine("StartServer");
  }

  void StartServer()
  {
    evntHndlr = GetComponent<GameEventHandler>();

    wss = new WebSocketServer("ws://localhost:9000");
    wss.AddWebSocketService<Broadcaster>("/", () => new Broadcaster(this));
    wss.Start();
  }

  void OnDestroy()
  {
    wss.Stop();
  }
}