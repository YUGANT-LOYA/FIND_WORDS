using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace YugantLoyaLibrary.FindWords
{
    public class Level : MonoBehaviour
    {
        [Header("Main Info")]
        //public Vector2 lineOffset;
        private LevelHandler _levelHandler;

        private Camera _cam;
        [HideInInspector] public Vector2Int gridSize;
        private float _camOrthographicSize;
        public int totalWordsToFind = 4;
        [SerializeField] private int quesInOneRow = 5;
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

        public Transform quesGridTrans, rotationContainer;
        [SerializeField] private Transform gridContainer;
        private float _currGridWidth, _currGridHeight, _currGridSize, _currQuesSize;
        [SerializeField] private float gridSpacing = 0.1f, quesSpacing = 0.2f;

        [Header("Input Data Info")] TextMeshProUGUI _levelNumText;

        public string LevelNumData
        {
            get => _levelNumText.text;
            set => _levelNumText.text = value;
        }

        private void Awake()
        {
            _cam = Camera.main;
        }

        public void StartInit()
        {
            _camOrthographicSize = _cam.orthographicSize;
            //Debug.Log("Level StartInit Called !");
            Debug.Log($"Aspect Ratio : {_cam.aspect} , Width : {Screen.width} , Height  : {Screen.height}");
            Debug.Log($"Factor  : {_cam.aspect * _camOrthographicSize}");

            SetCameraPos();
            CreateGrid();
            _levelNumText = UIManager.instance.levelText;
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

        public void FillData(LevelHandler handler)
        {
            AssignLevelHandler(handler);
        }

        void SetQuestionGrid()
        {
            GridCamScriptable gridCamScriptable = GameController.instance.GetGridCamScriptable();
            GameObject quesPrefab = DataHandler.instance.quesPrefab;
            Vector3 startPos = quesGridTrans.transform.position;
            GridCamScriptable.QuesDataInfo mainQuesInfo;
            int totalChild = quesGridTrans.childCount;
            bool isNewQuesBlockBuyThere = false;
            bool isQuesNotMatching = true;
            int numOfQues = DataHandler.CurrTotalQuesSize;

            if (DataHandler.CurrGridSize > GameController.instance.startingGridSize &&
                DataHandler.IsMaxGridOpened == 0)
            {
                isNewQuesBlockBuyThere = true;
            }

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
                                mainQuesInfo = quesInfo;
                                quesSpacing = quesInfo.quesSpacing;
                                quesGridTrans.transform.position = quesInfo.queContainerPos;
                                _currQuesSize = quesInfo.quesBlockScale;
                                break;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < numOfQues; i++)
            {
                GameObject gmObj;

                if (totalChild > 0)
                {
                    gmObj = quesGridTrans.GetChild(i).gameObject;
                }
                else
                {
                    gmObj = Instantiate(quesPrefab, quesGridTrans);
                }

                QuesTile quesTileScript = gmObj.GetComponent<QuesTile>();
                quesTileScript.SetLevelHandler(_levelHandler);
                gmObj.transform.localScale = new Vector3(_currQuesSize, _currQuesSize, _currQuesSize / 2);
                gmObj.transform.localPosition = new Vector3(startPos.x, 0, 0);
                gmObj.name = $"Ques_{i}";
                _levelHandler.UpdateQuesList(quesTileScript);

                if (i == numOfQues - 1 && isNewQuesBlockBuyThere)
                {
                    quesTileScript.SetUnlockCoinAmount(100);
                    quesTileScript.isUnlocked = false;
                    quesTileScript.IsLocked(true);
                }

                startPos.x += quesSpacing + _currQuesSize;
                totalChild--;
            }
        }

        private void AssignLevelHandler(LevelHandler handler)
        {
            _levelHandler = handler;
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
                    _levelHandler.totalGridsList.Add(gridTileScript);
                    startPos.x += /*gridSpacing +*/ _currGridSize;

                    if ((i == gridSize.x - 1 || j == gridSize.y - 1) && DataHandler.IsMaxGridOpened == 0)
                    {
                        _levelHandler.buyGridList.Add(gridTileScript);
                        gridTileScript.SetLockStatus(true);
                        gridTileScript.SetLockTextAmount(100);
                        gridTileScript.isLocked = true;
                        gridTileScript.GetText().gameObject.SetActive(false);
                        gridTileScript.isGridActive = false;
                        gmRenderer.material = new Material(gridTileScript.lockMaterial);
                    }
                    else
                    {
                        _levelHandler.unlockedGridList.Add(gridTileScript);
                        _levelHandler.gridAvailableOnScreenList.Add(gridTileScript);
                        gmRenderer.material = new Material(gridMaterial);
                        gridTileScript.gridMaterial = gmRenderer.material;
                    }

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

            StartCoroutine(PlaceGrids());
        }

        private IEnumerator PlaceGrids()
        {
            if (_levelHandler.totalGridsList.Count > 0)
            {
                foreach (GridTile gmObj in _levelHandler.totalGridsList)
                {
                    yield return new WaitForSeconds(timeToWaitForEachGrid / 2);
                    gmObj.gameObject.SetActive(true);
                    gmObj.transform.DOLocalMove(gmObj.defaultGridPos, timeToPlaceGrid).SetEase(gridPlacementEase);
                }
            }
        }

        private void AssignGridData(GridTile gridTileScript, int row, int column)
        {
            string str = _levelHandler.gridData[row][column].ToString().ToUpper();
            gridTileScript.GridTextData = str;

            if (string.IsNullOrWhiteSpace(str))
            {
                gridTileScript.isGridActive = false;
            }
        }
    }
}