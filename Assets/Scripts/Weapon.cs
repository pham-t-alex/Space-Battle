using Unity.Netcode;
using UnityEngine;

public abstract class Weapon : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public abstract void Upgrade();
    public abstract void Sell();
}