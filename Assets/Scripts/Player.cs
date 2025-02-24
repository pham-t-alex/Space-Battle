using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine.Experimental.AI;
using Unity.VisualScripting;

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

    // TEMPORARY, REMOVE LATER
    [SerializeField] private GameObject gun;

    private Rigidbody2D rb;

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
        if (IsServer)
        {
            GameController.Instance.SpawnPlayer(this);
            rb = GetComponent<Rigidbody2D>();
            // TEMPORARY, REMOVE LATER
            SpawnGun();
        }
        if (IsClient)
        {
            Destroy(GetComponent<Rigidbody2D>());
        }
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
            Move(Time.fixedDeltaTime);
        }
    }

    public void Move(float deltaTime)
    {
        if (IsServer)
        {
            rb.MovePosition(new Vector2(Mathf.Clamp(rb.position.x + movement * speed * deltaTime, leftBound, rightBound), rb.position.y));
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
}