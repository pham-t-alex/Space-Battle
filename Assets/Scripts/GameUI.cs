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
    [SerializeField] private GameObject shipOptionsUI;
    [SerializeField] private GameObject moduleUI;
    [SerializeField] private GameObject structureUI;

    private InputSystem_Actions controls;

    // 0: none
    // 1-9: module 1-9 is selected
    private int selectedModule = 0;
    // If selectedModule > 0, then this indicates whether there already is a structure there
    private bool moduleHasStructure = false;

    private Player player1;
    private Player player2;
    private int associatedPlayer;

    // Update is called once per frame
    void Update()
    {

    }

    // Client side setup
    public static void Setup(int player)
    {
        _instance.SetupControls();
        _instance.associatedPlayer = player;
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

    void SetupControls()
    {
        controls = new InputSystem_Actions();
        controls.Enable();
        controls.Player.Module1.performed += (ctx) => SelectModule(1);
        controls.Player.Module2.performed += (ctx) => SelectModule(2);
        controls.Player.Module3.performed += (ctx) => SelectModule(3);
        controls.Player.Module4.performed += (ctx) => SelectModule(4);
        controls.Player.Module5.performed += (ctx) => SelectModule(5);
        controls.Player.Module6.performed += (ctx) => SelectModule(6);
        controls.Player.Module7.performed += (ctx) => SelectModule(7);
        controls.Player.Module8.performed += (ctx) => SelectModule(8);
        controls.Player.Module9.performed += (ctx) => SelectModule(9);
    }

    // Client side setup
    public static void ClientSetup(Player p, int player, int maxHealth)
    {
        switch (player)
        {
            case 1:
                _instance.player1 = p;
                _instance.p1Health.Initialize(p, maxHealth);
                break;
            case 2:
                _instance.player2 = p;
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

    public void LevelUp()
    {
        GameMessenger.Instance.LevelUp();
    }

    public void SelectModule(int module)
    {
        if (selectedModule > 0) return;

    }

    // called externally
    public void ModuleSelectUpdate(int module)
    {
        selectedModule = module;
        shipOptionsUI.SetActive(false);
    }

    public void BuildStructure(int structure)
    {

    }

    public void DeselectModule()
    {
        if (selectedModule == 0) return;
        moduleUI.SetActive(false);
        structureUI.SetActive(false);
        shipOptionsUI.SetActive(true);
    }

    // True - trigger victory; False - trigger defeat
    public void TriggerGameEnd(bool victorious)
    {
        gameEndScreen.SetActive(true);
        gameEndScreen.GetComponent<GameEndScreen>().TriggerGameEnd(victorious);
    }
}
