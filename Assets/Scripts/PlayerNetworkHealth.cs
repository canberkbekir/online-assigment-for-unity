using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using StarterAssets;
using UnityEngine.InputSystem;
public class PlayerNetworkHealth : NetworkBehaviour
{
    public NetworkVariable<float> _healthVar = new(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public UnityAction<float> OnHealthUpdate;

    public float Health => _healthVar.Value;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _healthVar.OnValueChanged += OnHealthValueUpdated;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        _healthVar.OnValueChanged -= OnHealthValueUpdated;

    }
    private void OnHealthValueUpdated(float oldValue, float newValue)
    {
        OnHealthUpdate?.Invoke(newValue);
    }
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damageAmount)
    {
        if (!IsServer) return;

        if (_healthVar.Value <= 0) return;

        float newHealth = Mathf.Max(_healthVar.Value - damageAmount, 0);
        _healthVar.Value = newHealth;

        if (_healthVar.Value <= 0)
        {
            HandlePlayerDeath();
        }
    }
    private void HandlePlayerDeath()
    {

        SetPlayerVisibilityAndCollision(false);

        SetPlayerVisibilityAndCollisionClientRpc(false);

        GameManager.Instance.RespawnPlayer(this, 5f);

        SetPlayerComponentsState(false);

        Transform akmObject = transform.Find("AKM");
        if (akmObject != null)
        {
            akmObject.gameObject.SetActive(false);
        }
    }
    [ClientRpc]
    internal void SetPlayerComponentsStateClientRpc(bool state)
    {
        SetPlayerComponentsState(state);
    }

    internal void SetPlayerComponentsState(bool state)
    {
        bool isOwner = IsOwner;

        var playerMove = GetComponent<ClientPlayerMove>();
        if (playerMove != null)
        {
            playerMove.enabled = isOwner && state;
            playerMove.SetComponentsState(isOwner && state);
        }

        var thirdPersonController = GetComponent<ThirdPersonController>();
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = isOwner && state;
        }

        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = isOwner && state;
        }

        var characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = isOwner && state;
        }

        var networkAnimator = GetComponent<ClientNetworkAnimator>();
        if (networkAnimator != null)
        {
            networkAnimator.enabled = state;
        }

        Transform akmObject = transform.Find("AKM");
        if (akmObject != null)
        {
            akmObject.gameObject.SetActive(isOwner && state);
        }
    }
    [ClientRpc]
    internal void SetPlayerVisibilityAndCollisionClientRpc(bool state)
    {
        SetPlayerVisibilityAndCollision(state);
    }

    internal void SetPlayerVisibilityAndCollision(bool state)
    {
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = state;
        }

        var colliders = GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = state;
        }
    }

    internal Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(-10f, 10f);
        float z = Random.Range(-10f, 10f);
        return new Vector3(x, 1f, z);
    }
}
