using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wave", menuName = "Scriptable Objects/Wave")]
public class Wave : ScriptableObject
{
    public List<WaveComponent> backLineComponents = new List<WaveComponent>();
    public List<WaveComponent> frontLineComponents = new List<WaveComponent>();
}

[System.Serializable]
public class WaveComponent
{
    public GameObject alien;
    public int count;
    public float spawnDelay;
    public float afterSpawnDelay;
    public Modifiers modifiers;
}