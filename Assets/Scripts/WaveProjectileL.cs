using UnityEngine;

public class WaveProjectileL : WaveProjectile
{
    [SerializeField] private float attackRateMultiplier;
    [SerializeField] private float slowDuration;

    public override void HitAlien(Alien alien)
    {
        base.HitAlien(alien);
        alien.AddStatusEffect(new WaveSlow(slowDuration, attackRateMultiplier));
    }
}
