using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Frenspace.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerAnimation))]
    public class PlayerMovement : MonoBehaviour
    {
        /// ! --- PLAYER MOVEMENT SCRIPT
        /// This script handles ONLY the Player movement, other scripts will rely on this such as the PlayerAnimation.cs script and similar scripts.
        /// Movement.cs seemed a bit cluttered and a mess, so I remade it.

        [Header("Movement Components")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerAnimation playerAnimation;
        [SerializeField] private Transform playerModel;
        
        [Space(5)]
        
        [Header("Movement Settings")]
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private bool canMove = true;

        [Space(5)]
        [Header("Movement Debug")]
        [SerializeField] bool printLogs = true;
        [SerializeField] bool isMoving = false;
        [SerializeField] private Vector2 playerInput;

        public void OnEnable()
        {
            //Initialize the player and get all components needed.
            this.characterController = this.gameObject.GetComponent<CharacterController>();
            this.playerAnimation = this.GetComponent<PlayerAnimation>();
        }

        public void Update()
        {
            playerInput = CalculateMovementInput();
            
            // Move the player using the Character Controller
            // TODO: Eventually move this to using Networking since this will be a multiplayer game - V
            MovePlayer();
            
            // Rotate playerModel based on input
            RotateModel();
            
            // Animate the model
            AnimatePlayer();
        }

        /// <summary>
        /// Moves the player locally, do not use for networked game.
        /// </summary>
        private void MovePlayer()
        {
            // Used if we want the player to be unable to move in a certain area or action
            if(!canMove){ return; }
            
            // Calculate the motion
            Vector3 playerVelocity;
            playerVelocity = new Vector3(playerInput.x, this.transform.position.y, playerInput.y);

            // Are we moving?
            isMoving = playerInput != Vector2.zero;
            
            // Actually move the player
            characterController.SimpleMove(playerVelocity * movementSpeed);
        }

        /// <summary>
        /// Rotates the player model.
        /// </summary>
        
        /// TODO: This feels like it could be done better, might come back to it - V
        private void RotateModel()
        {
            // So the rotation doesn't snap back to normal if not moving
            if(!isMoving) return;
            
            // Snap angle to direction of character controller
            playerModel.transform.forward = new Vector3(playerInput.x, 0, playerInput.y);
        }

        private void AnimatePlayer()
        {
            // Play movement animation
            playerAnimation.MoveAnimation(isMoving);
        }

        /// <summary>  
        /// Calculate Movement Inputs
        /// </summary>
        private static Vector2 CalculateMovementInput()
        {
            var keyboardInputX = Input.GetAxisRaw("Horizontal");
            var keyboardInputY = Input.GetAxisRaw("Vertical");
            
            return new Vector2(keyboardInputX, keyboardInputY);
        }

    }

}
