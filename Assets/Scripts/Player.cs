using UnityEngine;
using Unity.Netcode;
using System;

[RequireComponent(typeof(Collider2D))]
public class Player : NetworkBehaviour
{
    [SerializeField] private float speed = 5;
    [SerializeField] private float leftBound = -5;
    [SerializeField] private float rightBound = 5;
    private float movement = 0;
    private float newMove = 0;
    private InputSystem_Actions controls;
    private NetworkVariable<int> health = new NetworkVariable<int>();
    [SerializeField] private int maxHealth = 5;

    // TEMPORARY, REMOVE LATER
    [SerializeField] private GameObject gun;

    private Rigidbody2D rb;

    public event Action PlayerDeathEvent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            controls = new InputSystem_Actions();
            controls.Enable();
        }
        if (!IsServer)
        {
            Destroy(GetComponent<Rigidbody2D>());
        }
    }

    public void Setup()
    {
        if (!IsServer) return;
        rb = GetComponent<Rigidbody2D>();
        // TEMPORARY, REMOVE LATER
        SpawnGun();
        health.Value = maxHealth;
    }

    // TEMPORARY, REMOVE LATER
    public void SpawnGun()
    {
        if (IsServer)
        {
            GameObject g = Instantiate(gun);
            NetworkObject obj = g.GetComponent<NetworkObject>();
            obj.Spawn();
            g.transform.parent = transform;
            g.transform.localPosition = Vector3.zero;
        }
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
            rb.MovePosition(new Vector2(Mathf.Clamp(rb.position.x + movement * speed * Time.fixedDeltaTime, leftBound, rightBound), rb.position.y));
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
        health.Value -= damage;
        if (health.Value < 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (!IsServer) return;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(0).gameObject);
        }
        PlayerDeathEvent?.Invoke();
        Destroy(gameObject);
    }
}