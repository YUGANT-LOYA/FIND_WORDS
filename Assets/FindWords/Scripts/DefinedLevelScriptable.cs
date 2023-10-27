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
        public int gridSize;
        [TextArea(5, 5)] public string gridData;
        public List<string> hintList;
    }

    public List<DefinedLevelInfo> definedLevelInfoList;
}