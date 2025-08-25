using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SessionManager : MonoBehaviour
{
    private static string username = "Guest";

    private static SessionManager _instance;
    public static SessionManager Instance => _instance;

    ISession activeSession;
    ISession ActiveSession
    {
        get => activeSession;
        set
        {
            activeSession = value;
            Debug.Log($"Active session: {activeSession}");
        }
    }

    private List<ISessionInfo> sessions;
    public List<ISessionInfo> Sessions => sessions;

    public string JoinCode => ActiveSession.Code;
    public int PlayerCount => ActiveSession.PlayerCount;

    public event Action SignInComplete;
    public event Action PlayerJoined;
    public event Action SessionsLoaded;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }
        _instance = this;

        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed In: " + AuthenticationService.Instance.PlayerId);
            SignInComplete?.Invoke();
            FetchSessions();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public async Task StartSessionAsHost(string sessionName = "")
    {
        if (sessionName == "")
        {
            sessionName = "Unnamed Lobby";
        }
        SessionOptions options = new SessionOptions
        {
            Name = sessionName,
            MaxPlayers = 2,
            IsLocked = false,
            IsPrivate = false,
            SessionProperties = new Dictionary<string, SessionProperty>
            {
                { "hostName", new SessionProperty(username) },
            }
        }.WithRelayNetwork();

        ActiveSession = await MultiplayerService.Instance.CreateSessionAsync(options);
        ActiveSession.CurrentPlayer.SetProperty("username", new PlayerProperty(username));
        ActiveSession.CurrentPlayer.SetProperty("clientId", new PlayerProperty(NetworkManager.Singleton.LocalClientId.ToString()));

        ActiveSession.PlayerJoined += (val) => PlayerJoined?.Invoke();
        Debug.Log($"Session {ActiveSession.Id} created! Join code: {ActiveSession.Code}");

        NetworkManager.Singleton.SceneManager.LoadScene("StructureSelection", LoadSceneMode.Single);
    }

    public async Task JoinSessionById(string id)
    {
        ActiveSession = await MultiplayerService.Instance.JoinSessionByIdAsync(id);
        ActiveSession.CurrentPlayer.SetProperty("username", new PlayerProperty(username));
        ActiveSession.CurrentPlayer.SetProperty("clientId", new PlayerProperty(NetworkManager.Singleton.LocalClientId.ToString()));

        Debug.Log($"Joined session {activeSession.Id}");
    }

    public async Task JoinSessionByCode(string code)
    {
        activeSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);
        ActiveSession.CurrentPlayer.SetProperty("username", new PlayerProperty(username));
        ActiveSession.CurrentPlayer.SetProperty("clientId", new PlayerProperty(NetworkManager.Singleton.LocalClientId.ToString()));

        Debug.Log($"Joined session {activeSession.Id}");
    }

    public async Task FetchSessions()
    {
        QuerySessionsOptions options = new QuerySessionsOptions();
        QuerySessionsResults results = await MultiplayerService.Instance.QuerySessionsAsync(options);
        sessions = new List<ISessionInfo>();
        foreach (ISessionInfo session in results.Sessions)
        {
            if (session.AvailableSlots > 0 && !session.IsLocked)
            {
                sessions.Add(session);
            }
        }
        SessionsLoaded?.Invoke();
    }
}