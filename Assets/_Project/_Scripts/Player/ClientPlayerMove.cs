using _Project;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClientPlayerMove : NetworkBehaviour
{
    [SerializeField] private CharacterController characterController;

    [SerializeField] private FirstPersonController firstPersonController;

    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private Transform playerCamera;

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
}
