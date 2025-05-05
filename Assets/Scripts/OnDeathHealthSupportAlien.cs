using UnityEngine;

public class OnDeathHealthSupportAlien : OnDeathSupportAlien
{
    [SerializeField] private float radius;
    [SerializeField] private int heal;
    [SerializeField] private int shield;

    public override void DeathEffect()
    {
        if (!IsServer) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("Alien"));

        foreach (Collider2D hit in hits)
        {
            Alien a = hit.gameObject.GetComponent<Alien>();
            if (heal > 0) a.Heal(heal);
            if (shield > 0) a.AddStatusEffect(new ShieldStatus(shield));
        }
    }
}
