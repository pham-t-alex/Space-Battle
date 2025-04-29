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
            a.Heal(heal);
            a.AddStatusEffect(new ShieldStatus(shield));
        }
    }
}
