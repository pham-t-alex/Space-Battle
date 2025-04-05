using Unity.Netcode;
using UnityEngine;

public class SpreadShot : BasicGun
{
    [SerializeField] private int bulletCount;
    [SerializeField] private float arcRange;
    public override void Shoot()
    {
        if (!IsServer)
        {
            return;
        }
        if (projectilePrefab != null)
        {
            for (int i = 0; i < bulletCount; i++)
            {
                GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                projectile.GetComponent<BasicPProjectile>().Rotate(-(arcRange / 2) + i * (arcRange / (bulletCount - 1)));
                projectile.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
