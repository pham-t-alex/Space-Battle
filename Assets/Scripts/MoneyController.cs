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
    [SerializeField] private float maxIncomeTimeSec = 6;
    private float incomeTimeSec = 0;

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
        if (incomeTimeSec > 0)
        {
            incomeTimeSec -= Time.deltaTime;
            if (incomeTimeSec <= 0)
            {
                incomeTimeSec = maxIncomeTimeSec;
                TriggerIncome();
            }
        }
    }

    public void Setup()
    {
        GameState.P1Money = startingMoney;
        GameState.P2Money = startingMoney;
        GameState.P1Income = startingIncome;
        GameState.P2Income = startingIncome;
        incomeTimeSec = maxIncomeTimeSec;
    }

    public void TriggerIncome()
    {
        Debug.Log("Triggering Income");
        // Player 1
        ChangeMoney(1, GameState.P1Money + GameState.P1Income);
        // Player 2
        ChangeMoney(2, GameState.P2Money + GameState.P2Income);
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
