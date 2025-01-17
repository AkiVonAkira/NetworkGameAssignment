using System;
using _Project;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project
{
    public class ClientPlayerMove : NetworkBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private InputSystem inputSystem;
        [SerializeField] private FirstPersonController firstPersonController;

        private void Awake()
        {
            firstPersonController.enabled = false;
            playerInput.enabled = false;
            inputSystem.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                playerInput.enabled = true;
                inputSystem.enabled = true;
            }

            if (IsServer)
            {
                firstPersonController.enabled = true;
            }
        }

        [Rpc(SendTo.Server)]
        private void UpdateInputServerRpc(Vector2 move, Vector2 look, bool jump, bool sprint, bool crouch)
        {
            inputSystem.MoveInput(move);
            inputSystem.LookInput(look);
            inputSystem.JumpInput(jump);
            inputSystem.SprintInput(sprint);
            inputSystem.CrouchInput(crouch);
            //firstPersonController.ReceiveInput(move, look, jump, sprint, crouch);
        }

        private void LateUpdate()
        {
            if (!IsOwner)  return;

            // Send movement input to the server
            var moveInput = inputSystem.move;
            var lookInput = inputSystem.look;
            var jumpInput = inputSystem.jump;
            var sprintInput = inputSystem.sprint;
            var crouchInput = inputSystem.crouch;

            UpdateInputServerRpc(moveInput, lookInput, jumpInput, sprintInput, crouchInput);
        }
    }
}
