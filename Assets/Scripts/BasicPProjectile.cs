using UnityEngine;
using Unity.Netcode;

public class BasicPProjectile : PlayerProjectile
{
    [SerializeField] private int damage = 0;
    [SerializeField] private float speed = 5;
    private float rotation;

    public override void HitAlien(Alien alien)
    {
        alien.Damage(damage);
        Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            return;
        }
        rb.linearVelocity = Quaternion.Euler(0, 0, rotation) * Vector2.up * speed;
    }

    public void Rotate(float angle)
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
        rotation = angle;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
