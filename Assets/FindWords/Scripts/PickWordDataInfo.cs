using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    [CreateAssetMenu(fileName = "PickDataInfo", menuName = "Data  Picking Info")]
    public class PickWordDataInfo : ScriptableObject
    {
        [Serializable]
        public struct PickingDataInfo
        {
            public int quesLetterCount;
            public List<string> wordList;
        }

        public List<PickingDataInfo> pickDataInfoList;
    }
}