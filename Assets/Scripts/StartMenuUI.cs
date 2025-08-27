using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class StartMenuUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        if (usernameInput.text != "")
        {
            SessionManager.Username = usernameInput.text;
            SceneManager.LoadScene("LobbySelection");
        }
    }
}
