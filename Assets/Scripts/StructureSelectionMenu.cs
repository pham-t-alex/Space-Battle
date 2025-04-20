using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StructureSelectionMenu : MonoBehaviour
{
    [SerializeField] private List<StructureSelectionButton> buttons = new List<StructureSelectionButton>();
    public StructureSelectionButton StructureButton(int button) => buttons[button];

    private static StructureSelectionMenu instance;
    public static StructureSelectionMenu Instance => instance;

    [SerializeField] private GameObject start;

    private void Awake()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsClient)
        {
            Destroy(gameObject);
        }
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
