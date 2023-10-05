using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    [CreateAssetMenu(fileName = "LevelDataInfo", menuName = "LevelData")]
    public class LevelDataInfo : ScriptableObject
    {
        [System.Serializable]
        public struct LevelInfo
        {
            public Vector2Int gridSize;
            public TextAsset levelCsv;
            public List<WordInfo> words;
        }

        [System.Serializable]
        public struct WordInfo
        {
            public string word;
        }
        
        public List<LevelInfo> levelInfo;

    }
}
