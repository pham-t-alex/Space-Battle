using UnityEngine;
using Unity.Netcode;

public class World : NetworkBehaviour
{
    [SerializeField] private float leftBound;
    [SerializeField] private float rightBound;
    [SerializeField] private float topBound;
    [SerializeField] private float bottomBound;

    [SerializeField] private Vector2 playerRelSpawnPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Only can be called by server
    public void PlayerSetup(Player player)
    {
        player.transform.parent = transform;
        player.transform.localPosition = playerRelSpawnPos;
        player.SetBounds(leftBound, rightBound);
    }
}
