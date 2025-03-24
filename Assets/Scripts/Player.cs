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

    [SerializeField] private int maxModules;
    private Module[] modules;
    private int moduleCount;
    private int level;
    [SerializeField] private float moduleGap;

    public bool CanAddModule => moduleCount < maxModules;
    // Should only be accessed by server
    public bool CanLevelUp => level < GameController.Instance.MaxLevel;

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
        modules = new Module[maxModules];
        GameObject g = Instantiate(GameController.Instance.ModulePrefab);
        g.GetComponent<NetworkObject>().Spawn();
        g.transform.SetParent(transform, false);
        g.transform.localPosition = Vector3.zero;
        modules[0] = g.GetComponent<Module>();
        moduleCount = 1;
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
        GameUI.SetupHealthbar(this, player, maxHealth);
    }

    public void AddModule(bool right)
    {
        if (!IsServer) return;
        GameObject g = Instantiate(GameController.Instance.ModulePrefab);
        g.GetComponent<NetworkObject>().Spawn();
        g.transform.SetParent(transform, false);
        if (right)
        {
            for (int i = 0; i < moduleCount; i++)
            {
                modules[i].transform.localPosition += Vector3.left * (moduleGap / 2);
            }
            g.transform.localPosition = modules[moduleCount - 1].transform.localPosition + Vector3.right * moduleGap;
            modules[moduleCount] = g.GetComponent<Module>();
        }
        else
        {
            for (int i = moduleCount - 1; i >= 0; i--)
            {
                modules[i].transform.localPosition += Vector3.right * (moduleGap / 2);
                modules[i + 1] = modules[i];
            }
            g.transform.localPosition = modules[1].transform.localPosition + Vector3.left * moduleGap;
            modules[0] = g.GetComponent<Module>();
        }
        moduleCount++;
        GetComponent<BoxCollider2D>().size += Vector2.right * moduleGap;
    }

    // TEMPORARY, REMOVE LATER
    /*public void SpawnGun()
    {
        if (IsServer)
        {
            GameObject g = Instantiate(gun);
            NetworkObject obj = g.GetComponent<NetworkObject>();
            obj.Spawn();
            g.transform.parent = transform;
            g.transform.localPosition = Vector3.zero;
        }
    }*/

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
            rb.MovePosition(new Vector2(Mathf.Clamp(rb.position.x + movement * speed * Time.fixedDeltaTime, leftBound + (moduleCount * moduleGap / 2), rightBound - (moduleCount * moduleGap / 2)), rb.position.y));
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
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        PlayerDeathEvent?.Invoke();
        Destroy(gameObject);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void DieRpc()
    {
        PlayerClientDeathEvent?.Invoke();
    }
}