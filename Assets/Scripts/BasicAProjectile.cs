using UnityEngine;

public class BasicAProjectile : AlienProjectile
{
    [SerializeField] private int damage = 0;
    [SerializeField] private Vector2 direction = Vector2.down;
    [SerializeField] private float speed = 5;
    private float rotation;

    public override void HitAlien(Alien alien)
    {
        
    }

    public override void HitPlayer(Player player)
    {
        player.Damage(damage);
        Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            return;
        }
        rb.linearVelocity = Quaternion.Euler(0, 0, rotation) * direction * speed;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Rotate(float angle)
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
        rotation = angle;
    }

}
