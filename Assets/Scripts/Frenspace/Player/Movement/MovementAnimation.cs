using UnityEngine;

namespace Frenspace.Player
{
  /// <summary>
  /// Handles an abitrary movement animation
  /// </summary>
  [RequireComponent(typeof(Animator))]
  abstract public class MovementAnimation : MonoBehaviour
  {
    protected Animator animator;

    void Start()
    {
      animator = gameObject.GetComponent<Animator>();
    }

    abstract public void Animate(Vector3 movement);
  }
}