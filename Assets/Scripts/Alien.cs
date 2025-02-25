using UnityEngine;
using Unity.Netcode;

public class Alien : NetworkBehaviour
{
    private NetworkVariable<int> health = new NetworkVariable<int>();
    [SerializeField] private int maxHealth = 5;

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
            health.Value = maxHealth;
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

    public void Damage(int damage)
    {
        if (IsServer)
        {
            health.Value -= damage;
            if (health.Value < 0)
            {
                Destroy(gameObject);
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
}
