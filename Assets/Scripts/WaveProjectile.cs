using UnityEngine;

public class WaveProjectile : BasicPProjectile
{
    // Units: scale per second
    [SerializeField] private float expansionRate = 0;
    private Vector3 baseScale;
    private float baseMultiplier = 1;

    public override void HitBorder()
    {
        
    }

    protected override void ServerUpdate()
    {
        base.ServerUpdate();
        if (baseScale == Vector3.zero) return;
        baseMultiplier += expansionRate * Time.deltaTime;
        transform.localScale = new Vector3(baseScale.x * baseMultiplier, baseScale.y, baseScale.z);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        baseScale = transform.localScale;
    }
}
