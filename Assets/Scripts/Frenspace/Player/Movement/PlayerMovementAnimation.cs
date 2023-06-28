using UnityEngine;

namespace Frenspace.Player
{
  public class PlayerMovementAnimation : MovementAnimation
  {
    override public void Animate(Vector3 movement)
    {
      if (movement.x == 0f && movement.z == 0f)
      {
        animator.Play("Idle");
      }
      else
      {
        animator.Play("Run");
      }
    }
  }

}