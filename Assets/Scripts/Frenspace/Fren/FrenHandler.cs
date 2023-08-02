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
  public Transform spawnPoint;

  private List<Fren> frens = new List<Fren>();

  void Update()
  {
    foreach (var fren in frens)
    {
      if (fren == null) continue;

      // TODO: handle each fren, animation, movement, etc.

    }
  }

  public void Spawn(ushort id)
  {
    var frenObj = Instantiate(frenPrefab, spawnPoint.position, spawnPoint.rotation);
    frenObj.transform.parent = transform;

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
        var frenObj = Instantiate(frenPrefab, pos, spawnPoint.rotation);
        frenObj.transform.parent = transform;

        var fren = new Fren() { ID = Convert.ToUInt16(i), go = frenObj, movement = movement };

        frens.Add(fren);
      }
    }
  }

}
