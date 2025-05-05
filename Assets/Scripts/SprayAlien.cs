using UnityEngine;
using Unity.Netcode;

public class SprayAlien : Alien
{
    [SerializeField] private float arc;

    public override void Shoot()
    {
        if (!IsServer)
        {
            return;
        }
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            projectile.GetComponent<BasicAProjectile>().Rotate(Random.Range(-arc / 2, arc / 2));
            projectile.GetComponent<NetworkObject>().Spawn();
        }
    }
}
