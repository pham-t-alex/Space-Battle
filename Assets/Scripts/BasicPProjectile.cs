using UnityEngine;
using Unity.Netcode;

public class BasicPProjectile : PlayerProjectile
{
    [SerializeField] protected int damage = 0;

    public override void HitAlien(Alien alien)
    {
        alien.Damage(damage, 0);
    }
}
