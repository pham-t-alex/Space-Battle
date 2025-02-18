using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class Player : NetworkBehaviour
{
    [SerializeField] private float speed = 5;
    [SerializeField] private float leftBound = -5;
    [SerializeField] private float rightBound = 5;
    private float movement;
    private InputSystem_Actions controls;
    private NetworkVariable<int> health = new NetworkVariable<int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controls = new InputSystem_Actions();
        controls.Enable();
        
    }

    // Update is called once per frame

    private void Update()
    {
        movement = controls.Player.Move.ReadValue<Vector2>().x;
    }

    void FixedUpdate()
    {
        MoveServerRpc();
    }

    [ServerRpc]
    public void MoveServerRpc()
    {
        Vector3 newPos = transform.position + (new Vector3(movement, 0, 0) * speed * Time.deltaTime);
        transform.position = new Vector3(Mathf.Clamp(newPos.x, leftBound, rightBound), newPos.y, newPos.z);
    }
}