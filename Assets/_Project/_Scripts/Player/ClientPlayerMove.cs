using _Project;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClientPlayerMove : NetworkBehaviour
{
    [SerializeField] private CharacterController m_CharacterController;

    [SerializeField] private FPSController m_FPSController;

    [SerializeField] private PlayerInput m_PlayerInput;

    [SerializeField] private Transform m_CameraFollow;

    private void Awake()
    {
        m_CharacterController.enabled = false;
        m_FPSController.enabled = false;
        m_PlayerInput.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        enabled = IsClient; // Has to be client

        // If it is not ours
        if (!IsOwner)
        {
            enabled = false;
            m_CharacterController.enabled = false;
            m_FPSController.enabled = false;
            m_PlayerInput.enabled = false;
            return;
        }

        // This is ours now
        m_CharacterController.enabled = true;
        m_FPSController.enabled = true;
        m_PlayerInput.enabled = true;
    }
}
