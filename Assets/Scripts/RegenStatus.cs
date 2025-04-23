using UnityEngine;
using System;

public class RegenStatus : StatusEffect
{
    // Units: full healthbars per minute
    [SerializeField] private float healRate = 1;
    [SerializeField] private float maxHealth;
    private float healDelay = 0f;
    private float maxHealDelay => 60f / (maxHealth * healRate);
    public event Action Heal;

    public RegenStatus(float healRate, float maxHealth) : base(Mathf.Infinity)
    {
        this.healRate = healRate;
        this.maxHealth = maxHealth;
        healDelay = maxHealDelay;
    }

    public override void Countdown(float time)
    {
        base.Countdown(time);
        if (healDelay > 0)
        {
            healDelay -= time;
            if (healDelay <= 0)
            {
                healDelay = maxHealDelay;
                Heal?.Invoke();
            }
        }
    }
}
