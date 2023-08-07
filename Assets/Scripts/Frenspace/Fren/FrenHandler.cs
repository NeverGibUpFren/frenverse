using System;
using System.Collections.Generic;
using UnityEngine;

using GameEvents;

public class FrenHandler : MonoBehaviour
{
  public class Fren
  {
    public ushort ID;
    public GameObject go;

    public MoveEvent movement;
  }

  public GameObject frenPrefab;

  private List<Fren> frens = new List<Fren>();

  void Update()
  {
    foreach (var fren in frens)
    {
      if (fren == null) continue;

      var go = fren.go;
      var t = go.transform;

      if (fren.movement == MoveEvent.STOPPED)
      {
        fren.go.GetComponent<Animator>().Play("Idle");
      }
      else
      {
        var movement = new Vector3();
        switch (fren.movement)
        {
          case MoveEvent.NORTH:
            t.rotation = Quaternion.AngleAxis(0, Vector3.up);
            movement = Vector3.forward;
            break;
          case MoveEvent.SOUTH:
            t.rotation = Quaternion.AngleAxis(180, Vector3.up);
            movement = Vector3.back;
            break;
          case MoveEvent.WEST:
            t.rotation = Quaternion.AngleAxis(-90, Vector3.up);
            movement = Vector3.left;
            break;
          case MoveEvent.EAST:
            t.rotation = Quaternion.AngleAxis(90, Vector3.up);
            movement = Vector3.right;
            break;
        }

        // movement = Vector3.forward;

        go.GetComponent<CharacterController>().Move((1f * movement) * Time.deltaTime);

        go.GetComponent<Animator>().Play("Run");
      }

      if (t.position.y > 0)
      {
        // fake gravity
        go.GetComponent<CharacterController>().Move(new Vector3(0, -2f, 0) * Time.deltaTime);
      }

    }
  }

  public void Spawn(ushort id)
  {
    var frenObj = Instantiate(frenPrefab, transform.position, frenPrefab.transform.rotation, transform);

    var fren = new Fren() { ID = id, go = frenObj, movement = MoveEvent.STOPPED };

    if (id < frens.Count)
    {
      frens[id] = fren;
    }
    else
    {
      frens.Add(fren);
    }
  }

  public void SetMovementState(ushort id, MoveEvent movement)
  {
    frens[id].movement = movement;
  }

  public void Port(ushort id, Vector3 to)
  {
    frens[id].go.GetComponent<Frenspace.Player.Movement>().Port(to);
  }

  public void Remove(ushort id)
  {
    Destroy(frens[id].go);
    frens[id] = null;
  }

  public List<Fren> GetFrens()
  {
    return frens;
  }

  public void SetFrens(List<(bool empty, Vector3 pos, MoveEvent movement)> list)
  {
    for (int i = 0; i < list.Count; i++)
    {
      (bool empty, Vector3 pos, MoveEvent movement) = list[i];
      if (empty)
      {
        frens.Add(null);
      }
      else
      {
        var frenObj = Instantiate(frenPrefab, pos, transform.rotation);
        frenObj.transform.parent = transform;

        var fren = new Fren() { ID = Convert.ToUInt16(i), go = frenObj, movement = movement };

        frens.Add(fren);
      }
    }
  }

}
