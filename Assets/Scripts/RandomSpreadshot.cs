using UnityEngine;
using Unity.Netcode;

public class RandomSpreadshot : BasicGun
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
                projectile.GetComponent<PlayerProjectile>().Rotate(Random.Range(-arcRange / 2, arcRange / 2));
                projectile.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
