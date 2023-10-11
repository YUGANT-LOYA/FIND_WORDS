using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    [CreateAssetMenu(fileName = "DifficultyInfo", menuName = "DifficultyData")]
    public class LevelDataInfo : ScriptableObject
    {
        [System.Serializable]
        public struct LevelInfo
        {
            public GameController.CurrDifficulty difficulty;
            public TextAsset wordTextFile;
            public int minQuesLetter;
        }
        
        public List<LevelInfo> levelInfo;

    }
}
