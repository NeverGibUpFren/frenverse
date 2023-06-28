using UnityEngine;

namespace Frenspace.Player
{
  /// <summary>
  /// Car movement implementation
  /// </summary>
  public class CarMovement : Movement
  {

    override protected Vector3 MovementModifier(Vector3 movement)
    {
      if (transform.position.y > 0)
      {
        // fake gravity
        movement.y = -2f;
      }

      return movement;
    }

  }
}