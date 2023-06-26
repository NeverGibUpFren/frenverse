using System;
using UnityEngine;

namespace Frenspace.Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private const string idleAnim = "Idle";
        private const string runAnim = "Run";

        public void MoveAnimation(bool value)
        {
            // Check which movement animation to play
            string curAnimation = !value ? idleAnim : runAnim;
            animator.Play(curAnimation);
        }
    }
}