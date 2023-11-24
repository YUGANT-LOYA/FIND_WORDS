using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    [CreateAssetMenu(fileName = "MainDictionary", menuName = "EnglishDictionary")]
    public class MainDictionary : ScriptableObject
    {
        [Serializable]
        public struct MainDictionaryInfo
        {
            public int wordLength;
            public List<WordLengthDetailedInfo> wordsInfo;
        }

        [Serializable]
        public struct WordLengthDetailedInfo
        {
            public char wordStartChar;
            public List<string> wordList;
        }

        public List<MainDictionaryInfo> dictInfoList;
    }
}