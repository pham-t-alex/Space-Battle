using UnityEngine;
using System;

public class StatusEffect
{
    private float timeLeft;
    public float TimeLeft => timeLeft;
    public event Action Expire;

    public StatusEffect(float duration)
    {
        timeLeft = duration;
    }

    public virtual void Countdown(float time)
    {
        timeLeft -= time;
        if (timeLeft <= 0)
        {
            Expire?.Invoke();
        }
    }

    public void SetTimeLeft(float time)
    {
        timeLeft = time;
    }
}