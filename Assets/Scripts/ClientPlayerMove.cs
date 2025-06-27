using StarterAssets;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

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


    private void Awake()
    {
        m_CharacterController.enabled = false;
        m_ThirdPersonController.enabled = false;
        m_PlayerInput.enabled = false;

    }
    public void SetComponentsState(bool state)
    {
        bool isOwner = IsOwner;

        m_CharacterController.enabled = isOwner && state;
        m_ThirdPersonController.enabled = isOwner && state;
        m_PlayerInput.enabled = isOwner && state;

        var networkAnimator = GetComponent<ClientNetworkAnimator>();
        if (networkAnimator != null)
        {
            networkAnimator.enabled = state;
        }
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        enabled = IsClient;

        if (!IsOwner)
        {
            enabled = false;
            m_CharacterController.enabled = false;
            m_ThirdPersonController.enabled = false;
            m_PlayerInput.enabled = false;
            return;
        }

        m_CharacterController.enabled = true;
        m_ThirdPersonController.enabled = true;
        m_PlayerInput.enabled = true;

        var virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
        virtualCamera.Follow = m_CameraFollow;
    }
}


