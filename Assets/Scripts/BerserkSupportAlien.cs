using UnityEngine;

public class BerserkSupportAlien : Alien
{
    [SerializeField] private float radius;
    [SerializeField] private int buff;
    [SerializeField] private float duration;

    public override void Shoot()
    {
        if (!IsServer) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("Alien"));

        foreach (Collider2D hit in hits)
        {
            Alien a = hit.gameObject.GetComponent<Alien>();
            a.AddStatusEffect(new BerserkStatus(duration, buff));
        }
    }
}
