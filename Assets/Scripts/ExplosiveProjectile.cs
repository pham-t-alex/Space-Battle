using Unity.Netcode;
using UnityEngine;

public class ExplosiveProjectile : PlayerProjectile
{
    [SerializeField] private int damage;
    [SerializeField] private float radius;
    [SerializeField] private float speed = 5;
    private float rotation;

    public override void HitAlien(Alien alien)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.NameToLayer("Alien"));
        Debug.Log("cast");

        foreach (Collider2D hit in hits)
        {
            Debug.Log(LayerMask.LayerToName(hit.gameObject.layer));
            Debug.Log("hit");
            Alien a = hit.gameObject.GetComponent<Alien>();
            a.Damage(damage);
        }
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
