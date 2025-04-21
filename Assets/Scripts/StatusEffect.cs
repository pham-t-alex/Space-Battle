using UnityEngine;
using System;

public class StatusEffect
{
    private bool stackable;
    public bool Stackable => stackable;

    private float duration;
    private float timeLeft;
    public event Action Expire;

    public StatusEffect(bool stackable, float duration)
    {
        this.stackable = stackable;
        this.duration = duration;
        timeLeft = duration;
    }

    public void Countdown(float time)
    {
        timeLeft -= time;
        if (timeLeft <= 0)
        {
            Expire?.Invoke();
        }
    }
}