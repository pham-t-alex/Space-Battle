using UnityEngine;
using Unity.Netcode;

public class VoidKingAura : MonoBehaviour
{
    // degrees per second
    [SerializeField] private float curveRate;
    [SerializeField] private float radius;

    private void Awake()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("PlayerAttack"));
        Debug.Log(hits.Length);

        foreach (Collider2D hit in hits)
        {
            PlayerProjectile p = hit.gameObject.GetComponent<PlayerProjectile>();
            if (p == null) return;

            Vector2 optimalDirection = transform.position - p.transform.position;
            float directionAngle = Vector2.SignedAngle(Vector2.right, p.GetComponent<Rigidbody2D>().linearVelocity);
            float angle = Vector2.SignedAngle(Quaternion.Euler(0, 0, directionAngle) * Vector2.right, optimalDirection);
            float warpAngle = 0;
            if (angle > 0)
            {
                warpAngle = Mathf.Clamp(curveRate * Time.deltaTime, 0, angle);
            }
            else if (angle < 0)
            {
                warpAngle = Mathf.Clamp(-curveRate * Time.deltaTime, angle, 0);
            }
            p.GetComponent<Rigidbody2D>().linearVelocity = Quaternion.Euler(0, 0, warpAngle) * p.GetComponent<Rigidbody2D>().linearVelocity;
            p.transform.rotation *= Quaternion.Euler(0, 0, warpAngle);
        }
    }
}