using UnityEngine;
using Unity.Netcode;

public class BasicPProjectile : PlayerProjectile
{
    [SerializeField] private int damage = 0;

    public override void HitAlien(Alien alien)
    {
        alien.Damage(damage);
        Destroy(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
