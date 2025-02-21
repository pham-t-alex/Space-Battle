using UnityEngine;
using Unity.Netcode;

public class GameController : MonoBehaviour
{
    private static GameController instance;
    public static GameController Instance => instance;

    private void Awake()
    {
        if (instance != null) // CHANGE TO REMOVE CLIENT
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    [SerializeField] private World world1;
    [SerializeField] private World world2;
    private int playerCount = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Only can be called by server
    public void SpawnPlayer(Player p)
    {
        if (playerCount == 0)
        {
            world1.PlayerSetup(p);
        }
        else if (playerCount == 1)
        {
            world2.PlayerSetup(p);
        }
        else
        {
            Destroy(p.gameObject);
        }
    }
}