using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class ClientNetworkBullet : NetworkBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 570f;
    public int maxBulletsInMagazine = 30;
    public float fireRate = 0.15f;
    private float nextFireTime = 0f;

    private int currentBullets;
    private bool isReloading = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            currentBullets = maxBulletsInMagazine;
        }
    }

    private void Update()
    {
        if (!enabled || !gameObject.activeInHierarchy) return;

        if (IsOwner)
        {
            var playerHealth = GetComponentInParent<PlayerNetworkHealth>();
            if (playerHealth != null && playerHealth.Health <= 0)
            {
                return;
            }

            if (Input.GetButton("Fire1") && currentBullets > 0 && !isReloading && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                ShootServerRpc(firePoint.position, firePoint.rotation);
                currentBullets--;
            }

            if (currentBullets == 0 && !isReloading)
            {
                StartCoroutine(Reload());
            }

            if (Input.GetKeyDown(KeyCode.R) && !isReloading)
            {
                StartCoroutine(Reload());
            }
        }
    }
    [ServerRpc]
    private void ShootServerRpc(Vector3 position, Quaternion rotation)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, rotation);

        NetworkObject networkObject = bullet.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.SpawnWithOwnership(OwnerClientId, true);
        }
        else
        {
            Destroy(bullet);
            return;
        }

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Destroy(bullet);
            return;
        }
        rb.linearVelocity = rotation * Vector3.forward * bulletSpeed;

        StartCoroutine(DestroyBulletAfterTime(bullet, 1f));
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bullet != null && bullet.TryGetComponent(out NetworkObject networkObject) && networkObject.IsSpawned)
        {
            networkObject.Despawn(true);
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(2.5f);
        currentBullets = maxBulletsInMagazine;
        isReloading = false;
    }
}
