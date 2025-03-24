using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    private static GameUI _instance;

    [SerializeField] private GameObject moneyText;
    [SerializeField] private GameObject incomeText;

    private bool sendFrontLineSet = false;

    private void Awake()
    {
        _instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    [SerializeField] private GameObject gameEndScreen;
    [SerializeField] private PlayerHealthbar p1Health;
    [SerializeField] private PlayerHealthbar p2Health;

    // Update is called once per frame
    void Update()
    {

    }

    // Client side setup
    public static void Setup(int player)
    {
        GameMessenger.Instance.ClientGameEndUpdate += _instance.TriggerGameEnd;
        switch (player)
        {
            case 1:
                GameState.Player1MoneyUpdate += _instance.UpdateMoney;
                GameState.Player1IncomeUpdate += _instance.UpdateIncome;
                break;
            case 2:
                GameState.Player2MoneyUpdate += _instance.UpdateMoney;
                GameState.Player2IncomeUpdate += _instance.UpdateIncome;
                break;
        }
    }

    // Client side healthbar setup
    public static void SetupHealthbar(Player p, int player, int maxHealth)
    {
        switch (player)
        {
            case 1:
                _instance.p1Health.Initialize(p, maxHealth);
                break;
            case 2:
                _instance.p2Health.Initialize(p, maxHealth);
                break;
        }
    }

    public void UpdateMoney(int money)
    {
        if (moneyText != null)
        {
            moneyText.GetComponent<TMP_Text>().text = "$" + money;
        }
    }

    public void UpdateIncome(int income)
    {
        if (incomeText != null)
        {
            incomeText.GetComponent<TMP_Text>().text = "" + income;
        }
    }

    public void SendAliens(int sendIndex)
    {
        GameMessenger.Instance.SendAliens(sendIndex, sendFrontLineSet);
    }

    public void AddModule(bool right)
    {
        GameMessenger.Instance.AddModule(right);
    }

    // True - trigger victory; False - trigger defeat
    public void TriggerGameEnd(bool victorious)
    {
        gameEndScreen.SetActive(true);
        gameEndScreen.GetComponent<GameEndScreen>().TriggerGameEnd(victorious);
    }
}
