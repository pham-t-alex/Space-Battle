using UnityEngine;
using UnityEngine.UI;

public class AlienHealthbar : MonoBehaviour
{
    private Alien alien;
    private Slider slider;
    private float offset;
    [SerializeField] private float addedOffset = 0f;
    public void Initialize(Alien a, int startingHealth, int maxHealth)
    {
        transform.SetParent(WorldDisplay.Instance.transform, false);
        alien = a;
        slider = GetComponent<Slider>();
        offset = alien.GetComponent<SpriteRenderer>().bounds.extents.y + addedOffset;
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
        a.HealthChange += UpdateHealth;
        a.AlienClientDeathEvent += Destroy;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (alien == null) return;
        transform.position = alien.transform.position + new Vector3(0, -offset);
    }

    public void UpdateHealth(int prev, int newHealth)
    {
        slider.value = newHealth;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}