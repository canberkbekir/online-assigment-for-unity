using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RespawnPlayer(PlayerNetworkHealth player, float delay)
    {
        StartCoroutine(RespawnPlayerCoroutine(player, delay));
    }

    private IEnumerator RespawnPlayerCoroutine(PlayerNetworkHealth player, float delay)
    {
        yield return new WaitForSeconds(delay);

        player.transform.position = player.GetRandomSpawnPosition();
        player._healthVar.Value = 100f;

        player.SetPlayerVisibilityAndCollision(true);
        player.SetPlayerVisibilityAndCollisionClientRpc(true);

        player.SetPlayerComponentsState(true);
        player.SetPlayerComponentsStateClientRpc(true);

        Transform akmObject = player.transform.Find("AKM");
        if (akmObject != null)
        {
            akmObject.gameObject.SetActive(true);

            Renderer[] renderers = akmObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = true;
            }
        }
    }
}
