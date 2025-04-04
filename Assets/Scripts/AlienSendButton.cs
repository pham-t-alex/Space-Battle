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
        income.text = $"{send.incomeChange}";
        unlockWave = send.unlockWave;
        InitializeCost(send.cost);
    }
}
