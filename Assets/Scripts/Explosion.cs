using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Explosion : NetworkBehaviour
{
    [SerializeField] private float duration;
    private float timer = 0;

    public void Update()
    {
        if (!IsServer) return;
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        timer = 0;
    }

    /*
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!IsServer)
        {
            return;
        }
        if (timer >= explosionDuration)
        {
            Debug.Log("expired");
            return;
        }
        if (!(collision.gameObject.layer == LayerMask.NameToLayer("Alien")))
        {
            Debug.Log("not alien");
            return;
        }
        Debug.Log("hit");
        Alien a = collision.GetComponent<Alien>();
        if (!hitAliens.Add(a)) return;
        Debug.Log("hit alien");
        HitAlien(a);
    }*/

    public abstract void HitAlien(Alien alien);
}
