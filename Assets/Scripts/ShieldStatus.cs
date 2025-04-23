using System;
using UnityEngine;

public class ShieldStatus : StatusEffect
{
    private int health;
    public int Health => health;
    public event Action<int> HealthChanged;
    public event Action ShieldBroken;
    public ShieldStatus(int health) : base(Mathf.Infinity)
    {
        this.health = health;
    }

    public void Damage(int damage)
    {
        health = Mathf.Max(health - damage, 0);
        HealthChanged?.Invoke(health);
        if (health == 0)
        {
            ShieldBroken?.Invoke();
        }
    }

    public void AddHealth(int health)
    {
        this.health += health;
        HealthChanged?.Invoke(this.health);
    }
}