using UnityEngine;
using UnityEngine.UI;

public class StructureSelectionButton : MonoBehaviour
{
    [SerializeField] private int structure;
    public int Structure => structure;
    private bool selected = false;
    [SerializeField] private GameObject icon;

    public void OnClick()
    {
        StructureSelectionMessenger.Instance.SelectStructure(!selected, structure);
        if (selected)
        {
            selected = false;
            icon.SetActive(false);
        }
    }

    public void Select()
    {
        selected = true;
        icon.SetActive(true);
    }
}