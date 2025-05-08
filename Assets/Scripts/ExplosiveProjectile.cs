using Unity.Netcode;
using UnityEngine;

public class ExplosiveProjectile : PlayerProjectile
{
    [SerializeField] protected int damage;
    [SerializeField] protected float radius;

    public override void HitAlien(Alien alien)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("Alien"));

        foreach (Collider2D hit in hits)
        {
            Alien a = hit.gameObject.GetComponent<Alien>();
            a.Damage(damage, 0);
        }
        CreateClientExplosionRpc(transform.position, radius, default);
    }

    [Rpc(SendTo.ClientsAndHost)]
    protected void CreateClientExplosionRpc(Vector2 position, float radius, RpcParams rpcParams)
    {
        GameObject g = Instantiate(ClientPrefabs.Instance.ExplosionPrefab, position, Quaternion.identity);
        g.transform.localScale = new Vector2(radius * 2, radius * 2);
        Destroy(g, 0.2f);
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
