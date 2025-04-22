using UnityEngine;

public class ModifierButton : MonoBehaviour
{
    public enum ModifierType
    {
        Shield
    }
    [SerializeField] private ModifierType modifierType;

    public void OnClick(bool on)
    {
        GameUI.Instance.ToggleModifier(on, modifierType);
    }
}
