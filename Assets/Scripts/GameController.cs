using UnityEngine;
using Unity.Netcode;

public class GameController : NetworkBehaviour
{
    private static GameController instance;
    public static GameController Instance => instance;

    private void Awake()
    {
        if (!IsServer)
        {
            Destroy(gameObject);
        }
    }

    private World world1;
    private World world2;
    private int playerCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlacePlayer(Player p)
    {

    }
}