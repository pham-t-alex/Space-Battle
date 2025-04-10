using System.Collections.Generic;
using UnityEngine;

public class T4LRocket : ExplosiveProjectile
{
    [SerializeField] private int innerDamageBonus;
    [SerializeField] private float innerRadius;
    private HashSet<Alien> innerAliens = new HashSet<Alien>();

    public override void HitAlien(Alien alien)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, innerRadius, LayerMask.GetMask("Alien"));

        foreach (Collider2D hit in hits)
        {
            Alien a = hit.gameObject.GetComponent<Alien>();
            innerAliens.Add(a);
            a.Damage(innerDamageBonus + damage);
        }

        hits = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("Alien"));

        foreach (Collider2D hit in hits)
        {
            Alien a = hit.gameObject.GetComponent<Alien>();
            if (innerAliens.Contains(a)) return;
            a.Damage(damage);
        }

        CreateClientExplosionRpc(transform.position, radius, default);
    }
}
