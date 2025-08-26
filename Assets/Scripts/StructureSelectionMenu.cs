using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StructureSelectionMenu : MonoBehaviour
{
    [SerializeField] private List<StructureSelectionButton> buttons = new List<StructureSelectionButton>();
    public StructureSelectionButton StructureButton(int button) => buttons[button];

    private static StructureSelectionMenu instance;
    public static StructureSelectionMenu Instance => instance;

    [SerializeField] private GameObject ready;
    [SerializeField] private TMP_Text p1Text;
    [SerializeField] private TMP_Text p2Text;
    [SerializeField] private Image p1Ready;
    [SerializeField] private Image p2Ready;

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

    public void TryReady()
    {
        StructureSelectionMessenger.Instance.TryReady();
    }

    public void ReadyLock()
    {
        ready.GetComponent<Button>().interactable = false;
        foreach (StructureSelectionButton b in buttons)
        {
            b.GetComponent<Button>().interactable = false;
        }
    }
}
