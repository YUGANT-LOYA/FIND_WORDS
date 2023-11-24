using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace YugantLoyaLibrary.FindWords
{
    [CreateAssetMenu(fileName = "Coin Scriptable", menuName = "Coin System")]
    public class CoinHandlerScriptable : ScriptableObject
    {
        [Serializable]
        public struct WordCompleteCoinData
        {
            [FormerlySerializedAs("gridCount")] public int quesLetterCount;
            public int coinPerWord;
        }

        public List<WordCompleteCoinData> wordCompleteCoinDataList;
        public List<int> quesUnlockDataList;
    }
}