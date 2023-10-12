using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    [CreateAssetMenu(fileName = "DifficultyInfo", menuName = "DifficultyData")]
    public class DifficultyDataInfo : ScriptableObject
    {
        [System.Serializable]
        public struct DifficultyInfo
        {
            public GameController.CurrDifficulty difficulty;
            public TextAsset wordTextFile;
            public int minQuesLetter;
        }

        public List<DifficultyInfo> difficultyInfos;
    }
}