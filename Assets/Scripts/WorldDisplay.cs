using Unity.Netcode;
using UnityEngine;

public class WorldDisplay : MonoBehaviour
{
    private static WorldDisplay instance;
    public static WorldDisplay Instance => instance;

    private void Awake()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsClient)
        {
            Destroy(gameObject);
        }
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
