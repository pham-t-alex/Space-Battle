using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StructureSelectionController : MonoBehaviour
{
    private static StructureSelectionController instance;
    public static StructureSelectionController Instance => instance;

    [SerializeField] private GameObject start;

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
        start.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TryStart()
    {
        if (StructureSelection.p1Structures.Count == 3 && StructureSelection.p2Structures.Count == 3)
            NetworkManager.Singleton.SceneManager.LoadScene("SpaceBattle", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void SelectStructure(ulong clientId, bool selected, int structure)
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
}