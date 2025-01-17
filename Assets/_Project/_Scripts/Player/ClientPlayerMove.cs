using _Project;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project
{
    public class ClientPlayerMove : NetworkBehaviour
    {
        [SerializeField] private CharacterController characterController;
        [SerializeField] private FirstPersonController firstPersonController;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private InputSystem inputSystem;
        // [SerializeField] private Transform playerCamera;

        private void Awake()
        {
            characterController.enabled = false;
            firstPersonController.enabled = false;
            playerInput.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            enabled = IsClient; // Has to be client

            // If it is not ours
            if (!IsOwner)
            {
                enabled = false;
                characterController.enabled = false;
                firstPersonController.enabled = false;
                playerInput.enabled = false;
                return;
            }

            // This is ours now
            characterController.enabled = true;
            firstPersonController.enabled = true;
            playerInput.enabled = true;
        }

        private void Update()
        {
            if (!IsOwner) return;

            // Send movement input to the server
            var moveInput = inputSystem.move;
            var lookInput = inputSystem.look;
            var jumpInput = inputSystem.jump;
            var sprintInput = inputSystem.sprint;
            var crouchInput = inputSystem.crouch;

            SubmitMovementInputServerRpc(moveInput, lookInput, jumpInput, sprintInput, crouchInput);
        }

        [ServerRpc]
        private void SubmitMovementInputServerRpc(Vector2 move, Vector2 look, bool jump, bool sprint, bool crouch)
        {
            firstPersonController.ReceiveInput(move, look, jump, sprint, crouch);
        }
    }
}
