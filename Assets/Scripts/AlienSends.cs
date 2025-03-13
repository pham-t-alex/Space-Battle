using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AlienSends", menuName = "Scriptable Objects/AlienSends")]
public class AlienSends : ScriptableObject
{
    public List<AlienSend> sends = new List<AlienSend>();
}

[System.Serializable]
public class AlienSend
{
    public int unlockWave;
    public int cost;
    public GameObject alien;
    public int count = 1;
    public int incomeChange = 0;
    // delay before a send
    public float delayBeforeSend;
    // delay between sends (only when count > 1)
    public float delayBetweenSends;
}