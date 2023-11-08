using System;
using UnityEngine;
using GameAnalyticsSDK;
using System.Collections.Generic;

public class GAScript : MonoBehaviour
{
     void InIt() => GameAnalytics.Initialize();

    private void Awake()
    {
        InIt();
    }

    public static void LevelStart(string levelName)
    {
        Debug.Log("GA Script Level Start  : " + levelName);
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, levelName);
    }

    public static void LevelEnd(bool isWin, string levelName)
    {
        if (isWin) LevelCompleted(levelName);
        else LevelFail(levelName);
    }

    static void LevelFail(string levelName)
    {
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, levelName);
    }

    static void LevelCompleted(string levelName)
    {
        Debug.Log("GA Script Level Complete  : " + levelName);
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, levelName);
    }
}