using UnityEngine;
using Unity.Netcode;
using System;

public class GameState : NetworkBehaviour
{
    // Should only be modified by MoneyController
    private NetworkVariable<int> p1Money = new NetworkVariable<int>(0);
    private NetworkVariable<int> p2Money = new NetworkVariable<int>(0);
    private NetworkVariable<int> p1Income = new NetworkVariable<int>(0);
    private NetworkVariable<int> p2Income = new NetworkVariable<int>(0);

    private static GameState instance;
    public static GameState Instance => instance;

    public int P1Money
    {
        get { return p1Money.Value; }
    }

    public int P2Money
    {
        get { return p2Money.Value; }
    }

    public int P1Income
    {
        get { return p1Income.Value; }
    }

    public int P2Income
    {
        get { return p2Income.Value; }
    }

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


    public event Action<int> Player1MoneyUpdate;
    public event Action<int> Player1IncomeUpdate;
    public event Action<int> Player2MoneyUpdate;
    public event Action<int> Player2IncomeUpdate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (IsServer)
        {
            MoneyController.Instance.P1InternalMoneyUpdate += SetP1Money;
            MoneyController.Instance.P2InternalMoneyUpdate += SetP2Money;
            MoneyController.Instance.P1InternalIncomeUpdate += SetP1Income;
            MoneyController.Instance.P2InternalIncomeUpdate += SetP2Income;
        }
        if (IsClient)
        {
            p1Money.OnValueChanged += (oldVal, newVal) => Player1MoneyUpdate?.Invoke(newVal);
            p2Money.OnValueChanged += (oldVal, newVal) => Player2MoneyUpdate?.Invoke(newVal);
            p1Income.OnValueChanged += (oldVal, newVal) => Player1IncomeUpdate?.Invoke(newVal);
            p2Income.OnValueChanged += (oldVal, newVal) => Player2IncomeUpdate?.Invoke(newVal);
        }
    }

    public override void OnNetworkSpawn()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetP1Money(int money)
    {
        if (!IsServer) return;
        p1Money.Value = money;
    }

    void SetP2Money(int money)
    {
        if (!IsServer) return;
        p2Money.Value = money;
    }

    void SetP1Income(int income)
    {
        if (!IsServer) return;
        p1Income.Value = income;
    }

    void SetP2Income(int income)
    {
        if (!IsServer) return;
        p2Income.Value = income;
    }
}
