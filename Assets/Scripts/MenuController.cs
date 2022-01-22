using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{

    GameObject AnyKeyTextObject;
    bool MenuReactive = true;
    Coroutine BlinkTextRoutine;
    float BlinkTextDelay = 0.75f;
    TextMeshProUGUI HighScoreText;
    dreamloLeaderBoard Dreamlo;

    public void Awake()
    {

        AnyKeyTextObject = GameObject.Find("Play Button Text");
        if (AnyKeyTextObject == null)
        {
            Debug.LogError("Play Button Text not found");
        }

        // Load Best Time
        GameObject HighScoreTextObject = GameObject.Find("HighScoreTextObject");
        HighScoreText = HighScoreTextObject.GetComponent<TextMeshProUGUI>();

        Dreamlo = dreamloLeaderBoard.GetSceneDreamloLeaderboard();
        Dreamlo.GetScores();

    }

    public void Start()
    {
        ResetBlink();
        GameAssets.Sound.MenuMusic.Play();
    }

    IEnumerator BlinkPlayText()
    {
        while (true)
        {
            yield return new WaitForSeconds(BlinkTextDelay);
            AnyKeyTextObject.SetActive(false);
            yield return new WaitForSeconds(BlinkTextDelay);
            AnyKeyTextObject.SetActive(true);
        }
    }

    void Update()
    {
        if (!MenuReactive) return;

        if (HighScoreText.text == "")
        {
            if (Dreamlo.highScores != "")
            {
                UpdateScore(Dreamlo.highScores);
            }
        }

        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            MenuReactive = false;
            GameAssets.Sound.MenuMusic.Stop();
            StartCoroutine("PlayGameSequence");

            return;
        }
    }


    void UpdateScore(string ScoreText)
    {
        string[] TextArray = ScoreText.Split('|');
        string OutputText = TextArray[0] + " : " + GameAssets.FormatTimeInMS(Convert.ToSingle(TextArray[1]));
    }

    void ResetBlink()
    {
        if (BlinkTextRoutine != null) StopCoroutine(BlinkTextRoutine);
        BlinkTextRoutine = StartCoroutine("BlinkPlayText");
    }

    IEnumerator PlayGameSequence()
    {
        BlinkTextDelay = 0.1f;
        ResetBlink();
        GameAssets.Sound.PlaySound.Play();
        yield return new WaitForSeconds(2);
        GameAssets.Sound.Song1.Play();
        GameObject.Find("TimerObject").GetComponent<TimeController>().StartTimer();
        SceneManager.LoadScene(1);
    }

}
