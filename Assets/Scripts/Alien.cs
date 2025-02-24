using UnityEngine;
using Unity.Netcode;

public class Alien : NetworkBehaviour
{
    [SerializeField] private int health = 5;
    [SerializeField] private int maxHealth = 5;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            health = maxHealth;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Damage(int damage)
    {
        if (IsServer)
        {
            health -= damage;
            if (health < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
