using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerNetworkEmojiController : NetworkBehaviour
{
    public GameObject emojiPrefab;
    private GameObject currentEmojiInstance;
    private bool isEmojiActive = false;
    private float emojiCooldown = 2f;
    private float lastEmojiTime = -2f;

    private void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.H) && Time.time - lastEmojiTime >= emojiCooldown)
            {
                ShowEmojiServerRpc();
            }
        }
    }

    [ServerRpc]
    private void ShowEmojiServerRpc()
    {
        if (isEmojiActive) return;

        Vector3 emojiPosition = transform.position + Vector3.up * 1.6f;
        Quaternion emojiRotation = transform.rotation;

        currentEmojiInstance = Instantiate(emojiPrefab, emojiPosition, emojiRotation);

        NetworkObject emojiNetworkObject = currentEmojiInstance.GetComponent<NetworkObject>();
        emojiNetworkObject.Spawn(true);

        currentEmojiInstance.transform.SetParent(transform);

        StartCoroutine(HideEmojiAfterDelay(2f));

        lastEmojiTime = Time.time;
        isEmojiActive = true;

        ShowEmojiClientRpc(emojiNetworkObject.NetworkObjectId, emojiRotation);
    }

    [ClientRpc]
    private void ShowEmojiClientRpc(ulong emojiNetworkObjectId, Quaternion emojiRotation)
    {
        if (!IsOwner)
        {
            NetworkObject emojiNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[emojiNetworkObjectId];
            currentEmojiInstance = emojiNetworkObject.gameObject;

            currentEmojiInstance.transform.SetParent(transform);

            currentEmojiInstance.transform.rotation = emojiRotation;
        }
    }

    private IEnumerator HideEmojiAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentEmojiInstance != null)
        {
            currentEmojiInstance.GetComponent<NetworkObject>().Despawn(true);
            Destroy(currentEmojiInstance);
        }

        isEmojiActive = false;
    }
}
