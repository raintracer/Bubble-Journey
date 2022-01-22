using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsController : MonoBehaviour
{

    void Awake()
    {
        GameAssets.Sound.Song1.Stop();
        GameAssets.Sound.CreditMusic.Play();
        GameObject.Find("TimerObject").GetComponent<TimeController>().StopTimer();
    }

}
