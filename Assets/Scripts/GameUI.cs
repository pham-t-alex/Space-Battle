using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    private static GameUI _instance;

    [SerializeField] private GameObject moneyText;
    [SerializeField] private GameObject incomeText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void Setup(int player)
    {
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
}
