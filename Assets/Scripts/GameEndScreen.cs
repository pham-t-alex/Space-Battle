using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections.Generic;

public class GameEndScreen : MonoBehaviour
{
    [SerializeField] private float maxOpacity;
    [SerializeField] private float sequenceTime;
    [SerializeField] private Image background;
    [SerializeField] private GameObject victoryText;
    [SerializeField] private GameObject defeatText;
    [SerializeField] private GameObject playAgainButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerGameEnd(bool victorious)
    {
        StartCoroutine(GameEndSequence(victorious));
    }

    IEnumerator GameEndSequence(bool victorious)
    {
        float elapsed = 0f;
        while (elapsed < sequenceTime)
        {
            elapsed += Time.deltaTime;
            background.color = new Color(background.color.r, background.color.g, background.color.b, maxOpacity * (Mathf.Min(elapsed, sequenceTime) / sequenceTime));
            yield return null;
        }
        if (victorious)
        {
            victoryText.SetActive(true);
        }
        else
        {
            defeatText.SetActive(true);
        }
        if (NetworkManager.Singleton.IsServer) playAgainButton.SetActive(true);
        yield return null;
    }

    public void PlayAgain()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (NetworkManager.Singleton.ConnectedClientsIds.Count < 2) return;

        StructureSelection.p1Structures = new List<int>();
        StructureSelection.p2Structures = new List<int>();
        NetworkManager.Singleton.SceneManager.LoadScene("StructureSelection", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
