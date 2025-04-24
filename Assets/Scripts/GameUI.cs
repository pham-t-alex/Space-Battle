using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

public class GameUI : MonoBehaviour
{
    private static GameUI instance;
    public static GameUI Instance => instance;

    [SerializeField] private GameObject moneyText;
    [SerializeField] private GameObject incomeText;

    private void Awake()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsClient)
        {
            Destroy(gameObject);
        }
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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
    [SerializeField] private GameObject upgrade1UI;
    [SerializeField] private GameObject upgrade2UI;
    [SerializeField] private GameObject noUpgradeUI;

    private InputSystem_Actions controls;

    // 0: none
    // 1-9: module 1-9 is selected
    private int selectedModule = 0;
    private int attemptedSelectedModule = 0;

    // use later for hotkey legality checking
    private bool hasStructure;

    private Player player1;
    private Player player2;
    private int associatedPlayer;

    [SerializeField] private AlienSends alienSends;
    [SerializeField] private GameObject alienSendButtonPrefab;
    [SerializeField] private GameObject alienSendsContainer;
    private List<AlienSendButton> buttons = new List<AlienSendButton>();

    [SerializeField] private BuyButton moduleLButton;
    [SerializeField] private BuyButton moduleRButton;
    [SerializeField] private BuyButton levelUpButton;
    // format: Lvl# (no space)
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text maxLevelText;

    [SerializeField] private BuyButton structure1Button;
    [SerializeField] private BuyButton structure2Button;
    [SerializeField] private BuyButton structure3Button;
    [SerializeField] private TMP_Text structure1Text;
    [SerializeField] private TMP_Text structure2Text;
    [SerializeField] private TMP_Text structure3Text;

    [SerializeField] private BuyButton upgradeButton;
    [SerializeField] private TMP_Text upgradeText;
    [SerializeField] private BuyButton upgrade1Button;
    [SerializeField] private BuyButton upgrade2Button;
    [SerializeField] private TMP_Text upgrade1Text;
    [SerializeField] private TMP_Text upgrade2Text;

    [SerializeField] private TMP_Text sellValue;
    [SerializeField] private ModifierCostMultipliers modifierCostMultipliers;

    // Alien modifiers
    private bool front = false;
    private Modifiers modifiers;

    // Update is called once per frame
    void Update()
    {

    }

    // Client side setup
    public void Setup(int player, int moduleCost, int levelCost, StructureInfo first, StructureInfo second, StructureInfo third)
    {
        SetupControls();
        GenerateSendUI(player);
        associatedPlayer = player;
        GameMessenger.Instance.ClientGameEndUpdate += TriggerGameEnd;
        GameMessenger.Instance.WaveUpdate += WaveUpdate;
        GameMessenger.Instance.IncomeMultiplierUpdate += UpdateIncome;
        structure1Button.InitializeCost(first.Cost);
        structure2Button.InitializeCost(second.Cost);
        structure3Button.InitializeCost(third.Cost);
        structure1Text.text = first.Name;
        structure2Text.text = second.Name;
        structure3Text.text = third.Name;
        switch (player)
        {
            case 1:
                GameState.Player1MoneyUpdate += UpdateMoney;
                GameState.Player1IncomeUpdate += UpdateIncome;

                GameState.Player1MoneyUpdate += structure1Button.MoneyUpdate;
                GameState.Player1MoneyUpdate += structure2Button.MoneyUpdate;
                GameState.Player1MoneyUpdate += structure3Button.MoneyUpdate;

                GameState.Player1MoneyUpdate += upgradeButton.MoneyUpdate;
                GameState.Player1MoneyUpdate += upgrade1Button.MoneyUpdate;
                GameState.Player1MoneyUpdate += upgrade2Button.MoneyUpdate;
                break;
            case 2:
                GameState.Player2MoneyUpdate += UpdateMoney;
                GameState.Player2IncomeUpdate += UpdateIncome;

                GameState.Player2MoneyUpdate += structure1Button.MoneyUpdate;
                GameState.Player2MoneyUpdate += structure2Button.MoneyUpdate;
                GameState.Player2MoneyUpdate += structure3Button.MoneyUpdate;

                GameState.Player2MoneyUpdate += upgradeButton.MoneyUpdate;
                GameState.Player2MoneyUpdate += upgrade1Button.MoneyUpdate;
                GameState.Player2MoneyUpdate += upgrade2Button.MoneyUpdate;
                break;
        }
        ButtonSetupUI(player, moduleCost, levelCost);
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
        controls.Player.Deselect.performed += (ctx) => DeselectModule();
        controls.Player.Sell.performed += (ctx) => SellStructure();
    }

    // Client side setup
    public void ClientSetup(Player p, int player, int maxHealth)
    {
        switch (player)
        {
            case 1:
                player1 = p;
                p1Health.Initialize(p, maxHealth);
                break;
            case 2:
                player2 = p;
                p2Health.Initialize(p, maxHealth);
                break;
        }
    }

    void ButtonSetupUI(int player, int moduleCost, int levelCost)
    {
        moduleLButton.InitializeCost(moduleCost);
        moduleRButton.InitializeCost(moduleCost);
        levelUpButton.InitializeCost(levelCost);
        switch (player)
        {
            case 1:
                GameState.Player1MoneyUpdate += moduleLButton.MoneyUpdate;
                GameState.Player1MoneyUpdate += moduleRButton.MoneyUpdate;
                GameState.Player1MoneyUpdate += levelUpButton.MoneyUpdate;
                moduleLButton.MoneyUpdate(GameState.P1Money);
                moduleRButton.MoneyUpdate(GameState.P1Money);
                levelUpButton.MoneyUpdate(GameState.P1Money);
                break;
            case 2:
                GameState.Player2MoneyUpdate += moduleLButton.MoneyUpdate;
                GameState.Player2MoneyUpdate += moduleRButton.MoneyUpdate;
                GameState.Player2MoneyUpdate += levelUpButton.MoneyUpdate;
                moduleLButton.MoneyUpdate(GameState.P2Money);
                moduleRButton.MoneyUpdate(GameState.P2Money);
                levelUpButton.MoneyUpdate(GameState.P2Money);
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
        GameMessenger.Instance.SendAliens(sendIndex, front, modifiers);
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
        attemptedSelectedModule = module;
        switch (associatedPlayer)
        {
            case 1:
                player1.SelectModuleRpc(module - 1, default);
                break;
            case 2:
                player2.SelectModuleRpc(module - 1, default);
                break;
        }
    }

    public void BuildStructure(int structure)
    {
        if (selectedModule == 0) return;
        GameMessenger.Instance.BuildStructure(selectedModule - 1, structure);
    }

    public void DeselectModule()
    {
        if (selectedModule == 0) return;
        selectedModule = 0;
        attemptedSelectedModule = 0;
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

    public void InvalidModuleSelection()
    {
        attemptedSelectedModule = 0;
    }

    public void OpenModuleUI()
    {
        if (selectedModule == 0)
        {
            selectedModule = attemptedSelectedModule;
            attemptedSelectedModule = 0;
        }
        int money = associatedPlayer == 1 ? GameState.P1Money : GameState.P2Money;
        structure1Button.MoneyUpdate(money);
        structure2Button.MoneyUpdate(money);
        structure3Button.MoneyUpdate(money);

        shipOptionsUI.SetActive(false);
        structureUI.SetActive(false);
        moduleUI.SetActive(true);
    }

    public void OpenStructureUI(int sellValue, StructureUpgradeInfo upgradeInfo)
    {
        if (selectedModule == 0)
        {
            selectedModule = attemptedSelectedModule;
            attemptedSelectedModule = 0;
        }
        shipOptionsUI.SetActive(false);
        moduleUI.SetActive(false);
        this.sellValue.text = $"${sellValue}";
        if (upgradeInfo.UpgradeCount == 1)
        {
            upgradeText.text = upgradeInfo.Upgrade1.Name;
            upgradeButton.InitializeCost(upgradeInfo.Upgrade1.Cost);
            upgradeButton.MoneyUpdate(associatedPlayer == 1 ? GameState.P1Money : GameState.P2Money);
            upgrade2UI.SetActive(false);
            noUpgradeUI.SetActive(false);
            upgrade1UI.SetActive(true);
        }
        else if (upgradeInfo.UpgradeCount == 2)
        {
            upgrade1Text.text = upgradeInfo.Upgrade1.Name;
            upgrade1Button.InitializeCost(upgradeInfo.Upgrade1.Cost);
            upgrade1Button.MoneyUpdate(associatedPlayer == 1 ? GameState.P1Money : GameState.P2Money);
            upgrade2Text.text = upgradeInfo.Upgrade2.Name;
            upgrade2Button.InitializeCost(upgradeInfo.Upgrade2.Cost);
            upgrade2Button.MoneyUpdate(associatedPlayer == 1 ? GameState.P1Money : GameState.P2Money);
            upgrade1UI.SetActive(false);
            noUpgradeUI.SetActive(false);
            upgrade2UI.SetActive(true);
        }
        else
        {
            upgrade1UI.SetActive(false);
            upgrade2UI.SetActive(false);
            noUpgradeUI.SetActive(true);
        }
        structureUI.SetActive(true);
    }

    public void UpgradeStructure(bool right)
    {
        if (selectedModule == 0) return;
        GameMessenger.Instance.UpgradeStructure(selectedModule - 1, right);
    }

    public void SellStructure()
    {
        if (selectedModule == 0) return;
        GameMessenger.Instance.SellStructure(selectedModule - 1);
    }

    public void GenerateSendUI(int player)
    {
        float height = alienSendButtonPrefab.GetComponent<RectTransform>().rect.height;
        RectTransform container = alienSendsContainer.GetComponent<RectTransform>();
        container.sizeDelta = new Vector2(container.sizeDelta.x, height * alienSends.sends.Count);
        for (int i = 0; i < alienSends.sends.Count; i++)
        {
            GameObject g = Instantiate(alienSendButtonPrefab, alienSendsContainer.transform);
            RectTransform r = g.GetComponent<RectTransform>();
            r.anchoredPosition = new Vector2(r.anchoredPosition.x, -i * height);
            AlienSendButton sendButton = g.GetComponent<AlienSendButton>();
            sendButton.InitializeSend(alienSends.sends[i]);
            int index = i;
            sendButton.SendButton.onClick.AddListener(() => SendAliens(index));
            buttons.Add(sendButton);
            if (player == 1)
            {
                GameState.Player1MoneyUpdate += sendButton.MoneyUpdate;
            }
            else
            {
                GameState.Player2MoneyUpdate += sendButton.MoneyUpdate;
            }
            g.SetActive(false);
        }
    }

    public void WaveUpdate(int wave)
    {
        int money = associatedPlayer == 1 ? GameState.P1Money : GameState.P2Money;
        foreach (AlienSendButton b in buttons)
        {
            if (!b.gameObject.activeSelf && b.UnlockWave <= wave)
            {
                b.gameObject.SetActive(true);
                b.MoneyUpdate(money);
            }
        }
    }

    public void UpdateModuleCost(int cost)
    {
        moduleLButton.InitializeCost(cost);
        moduleRButton.InitializeCost(cost);
        int money = associatedPlayer == 1 ? GameState.P1Money : GameState.P2Money;
        moduleLButton.MoneyUpdate(money);
        moduleRButton.MoneyUpdate(money);
    }

    public void UpdateLevelCost(int cost)
    {
        levelUpButton.InitializeCost(cost);
        int money = associatedPlayer == 1 ? GameState.P1Money : GameState.P2Money;
        levelUpButton.MoneyUpdate(money);
    }

    public void MaxModules()
    {
        moduleLButton.gameObject.SetActive(false);
        moduleRButton.gameObject.SetActive(false);
    }

    public void MaxLevel()
    {
        levelUpButton.gameObject.SetActive(false);
        maxLevelText.gameObject.SetActive(true);
    }

    public void UpdateLevel(int level)
    {
        levelText.text = $"Lvl{level}";
    }

    public void UpdateIncome(float newMultiplier)
    {
        foreach (AlienSendButton button in buttons)
        {
            button.IncomeUpdate(newMultiplier);
        }
    }

    public void UpdateFront(bool front)
    {
        this.front = front;
    }

    public void ToggleModifier(bool on, ModifierButton.ModifierType m)
    {
        switch (m)
        {
            case ModifierButton.ModifierType.Shield:
                modifiers.shielded = on;
                break;
            case ModifierButton.ModifierType.Berserk:
                modifiers.berserk = on;
                break;
            case ModifierButton.ModifierType.Invisible:
                modifiers.invisible = on;
                break;
            case ModifierButton.ModifierType.Regenerating:
                modifiers.regenerating = on;
                break;
        }
        UpdateSendPrices();
    }

    public void UpdateSendPrices()
    {
        foreach (AlienSendButton button in buttons)
        {
            button.ChangeCostMultiplier(GameController.ModifierCostMultiplier(modifiers, modifierCostMultipliers));
            button.MoneyUpdate(associatedPlayer == 1 ? GameState.P1Money : GameState.P2Money);
        }
    }

    public void TriggerOverdrive()
    {
        GameMessenger.Instance.TriggerOverdrive();
    }

    public void TriggerShield()
    {

    }
}