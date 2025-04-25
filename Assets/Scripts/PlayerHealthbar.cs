using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthbar : MonoBehaviour
{
    private Slider slider;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Slider shieldSlider;

    public void Initialize(Player p, int maxHealth)
    {
        slider = GetComponent<Slider>();
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
        text.text = maxHealth.ToString();
        p.HealthChange += UpdateHealth;
        p.MaxHealthChange += UpdateMaxHealth;
        p.PlayerClientDeathEvent += Death;
        p.ShieldChange += UpdateShield;
    }

    public void UpdateHealth(int prev, int newHealth)
    {
        slider.value = newHealth;
        text.text = newHealth.ToString();
    }

    public void UpdateMaxHealth(int prev, int newMaxHealth)
    {
        slider.maxValue = newMaxHealth;
    }

    public void InitializeShield(int value)
    {
        shieldSlider.maxValue = value;
        shieldSlider.value = value;
    }

    public void UpdateShield(int prev, int newShield)
    {
        if (prev == 0 || newShield > shieldSlider.maxValue)
        {
            shieldSlider.maxValue = newShield;
        }
        shieldSlider.value = newShield;
    }

    public void Death()
    {
        slider.value = 0;
        text.text = "0";
        shieldSlider.value = 0;
    }
}
