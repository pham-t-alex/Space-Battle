using UnityEngine;

public class BasicExplosion : Explosion
{
    [SerializeField] private int damage;

    public override void HitAlien(Alien alien)
    {
        alien.Damage(damage);
    }
}
