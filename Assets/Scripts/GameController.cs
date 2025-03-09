using UnityEngine;
using Unity.Netcode;
using UnityEditor.Build.Content;

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

    private ulong p1ID = 0;
    private ulong p2ID = 0;

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
    public void SpawnPlayer(Player p, ulong id)
    {
        if (playerCount == 0)
        {
            world1.PlayerSetup(p);
            p1ID = id;
            playerCount++;
        }
        else if (playerCount == 1)
        {
            world2.PlayerSetup(p);
            p2ID = id;
            playerCount++;
            StartGame();
        }
        else
        {
            Destroy(p.gameObject);
        }
    }

    // Called when both players join
    // Starts the game
    public void StartGame()
    {
        // Modify later
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }
        GameObject g = Instantiate(alienPrefab);
        g.GetComponent<NetworkObject>().Spawn();
        g.transform.position = world1.transform.position + new Vector3(0, 4, 0);

        // Money setup
        MoneyController.Instance.Setup();
        GameMessenger.Instance.GameUISetup(p1ID, p2ID);
    }
}