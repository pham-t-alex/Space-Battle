using UnityEngine;

public class BerserkStatus : StatusEffect
{
    private float buff;
    public float Buff => buff;
    public BerserkStatus(float duration, float buff) : base(true, duration)
    {
        this.buff = buff;
    }
}