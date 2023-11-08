using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LionStudios.Suite.Analytics;

public class LionStudiosManager : MonoBehaviour
{
    private void Awake()
    {
        LionAnalytics.GameStart();
    }

    public static void LevelStart(int levelNo, int attemptNum, int? score = null)
    {
        LionAnalytics.LevelStart(levelNo, attemptNum, score);
    } 
    
    public static void LevelComplete(int levelNo, int attemptNum, int? score = null)
    {
        LionAnalytics.LevelComplete(levelNo, attemptNum, score);
    }
    
    public static void LevelFail(int levelNo, int attemptNum, int? score = null)
    {
        LionAnalytics.LevelFail(levelNo, attemptNum, score);
    } 
    
    public static void LevelRestart(int levelNo, int attemptNum, int? score = null)
    {
        LionAnalytics.LevelRestart(levelNo, attemptNum, score);
    }


}
