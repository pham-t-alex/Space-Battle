using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;

public class Alien : NetworkBehaviour
{
    [SerializeField] private string alienName;
    public string AlienName => alienName;

    private NetworkVariable<int> health = new NetworkVariable<int>();
    [SerializeField] private int maxHealth;

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float reloadTimeLeft;
    [SerializeField] private float reloadTime;

    [SerializeField] private float moveSpeed;
    private bool frontLine;
    private bool sent;
    private int mapIndex;
    private Vector2 targetPosition;
    private bool finishedPath = false;
    private int world = 0;

    public event Action<int, int> HealthChange;
    public event Action AlienClientDeathEvent;

    // Modifiers / Traits
    [Header("Modifiers")]
    [SerializeField] private int armor = 0;
    public bool Armored => armor > 0;
    private Dictionary<Type, List<StatusEffect>> statusEffects = new Dictionary<Type, List<StatusEffect>>();

    event Action<float> StatusTimeUpdate;

    // Client side shield healthbar
    private ShieldedAlienHealthbar shieldBar;
    private GameObject shieldDisplay;

    public int ShieldHealth => statusEffects.ContainsKey(typeof(ShieldStatus)) && statusEffects[typeof(ShieldStatus)].Count > 0
        ? ((ShieldStatus)statusEffects[typeof(ShieldStatus)][0]).Health : 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public override void OnNetworkSpawn()
    {
        health.OnValueChanged += (oldVal, newVal) => HealthChange?.Invoke(oldVal, newVal);
        if (IsServer)
        {
            ServerSpawn();
        }
        if (IsClient)
        {
            ClientSpawn();
        }
    }

    void ClientSpawn()
    {
        AlienHealthbar healthbar = Instantiate(ClientPrefabs.Instance.AlienHealthbarPrefab).GetComponent<AlienHealthbar>();
        healthbar.Initialize(this, maxHealth);
    }

    void ServerSpawn()
    {
        health.Value = maxHealth;
        reloadTimeLeft = UnityEngine.Random.Range(reloadTime / 2, reloadTime);
    }

    public void Initialize(bool front, bool sent, int world)
    {
        this.world = world;
        frontLine = front;
        this.sent = sent;
        mapIndex = 1;
        finishedPath = false;
        GetNextMapPosition();
    }

    void GetNextMapPosition()
    {
        if (finishedPath) return;
        List<Vector2> path;
        if (frontLine && sent) path = GameController.Instance.Map.frontLineSentPath;
        else if (!frontLine && sent) path = GameController.Instance.Map.backLineSentPath;
        else if (frontLine && !sent) path = GameController.Instance.Map.frontLinePath;
        else path = GameController.Instance.Map.backLinePath;

        if (mapIndex < path.Count)
        {
            targetPosition = path[mapIndex] + GameController.Instance.GetWorldCenter(world);
            return;
        }

        finishedPath = true;
        List<MapEndRegion> ends;
        if (frontLine && sent) ends = GameController.Instance.Map.frontLineSentEnds;
        else if (!frontLine && sent) ends = GameController.Instance.Map.backLineSentEnds;
        else if (frontLine && !sent) ends = GameController.Instance.Map.frontLineEnds;
        else ends = GameController.Instance.Map.backLineEnds;

        MapEndRegion region = ends[UnityEngine.Random.Range(0, ends.Count)];
        Vector2 regionExtents = region.bounds / 2f;
        // the region's relative center + some random x and y + world center
        targetPosition = region.center + new Vector2(UnityEngine.Random.Range(-regionExtents.x, regionExtents.x), UnityEngine.Random.Range(-regionExtents.y, regionExtents.y)) + GameController.Instance.GetWorldCenter(world);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
        {
            return;
        }
        UpdateShoot();
        UpdateMove();
        StatusTimeUpdate?.Invoke(Time.deltaTime);
    }

    public void UpdateShoot()
    {
        if (reloadTimeLeft == 0) return;
        reloadTimeLeft -= Time.deltaTime;
        if (reloadTimeLeft > 0) return;
        Shoot();
        reloadTimeLeft = reloadTime;
    }

    public void UpdateMove()
    {
        if ((Vector2)transform.position == targetPosition && finishedPath) return;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        if ((Vector2)transform.position == targetPosition)
        {
            mapIndex++;
            GetNextMapPosition();
        }
    }

    public void Damage(int damage)
    {
        if (!IsServer) return;
        if (statusEffects.ContainsKey(typeof(ShieldStatus)) && statusEffects[typeof(ShieldStatus)].Count > 0)
        {
            List<StatusEffect> shield = statusEffects[typeof(ShieldStatus)];
            ((ShieldStatus)shield[0]).Damage(damage);
            ShieldClientRpc(ShieldHealth, default);
            return;
        }
        health.Value = Mathf.Max(health.Value - Mathf.Max(0, damage - armor), 0);
        if (health.Value == 0)
        {
            Die();
        }
    }

    public void Shoot()
    {
        if (!IsServer)
        {
            return;
        }
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            projectile.GetComponent<NetworkObject>().Spawn();
            // FOR TESTING PURPOSES ONLY
            AddStatusEffect(new ShieldStatus(2));
        }
    }

    public void Die()
    {
        if (!IsServer) return;
        DieRpc();
        Destroy(gameObject);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void DieRpc()
    {
        AlienClientDeathEvent?.Invoke();
    }

    public void AddStatusEffect(StatusEffect effect)
    {
        if (!IsServer) return;
        Type type = effect.GetType();
        if (!statusEffects.ContainsKey(type))
        {
            statusEffects.Add(type, new List<StatusEffect>());
        }
        statusEffects[type].Add(effect);

        switch (effect)
        {
            case ShieldStatus shield:
                List<StatusEffect> shieldEffects = statusEffects[type];
                if (shieldEffects.Count == 0)
                {
                    shieldEffects.Add(shield);
                    shield.ShieldBroken += () => RemoveStatusEffect(shield);
                    ShieldClientRpc(shield.Health, default);
                }
                else
                {
                    ((ShieldStatus)shieldEffects[0]).AddHealth(shield.Health);
                    ShieldClientRpc(ShieldHealth, default);
                }
                break;
        }
    }

    public void RemoveStatusEffect(StatusEffect effect)
    {
        statusEffects[effect.GetType()].Remove(effect);
        Debug.Log("shield is gone");
        Destroy(shieldBar);
        Destroy(shieldDisplay);
        return;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ShieldClientRpc(int shieldHealth, RpcParams rpcParams)
    {
        if (shieldBar == null)
        {
            shieldBar = Instantiate(ClientPrefabs.Instance.ShieldHealthbarPrefab).GetComponent<ShieldedAlienHealthbar>();
            shieldBar.Initialize(this, shieldHealth);
            shieldDisplay = Instantiate(ClientPrefabs.Instance.ShieldPrefab);
            shieldDisplay.GetComponent<FollowObject>().InitializeTarget(gameObject, Vector2.zero);
            shieldDisplay.transform.localScale = GetComponent<SpriteRenderer>().bounds.size * Mathf.Sqrt(2);
            return;
        }
        shieldBar.UpdateShield(shieldHealth);
    }
}