using UnityEngine;
using Unity.Netcode;
using UnityEditor.Build.Content;
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework.Constraints;

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

    [SerializeField] private AlienSends alienSends;
    private List<(AlienSend, bool)> p1Sends;
    private List<(AlienSend, bool)> p2Sends;

    [SerializeField] private List<Wave> waves = new List<Wave>();
    [SerializeField] private Map map;
    public Map Map => map;
    Vector2 frontLineSpawn = Vector2.zero;
    Vector2 backLineSpawn = Vector2.zero;
    Vector2 frontLineSentSpawn = Vector2.zero;
    Vector2 backLineSentSpawn = Vector2.zero;

    private bool oneLineDone;
    // positive value indicates that a wave has just finished and game is now waiting to spawn the next
    private float betweenWaveTimer;
    [SerializeField] private float maxWaveTimer;

    int wave = 0;

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
        UpdateWaveTimer();
    }

    public void UpdateWaveTimer()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }
        if (betweenWaveTimer == 0)
        {
            return;
        }
        betweenWaveTimer -= Time.deltaTime;
        if (betweenWaveTimer <= 0)
        {
            betweenWaveTimer = 0;
            SpawnWave();
        }
    }

    public void SpawnWave()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }
        if (wave >= waves.Count)
        {
            return;
        }

        StartCoroutine(SpawnWaveCoroutine(waves[wave].backLineComponents, false));
        StartCoroutine(SpawnWaveCoroutine(waves[wave].frontLineComponents, true));
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
    void StartGame()
    {
        // Modify later
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        // Money setup
        MoneyController.Instance.Setup();
        GameMessenger.Instance.GameUISetup(p1ID, p2ID);

        frontLineSpawn = map.frontLinePath[0];
        backLineSpawn = map.backLinePath[0];
        frontLineSentSpawn = map.frontLineSentPath[0];
        backLineSentSpawn = map.backLineSentPath[0];
        betweenWaveTimer = maxWaveTimer;
    }

    // Spawns a wave
    // front is true if frontline, false if backline
    IEnumerator SpawnWaveCoroutine(List<WaveComponent> components, bool front)
    {
        foreach (WaveComponent component in components)
        {
            for (int i = 0; i < component.count; i++)
            {
                SpawnAlien(component.alien, 1, front, false);
                SpawnAlien(component.alien, 2, front, false);
                yield return new WaitForSeconds(component.spawnDelay);
            }
            yield return new WaitForSeconds(component.afterSpawnDelay);
        }
        if (oneLineDone)
        {
            oneLineDone = false;
            wave++;
            betweenWaveTimer = maxWaveTimer;
        }
        else
        {
            oneLineDone = true;
        }
        yield return null;
    }

    // Handles player send spawning
    IEnumerator SpawnSendCoroutine(int player, AlienSend send, bool front)
    {
        yield return new WaitForSeconds(send.delayBeforeSend);
        for (int i = 0; i < send.count; i++)
        {
            if (player == 1) SpawnAlien(send.alien, 2, front, true);
            else SpawnAlien(send.alien, 1, front, true);
            if (i < send.count - 1)
            {
                yield return new WaitForSeconds(send.delayBetweenSends);
            }
        }
        yield return null;
    }

    void SpawnAlien(GameObject alien, int player, bool front, bool sent)
    {
        GameObject g = Instantiate(alien);
        g.GetComponent<NetworkObject>().Spawn();
        if (front && sent) g.transform.position = GetWorldCenter(player) + frontLineSentSpawn;
        else if (!front && sent) g.transform.position = GetWorldCenter(player) + backLineSentSpawn;
        else if (front && !sent) g.transform.position = GetWorldCenter(player) + frontLineSpawn;
        else g.transform.position = GetWorldCenter(player) + backLineSpawn;
        g.GetComponent<Alien>().Initialize(front, sent, player);
    }

    public Vector2 GetWorldCenter(int world)
    {
        switch (world)
        {
            case 1:
                return world1.transform.position;
            case 2:
                return world2.transform.position;
            default:
                return Vector2.zero;
        }
    }

    public bool TrySendAliens(ulong clientId, int sendIndex, bool front)
    {
        if (!NetworkManager.Singleton.IsServer) return false;
        int sender = 0;
        if (clientId == p1ID) sender = 1;
        else if (clientId == p2ID) sender = 2;
        else return false;

        if (sendIndex >= alienSends.sends.Count) return false;
        AlienSend send = alienSends.sends[sendIndex];

        // sender can only be 1 or 2
        if ((sender == 1 && p1Sends.Count >= 5) ||
            p2Sends.Count >= 5) return false;
        // spends money if possible
        if (!MoneyController.Instance.ChangeMoney(sender, -send.cost)) return false;

        // once this point is reached, it's a valid send
        MoneyController.Instance.ChangeIncome(sender, send.incomeChange);
        // add send to queue
        if (sender == 1) p1Sends.Add((send, front));
        else p2Sends.Add((send, front));

        return true;
    }
}