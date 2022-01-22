using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeController : MonoBehaviour
{

    float Timer = 0f;
    bool TimerActive = false;
    bool TimerFinished = false;
    float DeltaTime;
    TextMeshProUGUI Text;

    void Start()
    {
        DontDestroyOnLoad(GameObject.Find("TimerCanvas"));
        Text = GameObject.Find("TimerText").GetComponent<TextMeshProUGUI>();
    }

    void FixedUpdate()
    {
        if (TimerActive)
        {
            if (!TimerFinished)
            {

                // Increment Timer
                Timer += Time.deltaTime * 1000;

                // Update Timer
                Text.text = GameAssets.FormatTimeInMS(Timer);

            }
            
        }
    }

    public void StartTimer()
    {
        Timer = 0f;
        TimerActive = true;
    }

    public void HideTimer()
    {
        Text.color = new Color(0, 0, 0, 0);
    }

    public void StopTimer()
    {
        TimerFinished = true;
    }
}
