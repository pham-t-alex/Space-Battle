using UnityEngine;

public class T4RWaveProjectile : WaveProjectile
{
    [SerializeField] private int unarmoredBonus;

    public override void HitAlien(Alien alien)
    {
        alien.Damage(damage, (alien.Armored ? 0 : unarmoredBonus));
    }
}
