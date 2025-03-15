using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameEndScreen : MonoBehaviour
{
    [SerializeField] private float maxOpacity;
    [SerializeField] private float sequenceTime;
    [SerializeField] private Image background;
    [SerializeField] private GameObject victoryText;
    [SerializeField] private GameObject defeatText;
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
        yield return null;
    }
}
