using Unity.Netcode;
using UnityEngine;

public class BulletBehavior : NetworkBehaviour
{
    public float damageAmount = 30f;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        var playerHealth = other.GetComponent<PlayerNetworkHealth>();
        if (playerHealth != null && other.TryGetComponent(out NetworkObject networkObject))
        {
            if (networkObject.OwnerClientId != OwnerClientId)
            {
                playerHealth.TakeDamageServerRpc(damageAmount);

                if (TryGetComponent(out NetworkObject bulletNetworkObject) && bulletNetworkObject.IsSpawned)
                {
                    bulletNetworkObject.Despawn(true);
                }

                GetComponent<Collider>().enabled = false;
            }
        }
    }
}
