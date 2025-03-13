using UnityEngine;
using Unity.Netcode;
using System;

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

    private int p1Money;
    private int p2Money;
    private int p1Income;
    private int p2Income;

    public event Action<int> P1InternalMoneyUpdate;
    public event Action<int> P2InternalMoneyUpdate;
    public event Action<int> P1InternalIncomeUpdate;
    public event Action<int> P2InternalIncomeUpdate;

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
        SetMoney(1, startingMoney);
        SetMoney(2, startingMoney);
        SetIncome(1, startingIncome);
        SetIncome(2, startingIncome);
        incomeTimeSec = maxIncomeTimeSec;
    }

    public void TriggerIncome()
    {
        Debug.Log("Triggering Income");
        ChangeMoney(1, p1Income);
        ChangeMoney(2, p2Income);
    }

    // returns true if successful
    public bool ChangeMoney(int player, int money)
    {
        switch (player)
        {
            case 1:
                if (p1Money + money < 0) return false;
                SetMoney(1, p1Money + money);
                return true;
            case 2:
                if (p2Money + money < 0) return false;
                SetMoney(2, p2Money + money);
                return true;
            default:
                return false;
        }
    }

    void SetMoney(int player, int newMoney)
    {
        switch (player)
        {
            case 1:
                p1Money = newMoney;
                P1InternalMoneyUpdate?.Invoke(p1Money);
                break;
            case 2:
                p2Money = newMoney;
                P2InternalMoneyUpdate?.Invoke(p2Money);
                break;
        }
    }

    // returns true if successful
    public bool ChangeIncome(int player, int income)
    {
        switch (player)
        {
            case 1:
                if (p1Income + income < 0) return false;
                SetIncome(1, p1Income + income);
                return true;
            case 2:
                if (p2Income + income < 0) return false;
                SetIncome(2, p2Income + income);
                return true;
            default:
                return false;
        }
    }

    void SetIncome(int player, int newIncome)
    {
        switch (player)
        {
            case 1:
                p1Income = newIncome;
                P1InternalIncomeUpdate?.Invoke(p1Income);
                break;
            case 2:
                p2Income = newIncome;
                P2InternalIncomeUpdate?.Invoke(p2Income);
                break;
        }
    }
}
