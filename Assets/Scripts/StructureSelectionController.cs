using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StructureSelectionController : MonoBehaviour
{
    private static StructureSelectionController instance;
    public static StructureSelectionController Instance => instance;

    [SerializeField] private GameObject start;

    private bool p1Ready = false;
    private bool p2Ready = false;

    private void Awake()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            Destroy(this);
        }
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TryStart()
    {
        if (p1Ready && p2Ready) NetworkManager.Singleton.SceneManager.LoadScene("SpaceBattle", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void SelectStructure(ulong clientId, bool selected, int structure)
    {
        bool player1;
        if (clientId == NetworkManager.Singleton.ConnectedClientsIds[0])
        {
            player1 = true;
            if (p1Ready) return;
        }
        else if (clientId == NetworkManager.Singleton.ConnectedClientsIds[1])
        {
            player1 = false;
            if (p2Ready) return;
        }
        else return;

        if (selected)
        {
            if (player1)
            {
                if (StructureSelection.p1Structures.Count > 2 || StructureSelection.p1Structures.Contains(structure)) return;
                StructureSelection.p1Structures.Add(structure);
                StructureSelectionMessenger.Instance.StructureSelected(clientId, structure);
            }
            else
            {
                if (StructureSelection.p2Structures.Count > 2 || StructureSelection.p2Structures.Contains(structure)) return;
                StructureSelection.p2Structures.Add(structure);
                StructureSelectionMessenger.Instance.StructureSelected(clientId, structure);
            }
        }
        else
        {
            if (player1)
            {
                StructureSelection.p1Structures.Remove(structure);
            }
            else
            {
                StructureSelection.p2Structures.Remove(structure);
            }
        }
    }

    public void TryReady(ulong clientId)
    {
        bool player1;
        if (clientId == NetworkManager.Singleton.ConnectedClientsIds[0])
        {
            player1 = true;
        }
        else if (clientId == NetworkManager.Singleton.ConnectedClientsIds[1])
        {
            player1 = false;
        }
        else return;

        if (player1)
        {
            if (StructureSelection.p1Structures.Count != 3) return;
            p1Ready = true;
            StructureSelectionMessenger.Instance.ReadyLock(clientId);
        }
        else
        {
            if (StructureSelection.p2Structures.Count != 3) return;
            p2Ready = true;
            StructureSelectionMessenger.Instance.ReadyLock(clientId);
        }
        if (p1Ready && p2Ready) start.SetActive(true);
    }
}