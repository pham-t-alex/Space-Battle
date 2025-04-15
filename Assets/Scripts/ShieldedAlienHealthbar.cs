using UnityEngine;
using UnityEngine.UI;

public class ShieldedAlienHealthbar : AlienHealthbar
{
    [SerializeField] private Slider shieldSlider;
    public void InitializeShield(int shieldHealth)
    {
        shieldSlider.maxValue = shieldHealth;
        shieldSlider.value = shieldHealth;
    }

    public void UpdateShield(int newHealth)
    {
        if (newHealth > shieldSlider.maxValue) shieldSlider.maxValue = newHealth;
        shieldSlider.value = newHealth;
    }
}