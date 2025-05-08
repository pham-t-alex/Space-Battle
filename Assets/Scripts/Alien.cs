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

    [SerializeField] protected GameObject projectilePrefab;
    [SerializeField] private float reloadTimeLeft;
    [SerializeField] private float reloadTime;
    private float ReloadTime
    {
        get
        {
            float berserkBuff = 1;
            if (statusEffects.ContainsKey(typeof(BerserkStatus)) && statusEffects[typeof(BerserkStatus)].Count > 0) {
                foreach (StatusEffect effect in statusEffects[typeof(BerserkStatus)])
                {
                    berserkBuff += ((BerserkStatus)effect).Buff;
                }
            }
            float waveSlow = 1;
            if (statusEffects.ContainsKey(typeof(WaveSlow)) && statusEffects[typeof(WaveSlow)].Count > 0)
            {
                foreach (StatusEffect effect in statusEffects[typeof(WaveSlow)])
                {
                    waveSlow = Mathf.Min(waveSlow, ((WaveSlow)effect).Multiplier);
                }
            }
            return reloadTime / (berserkBuff * waveSlow);
        }
    }

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
    private List<StatusEffect> startingStatusEffects = new List<StatusEffect>();

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

    public void PreSpawnServerInitialize()
    {
        health.Value = maxHealth;
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

    protected virtual void ServerSpawn()
    {
        foreach (StatusEffect effect in startingStatusEffects)
        {
            AddStatusEffect(effect);
        }
        float maxReload = ReloadTime;
        reloadTimeLeft = UnityEngine.Random.Range(maxReload / 2, maxReload);
    }

    public virtual void Initialize(bool front, bool sent, int world, Player target)
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
            targetPosition = path[mapIndex] + GameController.Instance.GetWorldCenter(world) + new Vector2(UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(-0.2f, 0.2f));
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
        reloadTimeLeft = ReloadTime;
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

    public void Heal(int heal)
    {
        if (!IsServer) return;
        health.Value = Mathf.Min(health.Value + heal, maxHealth);
    }

    public virtual void Shoot()
    {
        if (!IsServer)
        {
            return;
        }
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            projectile.GetComponent<NetworkObject>().Spawn();
        }
    }

    public virtual void Die()
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

    // Should be run before spawn
    public void ApplyStartingModifiers(Modifiers modifiers)
    {
        if (modifiers.shielded)
        {
            AddStartingStatusEffect(new ShieldStatus(Mathf.RoundToInt(0.5f * health.Value)));
        }
        if (modifiers.invisible)
        {
            AddStartingStatusEffect(new InvisibleStatus(15));
        }
        if (modifiers.berserk)
        {
            AddStartingStatusEffect(new BerserkStatus(15, 1));
        }
        if (modifiers.regenerating)
        {
            AddStartingStatusEffect(new RegenStatus(4, maxHealth));
        }
    }

    // Should be run before spawn
    void AddStartingStatusEffect(StatusEffect effect)
    {
        startingStatusEffects.Add(effect);
    }

    public void AddStatusEffect(StatusEffect effect)
    {
        if (!IsServer) return;
        Type type = effect.GetType();
        if (!statusEffects.ContainsKey(type))
        {
            statusEffects.Add(type, new List<StatusEffect>());
        }

        List<StatusEffect> effects = statusEffects[type];
        // returns early if new effect is not added
        switch (effect)
        {
            case ShieldStatus shield:
                if (effects.Count == 0)
                {
                    effects.Add(shield);
                    shield.ShieldBroken += () => RemoveStatusEffect(shield);
                    ShieldClientRpc(shield.Health, default);
                }
                else
                {
                    ((ShieldStatus)effects[0]).AddHealth(shield.Health);
                    ShieldClientRpc(ShieldHealth, default);
                    return;
                }
                break;
            case InvisibleStatus invis:
                if (effects.Count == 0)
                {
                    effects.Add(invis);
                    InvisibleRpc(default);
                }
                else
                {
                    InvisibleStatus current = (InvisibleStatus)effects[0];
                    current.SetTimeLeft(Mathf.Max(invis.TimeLeft, current.TimeLeft));
                    return;
                }
                break;
            case BerserkStatus berserk:
                effects.Add(berserk);
                break;
            case RegenStatus regen:
                effects.Add(regen);
                regen.Heal += () => Heal(1);
                break;
            case WaveSlow slow:
                effects.Add(slow);
                break;
        }

        effect.Expire += () => RemoveStatusEffect(effect);
        StatusTimeUpdate += effect.Countdown;
    }

    public void RemoveStatusEffect(StatusEffect effect)
    {
        statusEffects[effect.GetType()].Remove(effect);

        switch (effect)
        {
            case ShieldStatus shield:
                DestroyShieldRpc(default);
                break;
            case InvisibleStatus invis:
                VisibleRpc(default);
                break;
        }
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

    [Rpc(SendTo.ClientsAndHost)]
    public void DestroyShieldRpc(RpcParams rpcParams)
    {
        Destroy(shieldBar.gameObject);
        Destroy(shieldDisplay);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void InvisibleRpc(RpcParams rpcParams)
    {
        Color c = GetComponent<SpriteRenderer>().color;
        GetComponent<SpriteRenderer>().color = new Color(c.r, c.g, c.b, 0);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void VisibleRpc(RpcParams rpcParams)
    {
        Color c = GetComponent<SpriteRenderer>().color;
        GetComponent<SpriteRenderer>().color = new Color(c.r, c.g, c.b, 1);
    }
}