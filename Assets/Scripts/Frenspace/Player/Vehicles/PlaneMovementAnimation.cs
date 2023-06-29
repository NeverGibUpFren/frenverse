using UnityEngine;

namespace Frenspace.Player
{
  public class PlaneMovementAnimation : MovementAnimation
  {
    override public void Animate(Vector3 movement)
    {
      if (transform.position.y > 0.01f)
      {
        animator.Play("Fly");
      }
      else
      {
        animator.Play("Idle");
      }
    }
  }

}