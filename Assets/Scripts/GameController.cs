using UnityEngine;
using Unity.Netcode;

public class GameController : MonoBehaviour
{
    private static GameController instance;
    public static GameController Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField] private World world1;
    [SerializeField] private World world2;
    private int playerCount = 0;

    // maybe change later
    [SerializeField] private GameObject alienPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += DestroyOnConnect;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= DestroyOnConnect;
        }
    }

    private void DestroyOnConnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Destroy(gameObject);
        }
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
            playerCount++;
        }
        else if (playerCount == 1)
        {
            world2.PlayerSetup(p);
            playerCount++;
            StartGame();
        }
        else
        {
            Destroy(p.gameObject);
        }
    }

    public void StartGame()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            GameObject g = Instantiate(alienPrefab);
            g.GetComponent<NetworkObject>().Spawn();
            g.transform.position = world1.transform.position + new Vector3(0, 4, 0);
        }
    }
}