using UnityEngine;

namespace Frenspace.Player
{
  /// <summary>
  /// Plane movement implementation
  /// </summary>
  public class PlaneMovement : Movement
  {
    public float speedY = 0.5f;

    public float maxElevation = 8.1f;
    public float minElevation = 0.3f;

    override protected void Start()
    {
      keys = new KeyCode[] { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.LeftShift, KeyCode.LeftAlt };
      base.Start();
    }

    override protected Quaternion RotationModifier(Quaternion rotation)
    {
      if (transform.position.y < minElevation)
      {
        // prevent rotating when plane is not hovering
        return transform.rotation;
      }
      return rotation;
    }

    override protected Vector3 MovementModifier(Vector3 movement)
    {
      if (keysPressed.Count > 0)
      {
        switch (keysPressed[keysPressed.Count - 1])
        {
          case KeyCode.LeftShift:
            movement.y += speedY;
            break;
          case KeyCode.LeftAlt:
            movement.y -= speedY;
            break;
        }

        if (transform.position.y >= maxElevation)
        {
          // sky limit
          movement.y = 0f;
        }
      }

      if (transform.position.y < minElevation)
      {
        // prevent moving when plane is not hovering
        movement.x = 0;
        movement.z = 0;
      }

      bool moving = (movement.x + movement.y) > 0f ? true : false;
      if (moving)
      {
        // TODO: rotate when moving
        // https://docs.unity3d.com/ScriptReference/Quaternion.Lerp.html
      }

      return movement;
    }
  }
}