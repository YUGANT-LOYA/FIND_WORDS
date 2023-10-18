using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace YugantLoyaLibrary.FindWords
{
    [CreateAssetMenu(fileName = "QuestionSizeInfo", menuName = "Question Size Data")]
    public class QuestionSizeInfo : ScriptableObject
    {
        [System.Serializable]
        public struct SizeInfo
        {
            public int gridSize;
            public int minQuesLetter,maxQuesLetter;
        }

        public List<SizeInfo> difficultyInfos;
    }
}