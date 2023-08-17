using UnityEngine;

namespace Frenspace.Player {
  /// <summary>
  /// Player movement implementation
  /// </summary>
  public class PlayerMovement : Movement {
    override protected Vector3 MovementModifier(Vector3 movement) {
      // Don't fake gravity at all for now 

      // Debug.DrawLine(transform.position, transform.position + Vector3.down * 0.004f, Color.red);
      // if (transform.position.y > 0.003f && !Physics.Raycast(transform.position, Vector3.down, 0.004f)) {
      //   movement.y -= 2f;
      // }

      return movement;
    }
  }

}