using UnityEngine;

// MUST BE USED BY HOMINGPROJECTILEALIEN
public class HomingAProjectile : AlienProjectile
{
    [SerializeField] private int damage = 0;
    [SerializeField] private Vector2 direction = Vector2.down;
    [SerializeField] private float speed = 5;
    [SerializeField] private float lifetime;
    private float timeLeft;
    // In degrees per second
    [SerializeField] private Player target;
    [SerializeField] private float curveRate;

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
        rb.linearVelocity = direction * speed;
        timeLeft = lifetime;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                Destroy(gameObject);
            }
        }
        Vector2 optimalDirection = target.transform.position - transform.position;
    }

    public override void HitBorder()
    {
        // do nothing
    }
}
