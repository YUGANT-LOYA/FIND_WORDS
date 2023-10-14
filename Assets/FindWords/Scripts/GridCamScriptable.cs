using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "GridCameraCalculation", menuName = "Grid And Camera Calculation")]
public class GridCamScriptable : ScriptableObject
{
    [Serializable]
    public struct CamGridSizeStruct
    {
        public Vector2Int screenSize;
        public List<GridDataInfo> gridDataInfos;
    }

    [Serializable]
    public struct GridDataInfo
    {
        public Vector2Int gridSize;
        public float camOrthographicSize;
        public Vector3 camPos;
        public float gridScale;
        public List<QuesDataInfo> queBlockInfoList;
    }

    [Serializable]
    public struct QuesDataInfo
    {
        public int numOfQues;
        public float quesBlockScale;
        public Vector3 queContainerPos;
    }

    public List<CamGridSizeStruct> camGridInfoList;
}