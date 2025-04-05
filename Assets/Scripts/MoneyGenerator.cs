using UnityEngine;
using Unity.Netcode;
using TMPro;

public class MoneyGenerator : Structure
{
    [SerializeField] private int moneyGain;
    // in seconds
    [SerializeField] private float moneyDelay;
    private float currentDelay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentDelay = moneyDelay;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        if (currentDelay > 0)
        {
            currentDelay -= Time.deltaTime;
            if (currentDelay <= 0)
            {
                ProduceMoney();
                currentDelay = moneyDelay;
            }
        }
    }

    public void ProduceMoney()
    {
        MoneyController.Instance.ChangeMoney(player.PlayerNum, moneyGain);
        ProduceMoneyDisplayRpc(moneyGain);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ProduceMoneyDisplayRpc(int money)
    {
        GameObject g = Instantiate(ClientPrefabs.Instance.MoneyDisplayPrefab);
        g.GetComponent<TMP_Text>().text = $"+${money}";
        g.GetComponent<FlyawayText>().Initialize(transform.position);
    }
}
