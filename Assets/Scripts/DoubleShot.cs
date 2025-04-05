using Unity.Netcode;
using UnityEngine;

public class DoubleShot : BasicGun
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
            GameObject projectile = Instantiate(projectilePrefab, transform.position + Vector3.left * (bulletCenterGap / 2), Quaternion.identity);
            projectile.GetComponent<NetworkObject>().Spawn();
            projectile = Instantiate(projectilePrefab, transform.position + Vector3.right * (bulletCenterGap / 2), Quaternion.identity);
            projectile.GetComponent<NetworkObject>().Spawn();
        }
    }
}
