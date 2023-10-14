using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Predefined_Levels", menuName = "Predefined Levels")]
public class DefinedLevelScriptable : ScriptableObject
{
    [Serializable]
    public struct DefinedLevelInfo
    {
        public bool shouldShuffle;
        //[TextArea(5, 5)] public string gridsData;
    }

    public TextAsset helperTextFile;
    public List<DefinedLevelInfo> definedLevelInfoList;
}