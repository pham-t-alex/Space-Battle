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

    [SerializeField] private int startingMoney;
    [SerializeField] private int startingIncome = 100;
    [SerializeField] private float maxIncomeTimeSec = 5;
    private float p1IncomeTimeSec = 0;
    private float p2IncomeTimeSec = 0;

    private int p1Money;
    private int p2Money;
    private int p1Income;
    private int p2Income;

    public event Action<int> P1InternalMoneyUpdate;
    public event Action<int> P2InternalMoneyUpdate;
    public event Action<int> P1InternalIncomeUpdate;
    public event Action<int> P2InternalIncomeUpdate;

    private float p1IncomeMultiplier = 1f;
    public float P1IncomeMultiplier => p1IncomeMultiplier;
    private float p2IncomeMultiplier = 1f;
    public float P2IncomeMultiplier => p2IncomeMultiplier;
    private float p1IncomeRateMultiplier = 1f;
    private float p2IncomeRateMultiplier = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (p1IncomeTimeSec > 0)
        {
            p1IncomeTimeSec -= Time.deltaTime;
            if (p1IncomeTimeSec <= 0)
            {
                p1IncomeTimeSec = maxIncomeTimeSec / p1IncomeRateMultiplier;
                TriggerIncome(1);
            }
        }
        if (p2IncomeTimeSec > 0)
        {
            p2IncomeTimeSec -= Time.deltaTime;
            if (p2IncomeTimeSec <= 0)
            {
                p2IncomeTimeSec = maxIncomeTimeSec / p2IncomeRateMultiplier;
                TriggerIncome(2);
            }
        }
    }

    public void Setup()
    {
        SetMoney(1, startingMoney);
        SetMoney(2, startingMoney);
        SetIncome(1, startingIncome);
        SetIncome(2, startingIncome);
        p1IncomeTimeSec = maxIncomeTimeSec;
        p2IncomeTimeSec = maxIncomeTimeSec;
    }

    public void TriggerIncome(int player)
    {
        switch (player)
        {
            case 1:
                ChangeMoney(1, p1Income);
                break;
            case 2:
                ChangeMoney(2, p2Income);
                break;
        }
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

    public void ChangeIncomeMultiplier(int player, int deltaMultiplier)
    {
        switch (player)
        {
            case 1:
                p1IncomeMultiplier += deltaMultiplier;
                break;
            case 2:
                p2IncomeMultiplier += deltaMultiplier;
                break;
        }
    }

    public void ChangeIncomeRateMultiplier(int player, int deltaMultiplier)
    {
        switch (player)
        {
            case 1:
                p1IncomeRateMultiplier += deltaMultiplier;
                break;
            case 2:
                p2IncomeRateMultiplier += deltaMultiplier;
                break;
        }
    }
}
