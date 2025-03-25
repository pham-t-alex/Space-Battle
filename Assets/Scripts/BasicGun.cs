using UnityEngine;
using Unity.Netcode;

public class BasicGun : Structure
{
    [SerializeField] private GameObject projectilePrefab;
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

    public void Shoot()
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

    public override void Sell()
    {
        throw new System.NotImplementedException();
    }

    public override void Upgrade()
    {
        throw new System.NotImplementedException();
    }
}
