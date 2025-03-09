using UnityEngine;
using Unity.Netcode;
using System;

public class GameState : NetworkBehaviour
{
    // Should only be modified by MoneyController
    private static NetworkVariable<int> p1Money = new NetworkVariable<int>(0);
    private static NetworkVariable<int> p2Money = new NetworkVariable<int>(0);
    private static NetworkVariable<int> p1Income = new NetworkVariable<int>(0);
    private static NetworkVariable<int> p2Income = new NetworkVariable<int>(0);
    
    public static int P1Money
    {
        set { p1Money.Value = value; }
        get { return p1Money.Value; }
    }

    public static int P2Money
    {
        set { p2Money.Value = value; }
        get { return p2Money.Value; }
    }

    public static int P1Income
    {
        set { p1Income.Value = value; }
        get { return p1Income.Value; }
    }

    public static int P2Income
    {
        set { p2Income.Value = value; }
        get { return p2Income.Value; }
    }

    public static event Action<int> Player1MoneyUpdate;
    public static event Action<int> Player1IncomeUpdate;
    public static event Action<int> Player2MoneyUpdate;
    public static event Action<int> Player2IncomeUpdate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        p1Money.OnValueChanged += (oldVal, newVal) => Player1MoneyUpdate?.Invoke(newVal);
        p2Money.OnValueChanged += (oldVal, newVal) => Player2MoneyUpdate?.Invoke(newVal);
        p1Income.OnValueChanged += (oldVal, newVal) => Player1IncomeUpdate?.Invoke(newVal);
        p2Income.OnValueChanged += (oldVal, newVal) => Player2IncomeUpdate?.Invoke(newVal);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
