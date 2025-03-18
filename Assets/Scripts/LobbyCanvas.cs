using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

public class LobbyCanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Button startSession;
    [SerializeField] private Button joinSession;
    [SerializeField] private Button start;

    private string sessionCode = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SessionManager.Instance.SignInComplete += AfterSignInActivateButtons;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdatePlayerCount()
    {
        playerCountText.text = "Player count: " + SessionManager.Instance.PlayerCount;
        if (SessionManager.Instance.PlayerCount >= 2)
        {
            start.interactable = true;
        }
    }

    public void AfterSignInActivateButtons()
    {
        startSession.interactable = true;
        joinSession.interactable = true;
    }

    public async void StartSessionAsHost()
    {
        await SessionManager.Instance.StartSessionAsHost();
        NetworkManager.Singleton.OnClientConnectedCallback += (val) => UpdatePlayerCount();
        NetworkManager.Singleton.OnClientDisconnectCallback += (val) => UpdatePlayerCount();
        UpdatePlayerCount();
        codeText.text = $"Code: {SessionManager.Instance.JoinCode}";
        start.gameObject.SetActive(true);
    }

    public async void JoinSession()
    {
        await SessionManager.Instance.JoinSessionByCode(sessionCode);
        NetworkManager.Singleton.OnClientConnectedCallback += (val) => UpdatePlayerCount();
        NetworkManager.Singleton.OnClientDisconnectCallback += (val) => UpdatePlayerCount();
        UpdatePlayerCount();
    }

    public void SetSessionCode(string text)
    {
        sessionCode = text;
    }

    public void StartGame()
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count < 2) return;
        NetworkManager.Singleton.SceneManager.LoadScene("SpaceBattle", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
