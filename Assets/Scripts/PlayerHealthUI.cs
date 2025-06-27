using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealthUI : MonoBehaviour
{
    public TMP_Text healthText;

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

    void OnClientConnected(ulong clientId)
    {
        if (IsLocalClient(clientId))
        {
            NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            PlayerNetworkHealth playerNetworkHealth = playerNetworkObject.GetComponent<PlayerNetworkHealth>();

            if (playerNetworkHealth)
            {
                OnPlayerHealthUpdated(playerNetworkHealth.Health);
                playerNetworkHealth.OnHealthUpdate += OnPlayerHealthUpdated;
            }
        }
    }

    void OnClientDisconnect(ulong clientId)
    {
        if (IsLocalClient(clientId))
        {
            OnPlayerHealthUpdated(-1f);
        }
    }

    private void OnPlayerHealthUpdated(float newHealthValue)
    {
        healthText.text = "Health: " + newHealthValue;
    }

    private bool IsLocalClient(ulong clientId)
    {
        return NetworkManager.Singleton.LocalClientId == clientId;
    }
}
