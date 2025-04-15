using UnityEngine;

public class FollowObject : MonoBehaviour
{
    private GameObject target;
    private Vector2 offset;
    [SerializeField] private Vector2 addedOffset = Vector2.zero;

    public void InitializeTarget(GameObject target, Vector2 offset)
    {
        this.target = target;
        this.offset = offset + addedOffset;
    }

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.transform.position + new Vector3(offset.x, offset.y);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
