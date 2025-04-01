using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class AlienSendButton : MonoBehaviour
{
    [SerializeField] private TMP_Text alien;
    [SerializeField] private TMP_Text count;
    [SerializeField] private TMP_Text cost;
    [SerializeField] private TMP_Text income;
    [SerializeField] private Button button;
    public Button SendButton => button;
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
        cost.text = $"${send.cost}";
        income.text = $"{send.incomeChange}";
        unlockWave = send.unlockWave;
    }
}
