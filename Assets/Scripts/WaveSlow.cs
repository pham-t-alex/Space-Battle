using UnityEngine;

public class WaveSlow : StatusEffect
{
    private float multiplier;
    public float Multiplier => multiplier;
    public WaveSlow(float duration, float multiplier) : base(duration)
    {
        this.multiplier = multiplier;
    }
}
