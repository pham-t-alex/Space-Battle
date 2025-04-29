using UnityEngine;
using Unity.Netcode;

public class AimingAlien : Alien
{
    private Player target;

    public override void Initialize(bool front, bool sent, int world, Player target)
    {
        base.Initialize(front, sent, world, target);
        this.target = target;
    }

    public override void Shoot()
    {
        if (!IsServer)
        {
            return;
        }
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            projectile.GetComponent<BasicAProjectile>().Rotate(VectorToAngle(target.transform.position - transform.position));
            projectile.GetComponent<NetworkObject>().Spawn();
        }
    }

    float VectorToAngle(Vector2 direction)
    {
        return Vector2.SignedAngle(Vector2.down, direction);
    }
}
