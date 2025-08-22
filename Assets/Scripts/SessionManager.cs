using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Multiplayer;
using System.Threading.Tasks;

public class SessionManager : MonoBehaviour
{
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

    private IList<ISessionInfo> sessions;
    public IList<ISessionInfo> Sessions => sessions;

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
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public async Task StartSessionAsHost()
    {
        SessionOptions options = new SessionOptions
        {
            MaxPlayers = 2,
            IsLocked = false,
            IsPrivate = false,
        }.WithRelayNetwork();

        ActiveSession = await MultiplayerService.Instance.CreateSessionAsync(options);
        ActiveSession.PlayerJoined += (val) => PlayerJoined?.Invoke();
        Debug.Log($"Session {ActiveSession.Id} created! Join code: {ActiveSession.Code}");
    }

    async Task JoinSessionById(string id)
    {
        ActiveSession = await MultiplayerService.Instance.JoinSessionByIdAsync(id);
        Debug.Log($"Joined session {activeSession.Id}");
    }

    public async Task JoinSessionByCode(string code)
    {
        activeSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);
        Debug.Log($"Joined session {activeSession.Id}");
    }

    //async Task KickPlayer(string id)
    //{
    //    if (!ActiveSession.IsHost) return;
    //    await ActiveSession.AsHost().RemovePlayerAsync(id);
    //}

    public async Task FetchSessions()
    {
        QuerySessionsOptions options = new QuerySessionsOptions
        {
            Count = 10
        };
        QuerySessionsResults results = await MultiplayerService.Instance.QuerySessionsAsync(options);
        sessions = results.Sessions;
        SessionsLoaded?.Invoke();
    }
}