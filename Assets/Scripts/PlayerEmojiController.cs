using Unity.Netcode;
using UnityEngine;
using System.Collections;
public class PlayerEmojiController : NetworkBehaviour
{
    public GameObject emojiObject;
    private bool isEmojiActive = false;
    private float emojiCooldown = 2f;
    private float lastEmojiTime = -2f;

    private NetworkVariable<bool> isEmojiVisible = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        isEmojiVisible.OnValueChanged += OnEmojiVisibilityChanged;

        UpdateEmojiVisibility(isEmojiVisible.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        isEmojiVisible.OnValueChanged -= OnEmojiVisibilityChanged;
    }

    private void OnEmojiVisibilityChanged(bool oldValue, bool newValue)
    {
        UpdateEmojiVisibility(newValue);
    }

    private void UpdateEmojiVisibility(bool isVisible)
    {
        emojiObject.SetActive(isVisible);

        Renderer[] renderers = emojiObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = isVisible;
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.H) && Time.time - lastEmojiTime >= emojiCooldown)
            {
                ToggleEmojiServerRpc();
            }
        }
    }

    [ServerRpc]
    private void ToggleEmojiServerRpc()
    {
        if (isEmojiActive) return;

        isEmojiVisible.Value = true;

        StartCoroutine(HideEmojiAfterDelay(2f));

        lastEmojiTime = Time.time;
        isEmojiActive = true;
    }

    private IEnumerator HideEmojiAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        isEmojiVisible.Value = false;
        isEmojiActive = false;
    }
}