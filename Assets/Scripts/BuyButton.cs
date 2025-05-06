using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuyButton : MonoBehaviour
{
    [SerializeField] private TMP_Text costText;
    private int cost;
    [SerializeField] private Button button;
    public Button SendButton => button;

    private bool buttonEnabled => button.interactable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeCost(int cost)
    {
        this.cost = cost;
        costText.text = $"${cost}";
    }

    public void MoneyUpdate(int money)
    {
        if (money >= cost)
        {
            if (buttonEnabled) return;
            button.interactable = true;
            costText.color = Color.white;
        }
        else
        {
            if (!buttonEnabled) return;
            button.interactable = false;
            costText.color = Color.red;
        }
    }
}