using System.Collections.Generic;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;

public class LobbySelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject lobbySelectionBoxPrefab;
    [SerializeField] private GameObject contentBox;
    [SerializeField] private TMP_Text userText;
    [SerializeField] private TMP_InputField lobbyNameInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SessionManager.Instance.SessionsLoaded += UpdateLobbies;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateLobbies()
    {
        RectTransform content = contentBox.GetComponent<RectTransform>();
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        List<ISessionInfo> sessions = SessionManager.Instance.Sessions;

        content.sizeDelta = new Vector2(content.sizeDelta.x, 10 + (sessions.Count * 110));
        for (int i = 0; i < sessions.Count; i++)
        {
            ISessionInfo session = sessions[i];

            GameObject g = Instantiate(lobbySelectionBoxPrefab, content);
            RectTransform rt = g.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -10 - (110 * i));
            
            LobbySelectionBox lobby = g.GetComponent<LobbySelectionBox>();
            lobby.LobbyNameText.text = session.Name;
            lobby.JoinButton.onClick.AddListener(() =>
            {
                SessionManager.Instance.JoinSessionById(session.Id);
            });
        }
    }

    public void Refresh()
    {
        SessionManager.Instance.FetchSessions();
    }

    public void CreateLobby()
    {
        SessionManager.Instance.StartSessionAsHost(lobbyNameInput.text);
    }
}
