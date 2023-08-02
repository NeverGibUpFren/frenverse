using System;
using System.Linq;
using System.Text;
using UnityEngine;

using WebSocketSharp;

using GameEvents;
using System.Collections.Generic;

public class Client : MonoBehaviour
{
  public static Client main;

  WebSocket ws;
  ushort ID;

  GameEventHandler evntHndlr;

  void Start()
  {
    evntHndlr = GetComponent<GameEventHandler>();

    Connect();

    main = this;
  }

  void Connect()
  {
    ws = new WebSocket("ws://localhost:9000");

    EventHandler<MessageEventArgs> SetID = null;
    SetID = (s, e) =>
    {
      ID = BitConverter.ToUInt16(e.RawData);
      evntHndlr.SpawnPlayer(ID);
      ws.OnMessage += HandleMsg;
      ws.OnMessage -= SetID;
    };
    ws.OnMessage += SetID;

    ws.OnClose += (s, e) => Debug.Log(2);

    ws.Connect();
  }

  void HandleMsg(object sender, MessageEventArgs e)
  {
    evntHndlr.HandleEvent(e.RawData);
  }


  [ContextMenu("Test")]
  public void Test()
  {
    ws.Close();
    Debug.Log(1);
  }

  void OnDestroy()
  {
    ws.Close();
  }

  public void Send(params byte[] events)
  {
    ws.Send(events);
  }

  public void Send(string s, params byte[] events)
  {
    ws.Send(events.Concat(Encoding.UTF8.GetBytes(s)).ToArray());
  }

}
