using Unity.Netcode;
using UnityEngine;

public class ClientPrefabs : MonoBehaviour
{
    private static ClientPrefabs instance;
    public static ClientPrefabs Instance => instance;
    
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

    [SerializeField] private GameObject alienHealthbarPrefab;
    public GameObject AlienHealthbarPrefab => alienHealthbarPrefab;
    [SerializeField] private GameObject moneyDisplayPrefab;
    public GameObject MoneyDisplayPrefab => moneyDisplayPrefab;
    [SerializeField] private GameObject explosionPrefab;
    public GameObject ExplosionPrefab => explosionPrefab;
    [SerializeField] private GameObject shieldHealthbarPrefab;
    public GameObject ShieldHealthbarPrefab => shieldHealthbarPrefab;
}
