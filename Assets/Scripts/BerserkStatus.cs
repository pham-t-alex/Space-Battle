using UnityEngine;

public class BerserkStatus : StatusEffect
{
    private float buff;
    public float Buff => buff;
    public BerserkStatus(float duration, float buff) : base(duration)
    {
        this.buff = buff;
    }
}