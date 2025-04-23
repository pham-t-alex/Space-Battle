using UnityEngine;

[CreateAssetMenu(fileName = "ModifierCostMultipliers", menuName = "Scriptable Objects/ModifierCostMultipliers")]
public class ModifierCostMultipliers : ScriptableObject
{
    public float shieldMultiplier = 1;
    public float berserkMultiplier = 1;
    public float invisMultiplier = 1;
    public float regenMultiplier = 1;
}
