using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class AlienSendButton : BuyButton
{
    [SerializeField] private TMP_Text alien;
    [SerializeField] private TMP_Text count;
    [SerializeField] private TMP_Text income;
    private int unlockWave;
    public int UnlockWave => unlockWave;
    private float internalIncome;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitializeSend(AlienSend send)
    {
        alien.text = send.alien.GetComponent<Alien>().AlienName;
        count.text = $"x{send.count.ToString()}";
        internalIncome = send.incomeChange;
        income.text = internalIncome.ToString("F1");
        if (internalIncome > 0) income.color = Color.green;
        else if (internalIncome < 0) income.color = Color.red;
        unlockWave = send.unlockWave;
        InitializeCost(send.cost);
    }

    public void IncomeUpdate(float multiplier)
    {
        float newIncome = (internalIncome >= 0) ? internalIncome * multiplier :
            internalIncome / multiplier;
        income.text = newIncome.ToString("F1");
    }
}
