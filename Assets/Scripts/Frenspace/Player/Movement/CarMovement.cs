using UnityEngine;

namespace Frenspace.Player {
  /// <summary>
  /// Car movement implementation
  /// </summary>
  public class CarMovement : Movement {

    override protected Vector3 MovementModifier(Vector3 movement) {

      return movement;
    }

  }
}