using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using GameEvents;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using GameStates;

public class FrenHandler : MonoBehaviour
{
  [BurstCompile]
  public struct MovementJob : IJobParallelForTransform
  {
    [ReadOnly]
    public NativeArray<MovementState> movements;

    [ReadOnly]
    public float deltaTime;

    public void Execute(int i, TransformAccess transform)
    {
      var m = movements[i];
      if (m == MovementState.STOPPED) return;

      var pos = transform.position;
      Quaternion rotation = default;
      Vector3 movement = default;

      switch (m)
      {
        case MovementState.NORTH:
          rotation = Quaternion.AngleAxis(0, Vector3.up);
          movement = Vector3.forward;
          break;
        case MovementState.SOUTH:
          rotation = Quaternion.AngleAxis(180, Vector3.up);
          movement = Vector3.back;
          break;
        case MovementState.WEST:
          rotation = Quaternion.AngleAxis(-90, Vector3.up);
          movement = Vector3.left;
          break;
        case MovementState.EAST:
          rotation = Quaternion.AngleAxis(90, Vector3.up);
          movement = Vector3.right;
          break;
      }

      var speed = 1f;
      pos += speed * movement * deltaTime;

      // TODO: vehicle handling

      if (transform.position.y > 0)
      {
        // fake gravity
        pos -= new Vector3(0, 2f, 0) * deltaTime;
      }

      transform.position = pos;
      transform.rotation = rotation;
    }
  }

  public class Fren
  {
    public ushort ID;
    public GameObject go;

    public ushort instanceId;
    public InstanceState instance;

    public MovementState movement;
    public VehicleState vehicle;


  }

  public GameObject frenPrefab;

  private List<Fren> frens = new List<Fren>();

  private TransformAccessArray m_AccessArray;
  private NativeArray<MovementState> m_MoveArray;
  private JobHandle m_moveJobHandle;

  void Start()
  {
    m_AccessArray = new TransformAccessArray(0);
    m_MoveArray = new NativeArray<MovementState>(0, Allocator.Persistent);
  }

  void Update()
  {
    m_moveJobHandle.Complete();

    var job = new MovementJob() { deltaTime = Time.deltaTime, movements = m_MoveArray };
    m_moveJobHandle = job.Schedule(m_AccessArray);
  }

  void OnDestroy()
  {
    m_moveJobHandle.Complete();
    m_MoveArray.Dispose();
    m_AccessArray.Dispose();
  }

  public void Spawn(Fren fren, bool update = true)
  {
    var frenObj = Instantiate(frenPrefab, fren.go.transform.position, frenPrefab.transform.rotation, transform);
    fren.go = frenObj;

    if (fren.ID < frens.Count)
    {
      frens[fren.ID] = fren;
    }
    else
    {
      frens.Add(fren);
    }

    // TODO: handle fren states

    if (!update) return;
    UpdateJobTransforms();
  }

  void UpdateJobTransforms()
  {
    m_moveJobHandle.Complete();
    m_MoveArray.Dispose();
    m_AccessArray.Dispose();
    m_AccessArray = new TransformAccessArray(frens.Where(f => f != null).Select(f => f.go.transform).ToArray());
    m_MoveArray = new NativeArray<MovementState>(frens.Where(f => f != null).Select(f => f.movement).ToArray(), Allocator.Persistent);
  }

  public void SetMovementState(ushort id, MovementState movement)
  {
    m_moveJobHandle.Complete();

    m_MoveArray[id] = movement;

    frens[id].movement = movement;

    frens[id].go.GetComponent<Animator>().Play(movement == MovementState.STOPPED ? "Idle" : "Run");
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

  public void SetFrens(List<Fren> list)
  {
    foreach (var of in frens)
    {
      Destroy(of.go);
    }

    var highestId = list.Max(f => f.ID); // one other options is just having a list with MAX_CONNECTIONS capacity

    frens = new List<Fren>(highestId);

    foreach (var fren in list)
    {
      Spawn(fren, false);
    }

    UpdateJobTransforms();
  }

}
