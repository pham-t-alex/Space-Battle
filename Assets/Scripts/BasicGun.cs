using UnityEngine;
using Unity.Netcode;

public class BasicGun : Structure
{
    [SerializeField] protected GameObject projectilePrefab;
    [SerializeField] private float reloadTimeLeft;
    [SerializeField] private float reloadTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            reloadTimeLeft = reloadTime;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            if (reloadTimeLeft > 0)
            {
                reloadTimeLeft -= Time.deltaTime;
                if (reloadTimeLeft <= 0)
                {
                    Shoot();
                    reloadTimeLeft = reloadTime;
                }
            }
        }
    }

    public virtual void Shoot()
    {
        if (!IsServer)
        {
            return;
        }
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            projectile.GetComponent<NetworkObject>().Spawn();
        }
    }
}
