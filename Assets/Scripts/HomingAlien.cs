using UnityEngine;
using Unity.Netcode;

public class HomingAlien : Alien
{
    private Player target;

    public override void Initialize(bool front, bool sent, int world, Player target)
    {
        base.Initialize(front, sent, world, target);
        this.target = target;
    }

    public override void Shoot()
    {
        if (!IsServer || target == null)
        {
            return;
        }
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            projectile.GetComponent<HomingAProjectile>().Initialize(target);
            projectile.GetComponent<NetworkObject>().Spawn();
        }
    }
}
