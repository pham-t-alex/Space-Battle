using UnityEngine;
using Unity.Netcode;

public abstract class AlienProjectile : NetworkBehaviour
{
    protected Rigidbody2D rb;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        if (IsClient)
        {
            Destroy(GetComponent<Rigidbody2D>());
        }
    }

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
        if (!IsServer)
        {
            return;
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("Border"))
        {
            HitBorder();
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Alien"))
        {
            //HitAlien(collision.GetComponent<Alien>());
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            HitPlayer(collision.GetComponent<Player>());
        }
    }

    public abstract void HitAlien(Alien alien);
    public abstract void HitPlayer(Player player);

    public virtual void HitBorder()
    {
        Destroy(gameObject);
    }
}
