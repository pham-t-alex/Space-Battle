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

    [SerializeField] private int startingMoney = 500;
    [SerializeField] private int startingIncome = 100;

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
        switch (player)
        {
            case 1:
                GameState.P1Money = newMoney;
                break;
            case 2:
                GameState.P2Money = newMoney;
                break;
        }
    }

    public void ChangeIncome(int player, int newIncome)
    {
        switch (player)
        {
            case 1:
                GameState.P1Income = newIncome;
                break;
            case 2:
                GameState.P2Income = newIncome;
                break;
        }
    }
}
