using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEditor.PackageManager;

public class GameController : MonoBehaviour
{
    private static GameController instance;
    public static GameController Instance => instance;

    private void Awake()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            Destroy(gameObject);
        }
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
    private List<(AlienSend, bool)> p1Sends = new List<(AlienSend, bool)> ();
    private List<(AlienSend, bool)> p2Sends = new List<(AlienSend, bool)> ();

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

    int wave = -1;

    private bool busySpawningP1Sends;
    private bool busySpawningP2Sends;

    private bool gameOver = false;

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject modulePrefab;
    public GameObject ModulePrefab => modulePrefab;

    private Player player1;
    private Player player2;

    [SerializeField] private int[] hpPerLevel;
    public int MaxLevel => hpPerLevel.Length - 1;
    public int LevelHP(int index) => hpPerLevel[index];
    // MUST BE THE SAME LENGTH AS HP PER LEVEL - 1
    [SerializeField] private int[] levelCosts;

    // Costs for extra modules
    [SerializeField] private int[] moduleCosts;
    public int MaxModules => moduleCosts.Length + 1;

    [SerializeField] private GameObject[] structures;

    // Contain indices of structures
    // Length 3, determines which structures each player can build
    [SerializeField] private int[] p1Structures = new int[3];
    [SerializeField] private int[] p2Structures = new int[3];

    [SerializeField] private float sellMultiplier;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject player = Instantiate(playerPrefab);
            SpawnPlayer(player.GetComponent<Player>(), clientId);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
        StartGame();
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
            wave++;
            GameMessenger.Instance.TriggerWaveUpdate(wave);
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
            p.PlayerDeathEvent += () => HandlePlayerDeath(1);
            p1ID = id;
            player1 = p;
            playerCount++;
        }
        else if (playerCount == 1)
        {
            world2.PlayerSetup(p);
            p.PlayerDeathEvent += () => HandlePlayerDeath(2);
            p2ID = id;
            player2 = p;
            playerCount++;
        }
        else
        {
            Destroy(p.gameObject);
        }
    }

    // player parameter is the player that died
    public void HandlePlayerDeath(int player)
    {
        if (gameOver) return;
        gameOver = true;
        if (player == 1)
        {
            GameMessenger.Instance.TriggerGameEnd(p1ID, p2ID, 2);
        }
        else
        {
            GameMessenger.Instance.TriggerGameEnd(p1ID, p2ID, 1);
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

        player1.HealthSetup(1, hpPerLevel[0]);
        player2.HealthSetup(2, hpPerLevel[0]);

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
            betweenWaveTimer = maxWaveTimer;
        }
        else
        {
            oneLineDone = true;
        }
        yield return null;
    }

    // Handles player send spawning
    // SHOULD NOT BE CALLED IF QUEUE IS EMPTY
    IEnumerator SpawnSendCoroutine(int player)
    {
        AlienSend send;
        bool front;
        if (player == 1)
        {
            send = p1Sends[0].Item1;
            front = p1Sends[0].Item2;
        }
        else
        {
            send = p2Sends[0].Item1;
            front = p2Sends[0].Item2;
        }
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
        if (player == 1)
        {
            p1Sends.RemoveAt(0);
            busySpawningP1Sends = false;
        }
        else
        {
            p2Sends.RemoveAt(0);
            busySpawningP2Sends = false;
        }
        TrySendQueue(player);
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
        if (send.unlockWave > wave) return false;

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
        TrySendQueue(sender);

        return true;
    }

    // tells the player queue to send (as opposed to checking at every update if the queue is nonempty)
    public void TrySendQueue(int player)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        switch (player)
        {
            case 1:
                if (p1Sends.Count == 0 || busySpawningP1Sends) return;
                busySpawningP1Sends = true;
                StartCoroutine(SpawnSendCoroutine(player));
                break;
            case 2:
                if (p2Sends.Count == 0 || busySpawningP2Sends) return;
                busySpawningP2Sends = true;
                StartCoroutine(SpawnSendCoroutine(player));
                break;
        }
    }

    public bool TryAddModule(ulong clientId, bool right)
    {
        if (!NetworkManager.Singleton.IsServer) return false;
        // get player
        int pNum = 0;
        Player p;
        if (clientId == p1ID)
        {
            pNum = 1;
            p = player1;
        }
        else if (clientId == p2ID)
        {
            pNum = 2;
            p = player2;
        }
        else return false;

        // if already at max
        if (!p.CanAddModule) return false;

        // spends money if possible
        if (!MoneyController.Instance.ChangeMoney(pNum, -moduleCosts[p.ModuleCount - 1])) return false;

        // money is spent
        p.AddModule(right);

        return true;
    }

    public bool TryLevelUp(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return false;
        // get player
        int pNum = 0;
        Player p;
        if (clientId == p1ID)
        {
            pNum = 1;
            p = player1;
        }
        else if (clientId == p2ID)
        {
            pNum = 2;
            p = player2;
        }
        else return false;

        // if already at max
        if (!p.CanLevelUp) return false;

        // spends money if possible
        if (!MoneyController.Instance.ChangeMoney(pNum, -levelCosts[p.Level])) return false;

        // money is spent
        p.LevelUp();

        return true;
    }

    public bool TryBuildStructure(ulong clientId, int module, int structure)
    {
        if (!NetworkManager.Singleton.IsServer) return false;
        // get player
        int pNum = 0;
        Player p;
        int[] s;

        if (clientId == p1ID)
        {
            pNum = 1;
            p = player1;
            s = p1Structures;
        }
        else if (clientId == p2ID)
        {
            pNum = 2;
            p = player2;
            s = p2Structures;
        }
        else return false;

        // check if legal
        Module m = p.GetModule(module);
        if (m == null) return false;
        if (m.ModuleStructure != null) return false;

        // spend money if possible
        GameObject structurePrefab = structures[s[structure]];
        int cost = structurePrefab.GetComponent<Structure>().Cost;
        if (!MoneyController.Instance.ChangeMoney(pNum, -cost)) return false;

        GameObject g = Instantiate(structurePrefab);
        g.GetComponent<NetworkObject>().Spawn();
        g.transform.SetParent(m.transform);
        g.transform.localPosition = Vector3.zero;
        g.GetComponent<Structure>().InitializeValue();
        m.SetStructure(g.GetComponent<Structure>());

        GameMessenger.Instance.UpdateStructure(clientId, m.ModuleStructure.UpgradeInfo);
        return true;
    }

    public bool TryUpgradeStructure(ulong clientId, int module, bool right)
    {
        if (!NetworkManager.Singleton.IsServer) return false;
        // get player
        int pNum = 0;
        Player p;

        if (clientId == p1ID)
        {
            pNum = 1;
            p = player1;
        }
        else if (clientId == p2ID)
        {
            pNum = 2;
            p = player2;
        }
        else return false;

        // check if legal
        Module m = p.GetModule(module);
        if (m == null) return false;
        Structure s = m.ModuleStructure;
        if (s == null || s.UpgradeCount < 1 || right && s.UpgradeCount < 2) return false;

        GameObject upgrade = s.UpgradePrefab(right ? 1 : 0);

        // spend money if possible
        int cost = upgrade.GetComponent<Structure>().Cost;
        if (!MoneyController.Instance.ChangeMoney(pNum, -cost)) return false;

        GameObject g = Instantiate(upgrade);
        g.GetComponent<NetworkObject>().Spawn();
        g.transform.SetParent(m.transform);
        g.transform.localPosition = Vector3.zero;
        g.GetComponent<Structure>().InitializeValue();
        m.Upgrade(g.GetComponent<Structure>());

        GameMessenger.Instance.UpdateStructure(clientId, m.ModuleStructure.UpgradeInfo);
        return true;
    }

    public bool SellStructure(ulong clientId, int module)
    {
        if (!NetworkManager.Singleton.IsServer) return false;
        // get player
        int pNum = 0;
        Player p;

        if (clientId == p1ID)
        {
            pNum = 1;
            p = player1;
        }
        else if (clientId == p2ID)
        {
            pNum = 2;
            p = player2;
        }
        else return false;

        // check if legal
        Module m = p.GetModule(module);
        if (m == null) return false;
        Structure s = m.ModuleStructure;
        if (s == null) return false;

        MoneyController.Instance.ChangeMoney(pNum, Mathf.RoundToInt(sellMultiplier * s.Value));
        m.Sell();

        GameMessenger.Instance.UpdateModule(clientId);
        return true;
    }
}