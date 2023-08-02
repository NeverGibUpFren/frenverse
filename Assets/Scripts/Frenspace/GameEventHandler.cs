using System;
using System.Collections.Generic;
using System.Text;
using GameEvents;
using UnityEngine;

public class GameEventHandler : MonoBehaviour
{
  public Transform spawnPoint;
  public GameObject playerPrefab;
  public FrenHandler frenHandler;

  private ushort ID = ushort.MaxValue;

  private GameObject player;


  private List<Action> actions = new List<Action>();
  void mainThread(Action a)
  {
    actions.Add(a);
  }

  void Update()
  {
    if (actions.Count > 0)
    {
      foreach (var action in actions)
      {
        action();
      }
      actions.Clear();
    }
  }


  public void HandleEvent(byte[] e)
  {
    var id = BitConverter.ToUInt16(new Span<byte>(e, 0, 2));

    switch ((GameEvent)e[2])
    {
      case GameEvent.PLAYER:
        {
          switch ((PlayerEvent)e[3])
          {
            case PlayerEvent.LIST:
              HandlePlayerList(e);
              break;
            case PlayerEvent.JOINED:
              SpawnFren(id);
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
                PortPlayer(id, new Span<byte>(e, 4, e.Length - 4));
              }
              else
              {
                PortFren(id, new Span<byte>(e, 4, e.Length - 4));
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
              Debug.Log(id + ":" + Encoding.UTF8.GetString(new Span<byte>(e, 4, e.Length - 4)));
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
    mainThread(() => frenHandler.SetFrens(frens));
  }

  public void SpawnPlayer(ushort id)
  {
    ID = id;
    mainThread(() =>
    {
      player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
      Camera.main.GetComponent<CameraControllercs>().target = player;
    });
  }

  public void SpawnFren(ushort id)
  {
    mainThread(() => frenHandler.Spawn(id));
  }

  public void RemoveFren(ushort id)
  {
    mainThread(() => frenHandler.Remove(id));
  }

  void PortPlayer(ushort id, ReadOnlySpan<byte> bytes)
  {
    var to = BytesUtility.ToVector3(bytes.ToArray());
    mainThread(() => player.GetComponent<Frenspace.Player.Movement>().Port(to));
  }

  void PortFren(ushort id, ReadOnlySpan<byte> bytes)
  {
    var to = BytesUtility.ToVector3(bytes.ToArray());
    mainThread(() => frenHandler.Port(id, to));
  }

  void MoveFren(ushort id, MoveEvent e)
  {
    frenHandler.SetMovementState(id, e);
  }

}
