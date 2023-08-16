using GameStates;
using UnityEngine;

namespace Frenspace.Fren {

  public class Fren {
    public ushort ID;

    public ushort instanceId;
    public InstanceState instance;

    public MovementState movement;
    public VehicleState vehicle;

    public Fren() { }

    public Fren(Fren f) {
      ID = f.ID;
      instanceId = f.instanceId;
      instance = f.instance;
      movement = f.movement;
      vehicle = f.vehicle;
    }
  }

  public class PositionedFren : Fren {
    public Vector3 position;

    public PositionedFren() : base() { }
    public PositionedFren(Fren f) : base(f) { }
  }

  public class InstancedFren : Fren {
    public GameObject go;

    public InstancedFren() : base() { }
    public InstancedFren(Fren f) : base(f) { }

    public PositionedFren ToPositioned() {
      return new PositionedFren(this) { position = go.transform.position };
    }
  }

}