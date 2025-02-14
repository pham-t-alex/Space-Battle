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
        if (newPos.x > rightBound)
        {
            newPos.x = rightBound;
        }
        else if (newPos.x < leftBound)
        {
            newPos.x = leftBound;
        }
        transform.position = newPos;
    }
}
