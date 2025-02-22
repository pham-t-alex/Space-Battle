using UnityEngine;

public class BasicPProjectile : PlayerProjectile
{
    [SerializeField] private int damage = 0;
    [SerializeField] private Vector2 direction = Vector2.up;
    [SerializeField] private float speed = 5;

    public override void HitEnemy(Enemy enemy)
    {
        throw new System.NotImplementedException();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            return;
        }
        rb.linearVelocity = direction * speed;
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
