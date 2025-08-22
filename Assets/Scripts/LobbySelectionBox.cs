using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LobbySelectionBox : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyNameText;
    public TMP_Text LobbyNameText => lobbyNameText;
    [SerializeField] private TMP_Text hostText;
    public TMP_Text HostText => hostText;
    [SerializeField] private Button joinButton;
    public Button JoinButton => joinButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
