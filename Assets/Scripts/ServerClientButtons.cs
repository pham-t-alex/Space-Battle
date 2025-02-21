using UnityEngine;
using Unity.Netcode;

public class ServerClientButtons : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Destroy(gameObject);
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        Destroy(gameObject);
    }
}
