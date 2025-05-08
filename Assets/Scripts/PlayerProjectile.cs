using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public abstract class PlayerProjectile : NetworkBehaviour
{
    protected Rigidbody2D rb;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            rb = GetComponent<Rigidbody2D>();
            rb.linearVelocity = Quaternion.Euler(0, 0, rotation) * Vector2.up * speed;
            timeLeft = lifetime;
        }
        if (!IsServer)
        {
            Destroy(GetComponent<Rigidbody2D>());
        }
    }
    // -1 pierce indicates infinite pierce
    [SerializeField] private int pierce;
    private HashSet<Alien> hitAliens = new HashSet<Alien>();
    [SerializeField] private float speed = 5;
    private float rotation;

    [SerializeField] private float lifetime = 10;
    private float timeLeft;

    // whether projectile can be interfered with (e.g. void king aura)
    [SerializeField] private bool interferable = true;
    public bool Interferable => interferable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        ServerUpdate();
    }

    protected virtual void ServerUpdate()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer || (pierce != -1 && hitAliens.Count >= pierce))
        {
            return;
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("Alien"))
        {
            Alien a = collision.GetComponent<Alien>();
            if (!hitAliens.Add(a)) return;
            HitAlien(a);
            if (pierce != -1 && hitAliens.Count >= pierce)
            {
                Destroy(gameObject);
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Border"))
        {
            HitBorder();
        }
    }

    public abstract void HitAlien(Alien alien);
    public virtual void HitBorder()
    {
        Destroy(gameObject);
    }

    public void Rotate(float angle)
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
        rotation = angle;
    }
}
