using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MainDictionary", menuName = "EnglishDictionary")]
public class MainDictionary : ScriptableObject
{
    [Serializable]
    public struct MainDictionaryInfo
    {
        public int wordLength;
        public TextAsset wordText;
    }

    public List<MainDictionaryInfo> dictInfoList;
}