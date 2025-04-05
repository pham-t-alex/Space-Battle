using UnityEngine;

public class FlyawayText : MonoBehaviour
{
    [SerializeField] private float flyDuration;
    [SerializeField] private float duration;
    private Vector2 startPosition;
    [SerializeField] private Vector2 endRelPosition;
    private float currentTime = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // client only
    public void Initialize(Vector2 position)
    {
        transform.SetParent(WorldDisplay.Instance.transform, false);
        startPosition = position;
        transform.position = position;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        float flyTime = Mathf.Clamp(currentTime / flyDuration, 0, 1);
        // quadratic interpolation from 0 to 1; decelerates toward the end
        float interpTime = flyTime * (2 - flyTime);
        transform.position = Vector3.Lerp(startPosition, startPosition + endRelPosition, interpTime);
        if (currentTime >= duration)
        {
            Destroy(gameObject);
        }
    }
}
