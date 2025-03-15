using UnityEngine;
using Unity.Netcode;

public class World : MonoBehaviour
{
    [SerializeField] private float leftBound;
    [SerializeField] private float rightBound;
    [SerializeField] private float topBound;
    [SerializeField] private float bottomBound;

    [SerializeField] private Vector2 playerRelSpawnPos;

    private void Awake()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Only can be called by server
    public void PlayerSetup(Player player)
    {
        player.transform.position = (Vector3)playerRelSpawnPos + transform.position;
        player.SetBounds(leftBound + transform.position.x, rightBound + transform.position.x);
    }
}