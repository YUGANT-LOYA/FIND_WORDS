using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace YugantLoyaLibrary.FindWords
{
    public class Level : MonoBehaviour
    {
        [Header("Main Info")] private Camera _cam;
        [HideInInspector] public Vector2Int gridSize;
        private float _camOrthographicSize;
        public Material gridMaterial;
        private readonly int[] _randomScreenPointArr = { -1, 1 };
        public Ease gridPlacementEase;
        Vector3 _gridContainerSize, _defaultStartPos;
        public float defaultStartYPos = 1.5f;

        [Tooltip("The Grid Should come inside camera's orthographic size, so there should be offset from one side")]
        public float camGridOffset = 0.5f;

        [Tooltip("The Distance outside the screen from where the grid should come and place in the grid")]
        public float distanceFromScreen = 5f;

        [Tooltip("Time to place Grid in its original Position")]
        public float timeToPlaceGrid = 0.4f, timeToWaitForEachGrid = 0.1f;

        public Transform quesGridTrans;
        [SerializeField] private Transform gridContainer;
        private float _currGridWidth, _currGridHeight, _currGridSize, _currQuesSize;
        [SerializeField] private float gridSpacing = 0.1f, quesSpacing = 0.2f;

        private void Awake()
        {
            _cam = Camera.main;
        }

        public void StartInit()
        {
            //Debug.Log("Level StartInit Called !");
            Debug.Log($"Aspect Ratio : {_cam.aspect} , Width : {Screen.width} , Height  : {Screen.height}");
            SetCameraPos();
            CreateGrid();
            LevelHandler.instance.SetCoinPerWord();
            Debug.Log("Level Init Completed !!");
        }

        void SetCameraPos()
        {
            GridCamScriptable gridCamScriptable = GameController.instance.GetGridCamScriptable();

            bool isNotMatching = true;

            foreach (GridCamScriptable.CamGridSizeStruct camInfo in gridCamScriptable.camGridInfoList)
            {
                if (Screen.width == camInfo.screenSize.x && Screen.height == camInfo.screenSize.y)
                {
                    foreach (GridCamScriptable.GridDataInfo gridInfo in camInfo.gridDataInfos)
                    {
                        if (gridInfo.gridSize.x == DataHandler.CurrGridSize)
                        {
                            _cam.orthographicSize = gridInfo.camOrthographicSize;
                            _currGridSize = gridInfo.gridScale;
                            _cam.transform.position = gridInfo.camPos;
                            isNotMatching = false;
                            break;
                        }
                    }
                }
            }

            if (isNotMatching)
            {
                //No Screen Size Matched !!!
                Debug.Log("No Screen Size Matched !");
                foreach (GridCamScriptable.CamGridSizeStruct camInfo in gridCamScriptable.camGridInfoList)
                {
                    if (camInfo.screenSize.x == 0 && camInfo.screenSize.y == 0)
                    {
                        foreach (GridCamScriptable.GridDataInfo gridInfo in camInfo.gridDataInfos)
                        {
                            if (gridInfo.gridSize.x == DataHandler.CurrGridSize)
                            {
                                _cam.orthographicSize = gridInfo.camOrthographicSize;
                                _currGridSize = gridInfo.gridScale;
                                _cam.transform.position = gridInfo.camPos;
                                break;
                            }
                        }
                    }
                }
            }

            SetQuestionGrid();
        }


        void SetQuestionGrid()
        {
            GridCamScriptable gridCamScriptable = GameController.instance.GetGridCamScriptable();
            GameObject quesPrefab = DataHandler.instance.quesPrefab;
            Vector3 startPos = quesGridTrans.transform.position;
            int totalChild = quesGridTrans.childCount;
            bool isQuesNotMatching = true;
            int numOfQues = DataHandler.CurrTotalQuesSize;

            foreach (GridCamScriptable.CamGridSizeStruct camInfo in gridCamScriptable.camGridInfoList)
            {
                if (Screen.width == camInfo.screenSize.x && Screen.height == camInfo.screenSize.y)
                {
                    foreach (GridCamScriptable.GridDataInfo gridInfo in camInfo.gridDataInfos)
                    {
                        List<GridCamScriptable.QuesDataInfo> quesInfoList = gridInfo.queBlockInfoList;

                        foreach (GridCamScriptable.QuesDataInfo quesInfo in quesInfoList)
                        {
                            if (quesInfo.numOfQues == numOfQues)
                            {
                                quesSpacing = quesInfo.quesSpacing;
                                quesGridTrans.transform.position = quesInfo.queContainerPos;
                                _currQuesSize = quesInfo.quesBlockScale;
                                _camOrthographicSize = gridInfo.camOrthographicSize;
                                isQuesNotMatching = false;
                                break;
                            }
                        }
                    }
                }
            }

            if (isQuesNotMatching)
            {
                foreach (GridCamScriptable.CamGridSizeStruct camInfo in gridCamScriptable.camGridInfoList)
                {
                    if (camInfo.screenSize.x == 0 && camInfo.screenSize.y == 0)
                    {
                        foreach (GridCamScriptable.GridDataInfo gridInfo in camInfo.gridDataInfos)
                        {
                            List<GridCamScriptable.QuesDataInfo> quesInfoList = gridInfo.queBlockInfoList;

                            foreach (GridCamScriptable.QuesDataInfo quesInfo in quesInfoList)
                            {
                                if (quesInfo.numOfQues == numOfQues)
                                {
                                    quesSpacing = quesInfo.quesSpacing;
                                    quesGridTrans.transform.position = quesInfo.queContainerPos;
                                    _currQuesSize = quesInfo.quesBlockScale;
                                    _camOrthographicSize = gridInfo.camOrthographicSize;
                                    break;
                                }
                            }
                        }
                    }
                }
            }


            for (int i = 0; i < numOfQues; i++)
            {
                GameObject gmObj = totalChild > 0
                    ? quesGridTrans.GetChild(i).gameObject
                    : Instantiate(quesPrefab, quesGridTrans);

                QuesTile quesTileScript = gmObj.GetComponent<QuesTile>();
                gmObj.transform.localScale = new Vector3(_currQuesSize, _currQuesSize, _currQuesSize / 2);
                gmObj.transform.localPosition = new Vector3(startPos.x, 0, 0);
                gmObj.name = $"Ques_{i}";
                LevelHandler.instance.UpdateQuesList(quesTileScript);
                startPos.x += quesSpacing + _currQuesSize;
                totalChild--;
            }
        }

        public Transform GetGridContainerTrans()
        {
            return gridContainer;
        }

        public Vector2 BottomOfScreenPoint()
        {
            var position = _cam.transform.position;
            return new Vector2(position.x, position.y - _camOrthographicSize);
        }

        public Vector2 GetRandomPointOutOfScreen()
        {
            float orthographicSize = _camOrthographicSize + camGridOffset;
            float randomX = 0f, randomY = 0f;
            int side = Random.Range(0, _randomScreenPointArr.Length);

            //Side  = -1 means Horizontal and Side = 1 means Vertical
            if (_randomScreenPointArr[side] == -1)
            {
                int xPoint = Random.Range(0, _randomScreenPointArr.Length);

                if (_randomScreenPointArr[xPoint] == -1)
                {
                    //When the side is selected as Left Side.
                    randomX = Random.Range(-orthographicSize - distanceFromScreen,
                        -orthographicSize / 2 - distanceFromScreen);
                    randomY = Random.Range(-orthographicSize, orthographicSize);
                }
                else if (_randomScreenPointArr[xPoint] == 1)
                {
                    //When the side is selected as Right Side.
                    randomX = Random.Range(orthographicSize / 2 + distanceFromScreen,
                        orthographicSize + distanceFromScreen);
                    randomY = Random.Range(-orthographicSize, orthographicSize);
                }
            }
            else
            {
                int yPoint = Random.Range(0, _randomScreenPointArr.Length);

                if (_randomScreenPointArr[yPoint] == -1)
                {
                    //When the side is selected as Bottom Side.
                    randomX = Random.Range(-orthographicSize / 2, orthographicSize / 2 + distanceFromScreen);
                    randomY = Random.Range(-orthographicSize - distanceFromScreen,
                        -(orthographicSize * 2) - distanceFromScreen);
                }
                else if (_randomScreenPointArr[yPoint] == 1)
                {
                    //When the side is selected as Top Side.
                    randomX = Random.Range(-orthographicSize / 2, orthographicSize / 2 + distanceFromScreen);
                    randomY = Random.Range(orthographicSize + distanceFromScreen,
                        orthographicSize * 2 + distanceFromScreen);
                }
            }

            return new Vector2(randomX, randomY);
        }

        private void CreateGrid()
        {
            GameObject gridPrefab = DataHandler.instance.gridPrefab;
            _defaultStartPos.y = defaultStartYPos;
            _defaultStartPos.x = -_camOrthographicSize / 2 + _currGridSize / 2;
            Debug.Log("Default Pos : " + _defaultStartPos.x);
            Vector3 startPos = new Vector3(_defaultStartPos.x, _defaultStartPos.y, _defaultStartPos.z);

            if (GameController.instance.maxGridSize < DataHandler.CurrGridSize)
            {
                DataHandler.IsMaxGridOpened = 1;
            }

            List<bool> gridScreenList = new List<bool>(SaveManager.Instance.state.gridOnScreenList);
            int index = 0;
            //Debug.Log("Start Pos : " + startPos);

            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    GameObject gmObj = Instantiate(gridPrefab, gridContainer);
                    GridTile gridTileScript = gmObj.GetComponent<GridTile>();
                    //Assigning New Material to each grid.
                    Renderer gmRenderer = gridTileScript.cube.GetComponent<Renderer>();
                    gmObj.transform.localScale = new Vector3(_currGridSize, _currGridSize, _currGridSize / 2);
                    gridTileScript.DefaultGridData(startPos);
                    gmObj.transform.position = BottomOfScreenPoint();
                    gmObj.SetActive(false);
                    gmObj.name = $"Grid_{i}_{j}";
                    gridTileScript.AssignInfo(this);
                    gridTileScript.GridID = new Vector2Int(i, j);
                    LevelHandler.instance.totalGridsList.Add(gridTileScript);

                    gridTileScript.isBlastAfterWordComplete = gridScreenList.Count > index && gridScreenList[index];

                    startPos.x += _currGridSize;

                    if ((i == gridSize.x - 1 || j == gridSize.y - 1) && DataHandler.IsMaxGridOpened == 0)
                    {
                        LevelHandler.instance.totalBuyingGridList.Add(gridTileScript);
                        gridTileScript.GetText().gameObject.SetActive(false);
                        gridTileScript.isGridActive = false;
                        gmRenderer.material = new Material(gridTileScript.lockMaterial);
                        LevelHandler.instance.lockedGridList.Add(gridTileScript);
                        gridTileScript.SetCurrentLockStatus(false);
                        gridTileScript.SetLockTextAmount(LevelHandler.instance.coinToUnlockNextGrid);
                        gridTileScript.isFullLocked = true;
                    }
                    else
                    {
                        LevelHandler.instance.unlockedGridList.Add(gridTileScript);
                        LevelHandler.instance.gridAvailableOnScreenList.Add(gridTileScript);
                        gmRenderer.material = new Material(gridMaterial);
                        gridTileScript.gridMaterial = gmRenderer.material;
                    }

                    //Debug.Log("GRID CREATED INDEX : " + index);
                    index++;
                    //Debug.Log($"Curr Grid Data Size New : {i} {j}");
                    AssignGridData(gridTileScript, i, j);
                }

                startPos = new Vector3(_defaultStartPos.x,
                    _defaultStartPos.y - ((i + 1) * _currGridSize) - (gridSpacing * (i + 1)),
                    _defaultStartPos.z);
            }

            if (DataHandler.CurrGridSize == GameController.instance.maxGridSize)
            {
                DataHandler.IsMaxGridOpened = 1;
            }

            UnlockPreviousGrids();
            LoadHintData();
        }

        private void LoadHintData()
        {
            if (SaveManager.Instance.state.hintList.Count <= 0)
                return;

            LevelHandler.instance.hintAvailList.Clear();

            foreach (string str in SaveManager.Instance.state.hintList)
            {
                LevelHandler.instance.hintAvailList.Add(str);
            }
        }

        void UnlockPreviousGrids()
        {
            List<GridTile> list = new List<GridTile>(LevelHandler.instance.lockedGridList);
            for (int i = 0; i < list.Count; i++)
            {
                GridTile tile = list[i];
                if (i < DataHandler.UnlockGridIndex)
                {
                    LevelHandler.instance.gridAvailableOnScreenList.Add(tile);
                    LevelHandler.instance.totalBuyingGridList.Remove(tile);
                    LevelHandler.instance.unlockedGridList.Add(tile);
                    tile.DeactivateLockStatus();
                    continue;
                }

                Debug.Log("Tile Unlocked : " + tile + " at : " + i);
                LevelHandler.instance.UnlockNextGridForCoins();
                break;
            }
        }

        public void GridPlacement()
        {
            StartCoroutine(PlaceGrids());
        }

        private IEnumerator PlaceGrids()
        {
            SoundManager.instance.PlaySound(SoundManager.SoundType.CardDeckSound);
            
            if (LevelHandler.instance.totalGridsList.Count > 0)
            {
                foreach (GridTile gmObj in LevelHandler.instance.totalGridsList)
                {
                    yield return new WaitForSeconds(timeToWaitForEachGrid / 2);

                    gmObj.gameObject.SetActive(true);

                    if (!gmObj.isBlastAfterWordComplete)
                    {
                        gmObj.transform.DOLocalMove(gmObj.defaultGridPos, timeToPlaceGrid).SetEase(gridPlacementEase);
                    }
                    else
                    {
                        LevelHandler.instance.wordCompletedGridList.Add(gmObj);
                        LevelHandler.instance.gridAvailableOnScreenList.Remove(gmObj);
                    }
                }
            }
            
            UIManager.instance.CheckAllButtonStatus();
            LevelHandler.instance.SetLevelRunningBool(true);
        }

        private void AssignGridData(GridTile gridTileScript, int row, int column)
        {
            string str = LevelHandler.instance.gridData[row][column].ToString().ToUpper();
            gridTileScript.GridTextData = str;

            if (string.IsNullOrWhiteSpace(str))
            {
                gridTileScript.isGridActive = false;
            }
        }
    }
}