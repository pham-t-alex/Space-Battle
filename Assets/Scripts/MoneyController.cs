using UnityEngine;
using Unity.Netcode;

public class MoneyController : MonoBehaviour
{
    private static MoneyController instance;
    public static MoneyController Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private int p1Money = 0;
    private int p1Income = 0;
    private int p2Money = 0;
    private int p2Income = 0;
    [SerializeField] private int startingMoney = 1000;
    [SerializeField] private int startingIncome;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += DestroyOnConnect;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= DestroyOnConnect;
        }
    }

    private void DestroyOnConnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeMoney(int player, int newMoney)
    {

    }

}
