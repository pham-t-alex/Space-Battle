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
        }
        if (!IsServer)
        {
            Destroy(GetComponent<Rigidbody2D>());
        }
    }
    // -1 pierce indicates infinite pierce
    [SerializeField] private int pierce;
    private HashSet<Alien> hitAliens = new HashSet<Alien>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
}
