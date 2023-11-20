using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
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
        [SerializeField] private float gridSpacing = 0.1f, gridXSpacing = -0.2f;

        private void Awake()
        {
            _cam = Camera.main;
            _camOrthographicSize = _cam.orthographicSize;
        }

        public void StartInit()
        {
            //Debug.Log("Level StartInit Called !");
            //Debug.Log($"Aspect Ratio : {_cam.aspect} , Width : {Screen.width} , Height  : {Screen.height}");
            //SetCameraPos();
            SetGridContainerPos();
            CreateGrid();
            LevelHandler.Instance.SetCoinPerWord();
            //Debug.Log("Level Init Completed !!");
        }

        private void SetGridContainerPos()
        {
            switch (DataHandler.CurrGridSize)
            {
                case 4:
                    _currGridSize = 1.2f;
                    gridContainer.transform.position = LevelHandler.Instance.boxContainer.transform.position -
                                                       new Vector3(2.25f, 0.1f, 0f);
                    break;

                case 5:
                    _currGridSize = 1f;
                    gridContainer.transform.position = LevelHandler.Instance.boxContainer.transform.position -
                                                       new Vector3(2.15f, 0.1f, 0f);
                    break;

                case 6:
                    _currGridSize = 0.85f;
                    gridContainer.transform.position = LevelHandler.Instance.boxContainer.transform.position -
                                                       new Vector3(2.3f, -0.15f, 0f);
                    break;

                case 7:
                    _currGridSize = 0.72f;
                    gridContainer.transform.position = LevelHandler.Instance.boxContainer.transform.position -
                                                       new Vector3(2.23f, -0.15f, 0f);
                    break;
            }

            int totalChild = quesGridTrans.childCount;
            Vector3 startPos = quesGridTrans.transform.position;
            float spacing = 0f;


            switch (DataHandler.CurrTotalQuesSize)
            {
                case 3:
                    _currQuesSize = 1.1f;
                    quesGridTrans.position = LevelHandler.Instance.boxContainer.transform.position -
                                             new Vector3(1.2f, -3.3f, 0f);
                    spacing = 0.15f;
                    break;

                case 4:
                    _currQuesSize = 0.9f;
                    quesGridTrans.position = LevelHandler.Instance.boxContainer.transform.position -
                                             new Vector3(1.55f, -3.5f, 0f);
                    spacing = 0.15f;
                    break;

                case 5:
                    _currQuesSize = 0.7f;
                    quesGridTrans.position = LevelHandler.Instance.boxContainer.transform.position -
                                             new Vector3(1.7f, -3.3f, 0f);
                    spacing = 0.15f;
                    break;

                case 6:

                    _currQuesSize = 0.6f;
                    quesGridTrans.position = LevelHandler.Instance.boxContainer.transform.position -
                                             new Vector3(1.9f, -3.3f, 0f);
                    spacing = 0.15f;

                    break;
            }

            for (int i = 0; i < DataHandler.CurrTotalQuesSize; i++)
            {
                GameObject gmObj = totalChild > 0
                    ? quesGridTrans.GetChild(i).gameObject
                    : Instantiate(DataHandler.instance.quesPrefab, quesGridTrans);

                QuesTile quesTileScript = gmObj.GetComponent<QuesTile>();
                gmObj.transform.localScale = new Vector3(_currQuesSize, _currQuesSize, _currQuesSize / 2);
                gmObj.transform.localPosition = new Vector3(startPos.x, 0, 0);
                gmObj.name = $"Ques_{i}";
                LevelHandler.Instance.UpdateQuesList(quesTileScript, i);
                startPos.x += (spacing) + _currQuesSize;
                totalChild--;
            }
        }

        private void CreateGrid()
        {
            GameObject gridPrefab = DataHandler.instance.gridPrefab;
            Transform boxTrans = gridContainer.transform;
            _defaultStartPos.y = boxTrans.localScale.y * 2;
            _defaultStartPos.x = _currGridSize / 2;
            //Debug.Log("Default Pos : " + _defaultStartPos.x);
            Vector3 startPos = new Vector3(_defaultStartPos.x, _defaultStartPos.y, _defaultStartPos.z);

            if (GameController.instance.maxGridSize < DataHandler.CurrGridSize &&
            DataHandler.UnlockGridIndex >= LevelHandler.Instance.lockedGridList.Count - 1)
            {
                Debug.Log("Before Max Grid Unlocked !!");
                DataHandler.IsMaxGridOpened = 1;
            }

            List<bool> gridScreenList = new List<bool>(SaveManager.Instance.state.gridOnScreenList);
            int index = 0;

            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    GameObject gmObj = Instantiate(gridPrefab, gridContainer);
                    GridTile gridTileScript = gmObj.GetComponent<GridTile>();
                    //Assigning New Material to each grid.
                    Renderer gmRenderer = gridTileScript.cube.GetComponent<Renderer>();
                    Vector3 globalPos = gmObj.transform.TransformPoint(startPos);
                    gmObj.transform.localScale = new Vector3(_currGridSize, _currGridSize, _currGridSize / 2);
                    gridTileScript.DefaultGridData(startPos, gmObj.transform.rotation, globalPos);
                    //Debug.Log("Start Pos : " + startPos);
                    gmObj.transform.position = BottomOfScreenPoint();
                    gmObj.SetActive(false);
                    gmObj.name = $"Grid_{i}_{j}";
                    gridTileScript.AssignInfo(this);
                    gridTileScript.GridID = new Vector2Int(i, j);
                    LevelHandler.Instance.totalGridsList.Add(gridTileScript);

                    if (DataHandler.NewGridCreated == 0)
                    {
                        //Debug.Log("New Grid Created And Blast Data Rest Done !");
                        gridTileScript.isBlastAfterWordComplete = false;

                        if (index == (gridSize.x * gridSize.y) - 1)
                        {
                            //Debug.Log("New Data Created PlayerPref Set to 1 !");
                            DataHandler.NewGridCreated = 1;
                        }
                    }
                    else
                    {
                        gridTileScript.isBlastAfterWordComplete = gridScreenList.Count > index && gridScreenList[index];
                    }

                    startPos.x += _currGridSize + gridXSpacing;

                    if ((i == gridSize.x - 1 || j == gridSize.y - 1) && DataHandler.IsMaxGridOpened == 0)
                    {
                        LevelHandler.Instance.totalBuyingGridList.Add(gridTileScript);
                        gridTileScript.GetText().gameObject.SetActive(false);
                        gridTileScript.isGridActive = false;
                        LevelHandler.Instance.lockedGridList.Add(gridTileScript);
                        gridTileScript.SetCurrentLockStatus(false);
                        gridTileScript.SetLockTextAmount(LevelHandler.Instance.coinToUnlockNextGrid);
                        gridTileScript.isFullLocked = true;
                    }
                    else
                    {
                        LevelHandler.Instance.unlockedGridList.Add(gridTileScript);
                        LevelHandler.Instance.gridAvailableOnScreenList.Add(gridTileScript);
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

            if (DataHandler.CurrGridSize == GameController.instance.maxGridSize &&
                DataHandler.UnlockGridIndex >= LevelHandler.Instance.lockedGridList.Count - 1)
            {
                Debug.Log("After Max Grid Unlocked !!");
                DataHandler.IsMaxGridOpened = 1;
            }

            UnlockPreviousGrids();
            LoadHintData();
        }

        public Transform GetGridContainerTrans()
        {
            return gridContainer;
        }


        public Vector2 BottomOfScreenPoint()
        {
            var position = _cam.transform.position;
            return new Vector2(position.x, position.y - _camOrthographicSize * 2);
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
                        -orthographicSize - distanceFromScreen);
                    randomY = Random.Range(-orthographicSize, orthographicSize);
                }
                else if (_randomScreenPointArr[xPoint] == 1)
                {
                    //When the side is selected as Right Side.
                    randomX = Random.Range(orthographicSize + distanceFromScreen,
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
                    randomX = Random.Range(-orthographicSize * 2, orthographicSize * 2 + distanceFromScreen);
                    randomY = Random.Range(-orthographicSize * 2 - distanceFromScreen,
                        -(orthographicSize * 3) - distanceFromScreen);
                }
                else if (_randomScreenPointArr[yPoint] == 1)
                {
                    //When the side is selected as Top Side.
                    randomX = Random.Range(-orthographicSize * 2, orthographicSize * 2 + distanceFromScreen);
                    randomY = Random.Range(orthographicSize * 2 + distanceFromScreen,
                        orthographicSize * 3 + distanceFromScreen);
                }
            }

            return new Vector2(randomX, randomY);
        }

        private void LoadHintData()
        {
            if (SaveManager.Instance.state.hintList.Count <= 0)
                return;

            LevelHandler.Instance.hintAvailList.Clear();

            foreach (string str in SaveManager.Instance.state.hintList)
            {
                LevelHandler.Instance.hintAvailList.Add(str);
            }
        }

        void UnlockPreviousGrids()
        {
            List<GridTile> list = new List<GridTile>(LevelHandler.Instance.lockedGridList);
            for (int i = 0; i < list.Count; i++)
            {
                GridTile tile = list[i];
                if (i < DataHandler.UnlockGridIndex)
                {
                    LevelHandler.Instance.gridAvailableOnScreenList.Add(tile);
                    LevelHandler.Instance.totalBuyingGridList.Remove(tile);
                    LevelHandler.Instance.unlockedGridList.Add(tile);
                    tile.DeactivateLockStatus();
                    continue;
                }

                //Debug.Log("Tile Unlocked : " + tile + " at : " + i);
                LevelHandler.Instance.UnlockNextGridForCoins();
                break;
            }
        }

        public void GridPlacement()
        {
            StartCoroutine(PlaceGrids());
        }

        private IEnumerator PlaceGrids()
        {
            SoundManager.instance.PlaySound(SoundManager.SoundType.CardDeck);
            UIManager.Instance.catIdleAnimator.Play("Empty");
            if (LevelHandler.Instance.totalGridsList.Count > 0)
            {
                foreach (GridTile gmObj in LevelHandler.Instance.totalGridsList)
                {
                    yield return new WaitForSeconds(timeToWaitForEachGrid / 2);

                    gmObj.gameObject.SetActive(true);

                    if (!gmObj.isBlastAfterWordComplete)
                    {
                        gmObj.transform.DOLocalMove(gmObj.defaultLocalGridPos, timeToPlaceGrid)
                            .SetEase(gridPlacementEase);
                    }
                    else
                    {
                        LevelHandler.Instance.wordCompletedGridList.Add(gmObj);
                        LevelHandler.Instance.gridAvailableOnScreenList.Remove(gmObj);
                    }
                }
            }

            UIManager.Instance.CheckAllButtonStatus();
            LevelHandler.Instance.SetLevelRunningBool(true);

            if (DataHandler.HelperLevelCompleted == 0)
            {
                LevelHandler.Instance.SetLevelRunningBool(false);
                GameController.instance.helper.PlayHelper();
            }
        }

        private void AssignGridData(GridTile gridTileScript, int row, int column)
        {
            string str = LevelHandler.Instance.gridData[row][column].ToString().ToUpper();
            gridTileScript.GridTextData = str;

            if (string.IsNullOrWhiteSpace(str))
            {
                gridTileScript.isGridActive = false;
            }
        }
    }
}