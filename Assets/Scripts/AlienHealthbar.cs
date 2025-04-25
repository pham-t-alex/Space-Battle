using UnityEngine;
using UnityEngine.UI;

public class AlienHealthbar : FollowObject
{
    private Slider slider;
    public void Initialize(Alien a, int maxHealth)
    {
        transform.SetParent(WorldDisplay.Instance.transform, false);
        InitializeTarget(a.gameObject, new Vector2(0, -a.GetComponent<SpriteRenderer>().bounds.extents.y));
        slider = GetComponent<Slider>();
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
        a.HealthChange += UpdateHealth;
        a.AlienClientDeathEvent += Destroy;
    }

    public void UpdateHealth(int prev, int newHealth)
    {
        slider.value = newHealth;
    }
}