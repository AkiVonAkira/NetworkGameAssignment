using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class ClientPlayerMove : NetworkBehaviour
{
    [SerializeField]
    CharacterController m_CharacterController;
    [SerializeField]
    ThirdPersonController m_ThirdPersonController;
    [SerializeField]
    PlayerInput m_PlayerInput;

    [SerializeField]
    Transform m_CameraFollow;

    void Awake()
    {
        m_CharacterController.enabled = false;
        m_ThirdPersonController.enabled = false;
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
            m_ThirdPersonController.enabled = false;
            m_PlayerInput.enabled = false;
            return;
        }

        // This is ours now
        m_CharacterController.enabled = true;
        m_ThirdPersonController.enabled = true;
        m_PlayerInput.enabled = true;
    }
}
