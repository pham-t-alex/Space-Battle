using UnityEngine;
using System;

public abstract class StructureAbility : MonoBehaviour
{
    [SerializeField] private float cooldown;
    [SerializeField] private float initialCooldown;
    private float timeLeft;

    public event Action Ready;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeLeft = initialCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                Ready?.Invoke();
            }
        }
    }

    public virtual void UseAbility()
    {
        ResetCooldown();
    }

    public void ResetCooldown()
    {
        timeLeft = cooldown;
    }
}
