using UnityEngine;

public class T4LWaveProjectile : T3LWaveProjectile
{
    [SerializeField] private float vulnerabilityDuration;

    public override void HitAlien(Alien alien)
    {
        base.HitAlien(alien);
        alien.AddStatusEffect(new WaveVulnerability(vulnerabilityDuration));
    }
}
