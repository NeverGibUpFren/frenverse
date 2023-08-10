using System;
using System.Collections.Generic;
using System.Text;
using GameEvents;
using UnityEngine;

public class GameEventHandler : MonoBehaviour
{
  public GameObject playerPrefab;
  public FrenHandler frenHandler;

  private ushort ID = ushort.MaxValue;

  private GameObject player;


  public void HandleEvent(byte[] e)
  {
    var id = BitConverter.ToUInt16(new ReadOnlySpan<byte>(e, 0, 2));

    switch ((GameEvent)e[2])
    {
      case GameEvent.PLAYER:
        {
          switch ((PlayerEvent)e[3])
          {
            case PlayerEvent.ASSIGN:
              ID = id;
              SpawnPlayer();
              break;
            case PlayerEvent.LIST:
              HandlePlayerList(e);
              break;
            case PlayerEvent.JOINED:
              SpawnFren(id);
              break;
            case PlayerEvent.REQUEST:
              SpawnFren(id);
              break;
            case PlayerEvent.LEFT:
              RemoveFren(id);
              break;
          }
          break;
        }
      case GameEvent.MOVE:
        {
          switch ((MoveEvent)e[3])
          {
            case MoveEvent.PORT:
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
              MoveFren(id, (MoveEvent)e[3]);
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
    var frens = new List<(bool empty, Vector3 pos, MoveEvent movement)>();
    BytesUtility.ForEachChunk(new ReadOnlySpan<byte>(bytes, 4, bytes.Length - 4), 14, (chunk, i) =>
    {
      if (chunk[0] == 0x00)
      {
        frens.Add((false, new Vector3(), MoveEvent.STOPPED));
      }
      else
      {
        frens.Add((true, BytesUtility.ToVector3(new ReadOnlySpan<byte>(chunk, 2, 12).ToArray()), (MoveEvent)chunk[1]));
      }
    });
    frenHandler.SetFrens(frens);
  }

  public void SpawnPlayer()
  {
    player = Instantiate(playerPrefab, transform.position, transform.rotation);
    Camera.main.GetComponent<CameraControllercs>().target = player;
  }

  public void SpawnFren(ushort id)
  {
    frenHandler.Spawn(id);
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

  void MoveFren(ushort id, MoveEvent e)
  {
    frenHandler.SetMovementState(id, e);
  }

}
