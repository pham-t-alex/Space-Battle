using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthbar : MonoBehaviour
{
    private Slider slider;
    [SerializeField] private TMP_Text text;
    public void Initialize(Player p, int maxHealth)
    {
        slider = GetComponent<Slider>();
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
        text.text = maxHealth.ToString();
        p.HealthChange += UpdateHealth;
        p.MaxHealthChange += UpdateMaxHealth;
        p.PlayerClientDeathEvent += Death;
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

    public void Death()
    {
        slider.value = 0;
        text.text = "0";
    }
}
