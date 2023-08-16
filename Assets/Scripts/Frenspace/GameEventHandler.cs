using System;
using System.Collections.Generic;
using System.Text;
using GameEvents;
using GameStates;
using UnityEngine;

public class GameEventHandler : MonoBehaviour
{
  public GameObject playerPrefab;

  [HideInInspector]
  public FrenHandler frenHandler;

  private ushort ID = ushort.MaxValue;

  private GameObject player;


  void Start()
  {
    frenHandler = GetComponent<FrenHandler>();
  }

  public void HandleEvent(byte[] e)
  {
    var id = BitConverter.ToUInt16(new ReadOnlySpan<byte>(e, 0, 2));

    switch ((GameEvent)e[2])
    {
      case GameEvent.PLAYER:
        {
          switch ((PlayerEvent)e[3])
          {
            case PlayerEvent.REQUEST: // server
              SpawnFren(new FrenHandler.Fren()
              {
                ID = id,
                go = new GameObject() { transform = { position = transform.position } },

                instanceId = 0,
                instance = InstanceState.WORLD,

                movement = MovementState.STOPPED,
                vehicle = VehicleState.UNMOUNTED
              });
              break;
            case PlayerEvent.LIST: // client
              ID = id;
              HandlePlayerList(e);
              break;
            case PlayerEvent.JOINED: // client
              SpawnFren(BytesUtility.UnpackFren(new ReadOnlySpan<byte>(e, 4, 19).ToArray()));
              break;
            case PlayerEvent.LEFT:  // client
              RemoveFren(id);
              break;
          }
          break;
        }
      case GameEvent.MOVE:
        {
          switch ((MovementState)e[3])
          {
            case MovementState.PORT:
              if (id == ID)
              {
                PortPlayer(new ReadOnlySpan<byte>(e, 4, e.Length - 4));
              }
              else
              {
                PortFren(id, new ReadOnlySpan<byte>(e, 4, e.Length - 4));
              }
              break;
            default:
              MoveFren(id, (MovementState)e[3]);
              break;
          }
          break;
        }
      case GameEvent.SOCIAL:
        {
          switch ((SocialEvent)e[3])
          {
            case SocialEvent.SAY:
              Debug.Log(id + ":" + Encoding.UTF8.GetString(new ReadOnlySpan<byte>(e, 4, e.Length - 4)));
              break;
          }
          break;
        }
    }
  }

  void HandlePlayerList(byte[] bytes)
  {
    var frens = new List<FrenHandler.Fren>();

    var i = 4; // skip id and events
    var chunkSize = 19;
    while (i < bytes.Length)
    {
      var b = new ReadOnlySpan<byte>(bytes, i, chunkSize).ToArray();

      var f = BytesUtility.UnpackFren(b);
      if (f.ID == ID)
      {
        SpawnPlayer(f);
      }
      else
      {
        frens.Add(f);
      }

      i += chunkSize;
    }

    if (frens.Count > 0)
      frenHandler.SetFrens(frens);
  }

  public void SpawnPlayer(FrenHandler.Fren playerFren)
  {
    player = Instantiate(playerPrefab, playerFren.go.transform.position, transform.rotation);
    Camera.main.GetComponent<CameraControllercs>().target = player;
  }

  public void SpawnFren(FrenHandler.Fren fren)
  {
    frenHandler.Spawn(fren);
  }

  public void RemoveFren(ushort id)
  {
    frenHandler.Remove(id);
  }

  void PortPlayer(ReadOnlySpan<byte> bytes)
  {
    var to = BytesUtility.ToVector3(bytes.ToArray());
    player.GetComponent<Frenspace.Player.Movement>().Port(to);
  }

  void PortFren(ushort id, ReadOnlySpan<byte> bytes)
  {
    var to = BytesUtility.ToVector3(bytes.ToArray());
    frenHandler.Port(id, to);
  }

  void MoveFren(ushort id, MovementState e)
  {
    frenHandler.SetMovementState(id, e);
  }

}
