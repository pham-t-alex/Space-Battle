using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;

public class Player : NetworkBehaviour
{
    [SerializeField] private float speed = 5;
    [SerializeField] private float leftBound = -5;
    [SerializeField] private float rightBound = 5;
    private float movement = 0;
    private float newMove = 0;
    private InputSystem_Actions controls;
    private NetworkVariable<int> health = new NetworkVariable<int>();
    private NetworkVariable<int> maxHealth = new NetworkVariable<int>();

    private Rigidbody2D rb;

    public event Action PlayerDeathEvent;
    public event Action PlayerClientDeathEvent;
    public event Action<int, int> HealthChange;
    public event Action<int, int> MaxHealthChange;

    private List<Module> modules = new List<Module>();
    public int ModuleCount => modules.Count;
    // note that level is starting at 0, not 1; add 1 to level when displaying
    private int level;
    public int Level => level;
    [SerializeField] private float moduleGap;
    // Should only be accessed by server
    public bool CanAddModule => modules.Count < GameController.Instance.MaxModules;
    public bool CanLevelUp => level < GameController.Instance.MaxLevel;
    public Module GetModule(int module) => modules[module];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        health.OnValueChanged += (oldVal, newVal) => HealthChange?.Invoke(oldVal, newVal);
        maxHealth.OnValueChanged += (oldVal, newVal) => MaxHealthChange?.Invoke(oldVal, newVal);
        if (IsOwner)
        {
            controls = new InputSystem_Actions();
            controls.Enable();
        }
        if (IsServer)
        {
            ServerSetup();
        }
        else
        {
            Destroy(GetComponent<Rigidbody2D>());
            Destroy(GetComponent<Collider2D>());
        }
    }

    public void ServerSetup()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject g = Instantiate(GameController.Instance.ModulePrefab);
        g.GetComponent<NetworkObject>().Spawn();
        g.transform.SetParent(transform, false);
        g.transform.localPosition = Vector3.zero;
        modules.Add(g.GetComponent<Module>());
    }

    // called by server
    public void HealthSetup(int player, int maxHealth)
    {
        health.Value = maxHealth;
        this.maxHealth.Value = maxHealth;
        HealthbarSetupRpc(player, maxHealth);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void HealthbarSetupRpc(int player, int maxHealth)
    {
        GameUI.Instance.ClientSetup(this, player, maxHealth);
    }

    public void AddModule(bool right)
    {
        if (!IsServer) return;
        GameObject g = Instantiate(GameController.Instance.ModulePrefab);
        g.GetComponent<NetworkObject>().Spawn();
        g.transform.SetParent(transform, false);
        if (right)
        {
            for (int i = 0; i < modules.Count; i++)
            {
                modules[i].transform.localPosition += Vector3.left * (moduleGap / 2);
            }
            g.transform.localPosition = modules[modules.Count - 1].transform.localPosition + Vector3.right * moduleGap;
            modules.Add(g.GetComponent<Module>());
        }
        else
        {
            for (int i = 0; i < modules.Count; i++)
            {
                modules[i].transform.localPosition += Vector3.right * (moduleGap / 2);
            }
            g.transform.localPosition = modules[1].transform.localPosition + Vector3.left * moduleGap;
            modules.Insert(0, g.GetComponent<Module>());
        }
        GetComponent<BoxCollider2D>().size += Vector2.right * moduleGap;
    }

    public void LevelUp()
    {
        if (!IsServer) return;
        level++;
        int newMax = GameController.Instance.LevelHP(level);
        int diff = newMax - maxHealth.Value;
        maxHealth.Value = newMax;
        health.Value += diff;
    }

    // Update is called once per frame
    private void Update()
    {
        if (IsOwner)
        {
            newMove = controls.Player.Move.ReadValue<Vector2>().x;
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            if (movement != newMove)
            {
                movement = newMove;
                MovementUpdateServerRpc(movement);
            }
        }
        if (IsServer)
        {
            Move();
        }
    }

    public void Move()
    {
        if (IsServer)
        {
            rb.MovePosition(new Vector2(Mathf.Clamp(rb.position.x + movement * speed * Time.fixedDeltaTime, 
                leftBound + (modules.Count * moduleGap / 2), rightBound - (modules.Count * moduleGap / 2)), rb.position.y));
        }
    }

    [ServerRpc]
    public void MovementUpdateServerRpc(float move)
    {
        movement = move;
    }

    // Only can be called by server
    public void SetBounds(float left, float right)
    {
        leftBound = left;
        rightBound = right;
    }

    public void Damage(int damage)
    {
        if (!IsServer) return;
        health.Value = Mathf.Max(health.Value - damage, 0);
        if (health.Value == 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (!IsServer) return;
        DieRpc();
        for (int i = 0; i < modules.Count; i++)
        {
            modules[i].Destroy();
        }
        PlayerDeathEvent?.Invoke();
        Destroy(gameObject);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void DieRpc()
    {
        PlayerClientDeathEvent?.Invoke();
    }

    // called by GameUI from client
    [Rpc(SendTo.Server)]
    public void SelectModuleRpc(int module, RpcParams rpcParams)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (module > modules.Count)
        {
            InvalidModuleRpc(RpcTarget.Single(clientId, RpcTargetUse.Temp));
            return;
        }
        Module m = modules[module];
        ModuleUIRpc(RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void InvalidModuleRpc(RpcParams rpcParams)
    {
        GameUI.Instance.InvalidModuleSelection();
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ModuleUIRpc(RpcParams rpcParams)
    {
        GameUI.Instance.OpenModuleUI();
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void StructureUIRpc(StructureUpgradeInfo info, RpcParams rpcParams)
    {

    }
}