using UnityEngine;

// MUST BE USED BY HOMINGPROJECTILEALIEN
public class HomingAProjectile : AlienProjectile
{
    [SerializeField] private int damage = 0;
    [SerializeField] private float directionAngle = -90;
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
        rb.linearVelocity = Quaternion.Euler(0, 0, directionAngle) * Vector2.right * speed;
        timeLeft = lifetime;
    }

    public void Initialize(Player target)
    {
        this.target = target;
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
        float angle = Vector2.SignedAngle(Quaternion.Euler(0, 0, directionAngle) * Vector2.right, optimalDirection);
        if (angle > 0)
        {
            directionAngle = Mathf.Clamp(directionAngle + (curveRate * Time.deltaTime), directionAngle, directionAngle + angle);
        }
        else if (angle < 0)
        {
            directionAngle = Mathf.Clamp(directionAngle - (curveRate * Time.deltaTime), directionAngle - angle, directionAngle);
        }
        rb.linearVelocity = Quaternion.Euler(0, 0, directionAngle) * Vector2.right * speed;
    }

    public override void HitBorder()
    {
        // do nothing
    }
}
