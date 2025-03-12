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

    [SerializeField] private List<Wave> waves = new List<Wave>();
    [SerializeField] private Map map;
    Vector2 frontLineSpawn = Vector2.zero;
    Vector2 backLineSpawn = Vector2.zero;

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
        UpdateWaveTimer(Time.deltaTime);
    }

    public void UpdateWaveTimer(float deltaTime)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }
        if (betweenWaveTimer == 0)
        {
            return;
        }
        betweenWaveTimer -= deltaTime;
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
                SpawnAlien(component.alien, front);
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

    void SpawnAlien(GameObject alien, bool front)
    {
        GameObject g1 = Instantiate(alien);
        g1.GetComponent<NetworkObject>().Spawn();
        if (front)
        {
            g1.transform.position = world1.transform.position + (Vector3)frontLineSpawn;
        }
        else
        {
            g1.transform.position = world1.transform.position + (Vector3)backLineSpawn;
        }

        GameObject g2 = Instantiate(alien);
        g2.GetComponent<NetworkObject>().Spawn();
        if (front)
        {
            g2.transform.position = world2.transform.position + (Vector3)frontLineSpawn;
        }
        else
        {
            g2.transform.position = world2.transform.position + (Vector3)backLineSpawn;
        }
    }
}