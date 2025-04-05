using Unity.Netcode;
using UnityEngine;

public class TripleShot : BasicGun
{
    [SerializeField] private float bulletCenterGap;
    public override void Shoot()
    {
        if (!IsServer)
        {
            return;
        }
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, transform.position + Vector3.left * bulletCenterGap, Quaternion.identity);
            projectile.GetComponent<NetworkObject>().Spawn();
            projectile = Instantiate(projectilePrefab, transform.position + Vector3.right * bulletCenterGap, Quaternion.identity);
            projectile.GetComponent<NetworkObject>().Spawn();
            projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            projectile.GetComponent<NetworkObject>().Spawn();
        }
    }
}
