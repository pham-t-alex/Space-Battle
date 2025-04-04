using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Rendering.UI;

public class BuyButton : MonoBehaviour
{
    [SerializeField] private TMP_Text costText;
    private int cost;
    [SerializeField] private Button button;
    public Button SendButton => button;

    private bool buttonEnabled = false;

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
            buttonEnabled = true;
            button.interactable = true;
            costText.color = Color.white;
        }
        else
        {
            if (!buttonEnabled) return;
            buttonEnabled = false;
            button.interactable = false;
            costText.color = Color.red;
        }
    }
}