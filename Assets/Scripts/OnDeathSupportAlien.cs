using UnityEngine;

public abstract class OnDeathSupportAlien : Alien
{
    public override void Die()
    {
        if (!IsServer) return;
        DeathEffect();
        DieRpc();
        Destroy(gameObject);
    }

    public abstract void DeathEffect();
}
