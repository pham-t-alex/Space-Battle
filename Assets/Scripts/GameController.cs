using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using System;

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
    private List<(AlienSend, bool, Modifiers)> p1Sends = new List<(AlienSend, bool, Modifiers)> ();
    private List<(AlienSend, bool, Modifiers)> p2Sends = new List<(AlienSend, bool, Modifiers)> ();

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
    public float SellMultiplier => sellMultiplier;

    // Modifiers
    [SerializeField] private ModifierCostMultipliers modifierCostMultipliers;
    public static float ModifierCostMultiplier(Modifiers modifiers, ModifierCostMultipliers multipliers)
    {
        float multiplier = 1;
        if (modifiers.shielded)
        {
            multiplier *= multipliers.shieldMultiplier;
        }
        if (modifiers.berserk)
        {
            multiplier *= multipliers.berserkMultiplier;
        }
        if (modifiers.invisible)
        {
            multiplier *= multipliers.invisMultiplier;
        }
        if (modifiers.regenerating)
        {
            multiplier *= multipliers.regenMultiplier;
        }
        return multiplier;
    }

    // Boosts (overdrive and shield)
    [SerializeField] private float overdriveDelay;
    private float p1OverdriveDelayTimeLeft;
    private float p2OverdriveDelayTimeLeft;
    [SerializeField] private int overdriveCount;
    private int p1OverdriveCount;
    private int p2OverdriveCount;
    [SerializeField] private float overdriveMultiplier = 1;
    [SerializeField] private float overdriveDuration;
    private float p1OverdriveTimeLeft;
    private float p2OverdriveTimeLeft;
    [SerializeField] private float startingOverdriveDelay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject player = Instantiate(playerPrefab);
            SpawnPlayer(player.GetComponent<Player>(), clientId);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
        for (int i = 0; i < 3; i++)
        {
            p1Structures[i] = StructureSelection.p1Structures[i];
            p2Structures[i] = StructureSelection.p2Structures[i];
        }
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateWaveTimer();
        UpdateOverdriveCooldown();
    }

    void UpdateWaveTimer()
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

    void UpdateOverdriveCooldown()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }
        float time = Time.deltaTime;
        if (p1OverdriveDelayTimeLeft > 0)
        {
            p1OverdriveDelayTimeLeft -= time;
            if (p1OverdriveDelayTimeLeft <= 0)
            {
                p1OverdriveDelayTimeLeft = 0;
                if (p1OverdriveCount > 0) GameMessenger.Instance.TriggerOverdriveReady(p1ID);
            }
        }
        if (p2OverdriveDelayTimeLeft > 0)
        {
            p2OverdriveDelayTimeLeft -= time;
            if (p2OverdriveDelayTimeLeft <= 0)
            {
                p2OverdriveDelayTimeLeft = 0;
                if (p2OverdriveCount > 0) GameMessenger.Instance.TriggerOverdriveReady(p2ID);
            }
        }
        if (p1OverdriveTimeLeft > 0)
        {
            p1OverdriveTimeLeft -= time;
            if (p1OverdriveTimeLeft <= 0)
            {
                p1OverdriveTimeLeft = 0;
                player1.ToggleOverdrive(1);
            }
        }
        if (p2OverdriveTimeLeft > 0)
        {
            p2OverdriveTimeLeft -= time;
            if (p2OverdriveTimeLeft <= 0)
            {
                p2OverdriveTimeLeft = 0;
                player2.ToggleOverdrive(1);
            }
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
        GameMessenger.Instance.GameUISetup(p1ID, p2ID, moduleCosts[0], levelCosts[0],
            structures[p1Structures[0]].GetComponent<Structure>().Info,
            structures[p1Structures[1]].GetComponent<Structure>().Info,
            structures[p1Structures[2]].GetComponent<Structure>().Info,
            structures[p2Structures[0]].GetComponent<Structure>().Info,
            structures[p2Structures[1]].GetComponent<Structure>().Info,
            structures[p2Structures[2]].GetComponent<Structure>().Info);

        // sets up health and player number
        player1.HealthSetup(1, hpPerLevel[0]);
        player2.HealthSetup(2, hpPerLevel[0]);

        frontLineSpawn = map.frontLinePath[0];
        backLineSpawn = map.backLinePath[0];
        frontLineSentSpawn = map.frontLineSentPath[0];
        backLineSentSpawn = map.backLineSentPath[0];
        betweenWaveTimer = maxWaveTimer;

        p1OverdriveCount = overdriveCount;
        p2OverdriveCount = overdriveCount;
        p1OverdriveDelayTimeLeft = startingOverdriveDelay;
        p2OverdriveDelayTimeLeft = startingOverdriveDelay;
    }

    // Spawns a wave
    // front is true if frontline, false if backline
    IEnumerator SpawnWaveCoroutine(List<WaveComponent> components, bool front)
    {
        foreach (WaveComponent component in components)
        {
            for (int i = 0; i < component.count; i++)
            {
                SpawnAlien(component.alien, 1, front, false, component.modifiers);
                SpawnAlien(component.alien, 2, front, false, component.modifiers);
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
        Modifiers modifiers;
        if (player == 1)
        {
            send = p1Sends[0].Item1;
            front = p1Sends[0].Item2;
            modifiers = p1Sends[0].Item3;
        }
        else
        {
            send = p2Sends[0].Item1;
            front = p2Sends[0].Item2;
            modifiers = p2Sends[0].Item3;
        }
        yield return new WaitForSeconds(send.delayBeforeSend);
        for (int i = 0; i < send.count; i++)
        {
            // FIX TO MAKE IT MODIFY ALIENS
            if (player == 1) SpawnAlien(send.alien, 2, front, true, modifiers);
            else SpawnAlien(send.alien, 1, front, true, modifiers);
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

    void SpawnAlien(GameObject alien, int player, bool front, bool sent, Modifiers modifiers)
    {
        GameObject g = Instantiate(alien);
        g.GetComponent<Alien>().PreSpawnServerInitialize();
        g.GetComponent<Alien>().ApplyStartingModifiers(modifiers);
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

    public bool TrySendAliens(ulong clientId, int sendIndex, bool front, Modifiers modifiers)
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
        if (!MoneyController.Instance.ChangeMoney(sender, -(Mathf.RoundToInt(send.cost * ModifierCostMultiplier(modifiers, modifierCostMultipliers))))) return false;

        // once this point is reached, it's a valid send
        float multiplier = sender == 1 ? MoneyController.Instance.P1IncomeMultiplier :
            MoneyController.Instance.P2IncomeMultiplier;
        float incomeChange = (send.incomeChange >= 0) ? send.incomeChange * multiplier :
            send.incomeChange / multiplier;
        MoneyController.Instance.ChangeIncome(sender, incomeChange);
        // add send to queue
        if (sender == 1) p1Sends.Add((send, front, modifiers));
        else p2Sends.Add((send, front, modifiers));
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
        int moduleIndex = p.ModuleCount;
        if (!MoneyController.Instance.ChangeMoney(pNum, -moduleCosts[moduleIndex - 1])) return false;

        // money is spent
        p.AddModule(right);
        if (moduleIndex >= moduleCosts.Length) GameMessenger.Instance.MaxModules(clientId);
        else GameMessenger.Instance.UpdateModuleCost(clientId, moduleCosts[moduleIndex]);
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
        int levelIndex = p.Level;
        if (!MoneyController.Instance.ChangeMoney(pNum, -levelCosts[levelIndex])) return false;

        // money is spent
        p.LevelUp();
        if (levelIndex + 1 >= levelCosts.Length) GameMessenger.Instance.MaxLevel(clientId, levelIndex + 2);
        else GameMessenger.Instance.UpdateLevelCost(clientId, levelCosts[levelIndex + 1], levelIndex + 2);
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
        Structure str = g.GetComponent<Structure>();
        str.InitializePlayer(p);
        str.InitializeValue();
        m.SetStructure(str);
        str.OnBuild();

        GameMessenger.Instance.UpdateStructure(clientId, Mathf.RoundToInt(sellMultiplier * m.ModuleStructure.Value), m.ModuleStructure.UpgradeInfo);
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
        Structure str = g.GetComponent<Structure>();
        str.InitializePlayer(p);
        str.InitializeValue();
        m.ModuleStructure.OnUpgrade();
        m.Upgrade(str);
        str.OnBuild();

        GameMessenger.Instance.UpdateStructure(clientId, Mathf.RoundToInt(sellMultiplier * m.ModuleStructure.Value), m.ModuleStructure.UpgradeInfo);
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
        Structure str = m.ModuleStructure;
        str.OnSell();
        m.Sell();

        GameMessenger.Instance.UpdateModule(clientId);
        return true;
    }

    // this is needed to get the clientID
    // calls: MoneyController -> this -> GameMessenger c
    public void IncomeMultiplierUpdate(int player, float newMultiplier)
    {
        GameMessenger.Instance.TriggerIncomeMultiplierUpdate(player == 1 ? p1ID : p2ID, newMultiplier);
    }

    public void TriggerOverdrive(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;
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
        else return;

        // check if legal
        switch (pNum)
        {
            case 1:
                if (p1OverdriveCount <= 0) return;
                if (p1OverdriveDelayTimeLeft > 0) return;
                break;
            case 2:
                if (p2OverdriveCount <= 0) return;
                if (p2OverdriveDelayTimeLeft > 0) return;
                break;
        }

        switch (pNum)
        {
            case 1:
                p1OverdriveDelayTimeLeft = overdriveDelay;
                p1OverdriveCount--;
                p1OverdriveTimeLeft = overdriveDuration;
                player1.ToggleOverdrive(overdriveMultiplier);
                break;
            case 2:
                p2OverdriveDelayTimeLeft = overdriveDelay;
                p2OverdriveCount--;
                p2OverdriveTimeLeft = overdriveDuration;
                player2.ToggleOverdrive(overdriveMultiplier);
                break;
        }
        GameMessenger.Instance.TriggerOverdriveComplete(clientId);
    }
}