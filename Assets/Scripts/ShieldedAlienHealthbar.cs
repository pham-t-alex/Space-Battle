using UnityEngine;
using UnityEngine.UI;

public class ShieldedAlienHealthbar : FollowObject
{
    [SerializeField] private Slider shieldSlider;

    public void Initialize(Alien a, int shieldHealth)
    {
        transform.SetParent(WorldDisplay.Instance.transform, false);
        InitializeTarget(a.gameObject, new Vector2(0, -a.GetComponent<SpriteRenderer>().bounds.extents.y));
        shieldSlider.maxValue = shieldHealth;
        shieldSlider.value = shieldHealth;
        a.AlienClientDeathEvent += Destroy;
    }

    public void UpdateShield(int newHealth)
    {
        if (newHealth > shieldSlider.maxValue) shieldSlider.maxValue = newHealth;
        shieldSlider.value = newHealth;
    }
}