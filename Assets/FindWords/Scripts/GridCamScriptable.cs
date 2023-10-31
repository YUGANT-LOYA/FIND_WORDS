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
        public Vector3 gridPlacementContainerPos, gridPlacementContainerSize;
        public List<GridDataInfo> gridDataInfos;
    }

    [Serializable]
    public struct GridDataInfo
    {
        public Vector2Int gridSize;
        public float camOrthographicSize;
        public Vector3 camPos;
        public float gridScale;
        public float quesBlockScale, quesSpacing;
        public Vector3 queContainerPos;
    }

    [Serializable]
    public struct QuesDataInfo
    {
    }

    public List<CamGridSizeStruct> camGridInfoList;
}