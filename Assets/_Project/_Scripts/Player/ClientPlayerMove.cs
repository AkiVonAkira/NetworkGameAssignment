using _Project;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClientPlayerMove : NetworkBehaviour
{
    [SerializeField] private CharacterController mCharacterController;

    [SerializeField] private FirstPersonController mFirstPersonController;

    [SerializeField] private PlayerInput mPlayerInput;

    [SerializeField] private Transform mCameraFollow;

    private void Awake()
    {
        mCharacterController.enabled = false;
        mFirstPersonController.enabled = false;
        mPlayerInput.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        enabled = IsClient; // Has to be client

        // If it is not ours
        if (!IsOwner)
        {
            enabled = false;
            mCharacterController.enabled = false;
            mFirstPersonController.enabled = false;
            mPlayerInput.enabled = false;
            return;
        }

        // This is ours now
        mCharacterController.enabled = true;
        mFirstPersonController.enabled = true;
        mPlayerInput.enabled = true;
    }
}
