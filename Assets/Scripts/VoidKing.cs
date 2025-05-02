using UnityEngine;

public class VoidKing : Alien
{
    [SerializeField] private GameObject serverAura;

    protected override void ServerSpawn()
    {
        base.ServerSpawn();
        GameObject g = Instantiate(serverAura, transform);
    }
}