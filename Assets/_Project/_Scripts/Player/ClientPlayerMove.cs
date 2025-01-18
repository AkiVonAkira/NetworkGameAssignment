using Unity.Cinemachine;
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
        [SerializeField] private CinemachineCamera playerCamera;

        private void Awake()
        {
            firstPersonController.enabled = false;
            playerInput.enabled = false;
            inputSystem.enabled = false;
            playerCamera.enabled = false;
        }

        private void LateUpdate()
        {
            if (!IsOwner) return;

            // Send movement input to the server
            var moveInput = inputSystem.move;
            var lookInput = inputSystem.look;
            var jumpInput = inputSystem.jump;
            var sprintInput = inputSystem.sprint;
            var crouchInput = inputSystem.crouch;

            UpdateInputServerRpc(moveInput, lookInput, jumpInput, sprintInput, crouchInput);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                playerInput.enabled = true;
                inputSystem.enabled = true;
                firstPersonController.enabled = true;
                playerCamera.enabled = true;
                playerCamera.Priority = 10;
                ChatManager.Instance.chatPanel.SetActive(true);
            }
            else
            {
                playerCamera.Priority = 0;
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
    }
}