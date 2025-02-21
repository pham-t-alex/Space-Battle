using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class Player : NetworkBehaviour
{
    [SerializeField] private float speed = 5;
    [SerializeField] private float leftBound = -5;
    [SerializeField] private float rightBound = 5;
    private float movement = 0;
    private InputSystem_Actions controls;
    private NetworkVariable<int> health = new NetworkVariable<int>();

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
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (IsOwner)
        {
            float move = controls.Player.Move.ReadValue<Vector2>().x;
            if (move != movement)
            {
                movement = move;
                MovementUpdateServerRpc(move);
            }
        }
    }

    void FixedUpdate()
    {
        if (IsServer)
        {
            Move();
        }
    }

    // Can only be called by server
    public void Move()
    {
        Vector3 newPos = transform.localPosition + (new Vector3(movement, 0, 0) * speed * Time.deltaTime);
        transform.localPosition = new Vector3(Mathf.Clamp(newPos.x, leftBound, rightBound), newPos.y, newPos.z);
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