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
        private readonly int[] _randomScreenPointArr = { -1, 1 };
        public Ease gridPlacementEase;
        Vector3 _gridContainerSize, _defaultStartPos;

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
            Debug.Log($"Aspect Ratio : {_cam.aspect} , Width : {Screen.width} , Height  : {Screen.height}");
            SetGridContainerPos(DataHandler.CurrGridSize);
            SetQuesGrid(DataHandler.CurrTotalQuesSize);
            CreateGrid();
            Debug.Log("Level Init Completed !!");
        }

        private void SetGridContainerPos(int gridCount)
        {
            switch (gridCount)
            {
                case 4:
                    _currGridSize = 1.2f;
                    gridContainer.transform.position = LevelHandler.Instance.boxContainer.transform.position -
                                                       new Vector3(2.25f, 0.1f, 0f);
                    break;

                case 5:
                    _currGridSize = 1f;
                    gridContainer.transform.position = LevelHandler.Instance.boxContainer.transform.position -
                                                       new Vector3(2.3f, 0.1f, 0f);
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
        }


        public void SetQuesGrid(int quesGridCount, bool isGridUnlockFromOutside = false)
        {
            if (DataHandler.CurrTotalQuesSize >= GameController.Instance.maxGridSize)
            {
                DataHandler.CurrTotalQuesSize = GameController.Instance.maxQuesSize;
                quesGridCount = DataHandler.CurrTotalQuesSize;
            }

            int totalChild = quesGridTrans.childCount;
            quesGridTrans.localPosition = Vector2.zero;
            Vector3 startPos = quesGridTrans.transform.localPosition;
            float spacing = 0.05f;
            //Debug.Log("Set Ques Grid Count : " + quesGridCount);

            switch (quesGridCount)
            {
                case 3:
                    _currQuesSize = 1.1f;
                    break;

                case 4:
                    _currQuesSize = 1f;
                    break;

                case 5:
                    _currQuesSize = 0.85f;
                    break;

                case 6:
                    _currQuesSize = 0.71f;
                    break;
            }


            LevelHandler.Instance.quesTileList.Clear();

            int index = 0;
            for (int i = 0; i < quesGridCount; i++)
            {
                GameObject gmObj = totalChild > 0
                    ? quesGridTrans.GetChild(i).gameObject
                    : Instantiate(DataHandler.Instance.quesPrefab, quesGridTrans);

                QuesTile quesTileScript = gmObj.GetComponent<QuesTile>();
                gmObj.transform.localScale = new Vector3(_currQuesSize, _currQuesSize, _currQuesSize / 2);
                quesTileScript.defaultTileScale = gmObj.transform.localScale;
                gmObj.transform.localPosition = new Vector3(startPos.x, 0, 0);
                gmObj.name = $"Ques_{i}";

                if (index >= DataHandler.UnlockedQuesLetter)
                {
                    quesTileScript.LockQuesTile(
                        DataHandler.Instance.quesGridUnlockPrice[DataHandler.NewQuesGridUnlockIndex]);
                }

                LevelHandler.Instance.UpdateQuesList(quesTileScript, i);
                startPos.x += (spacing) + _currQuesSize;
                totalChild--;
                index++;
            }

            switch (quesGridCount)
            {
                case 3:

                    quesGridTrans.localPosition = new Vector3(-1.2f, 3.3f, -0.5f);

                    break;

                case 4:

                    quesGridTrans.localPosition = new Vector3(-1.6f, 3.3f, -0.5f);

                    break;

                case 5:

                    quesGridTrans.localPosition = new Vector3(-1.8f, 3.3f, -0.5f);

                    break;

                case 6:

                    quesGridTrans.localPosition = new Vector3(-1.9f, 3.3f, -0.5f);

                    break;
            }

            LevelHandler.Instance.SetCoinPerWord();
        }

        private void CreateGrid()
        {
            GameObject gridPrefab = DataHandler.Instance.gridPrefab;
            Transform boxTrans = gridContainer.transform;
            _defaultStartPos.y = boxTrans.localScale.y * 2;
            _defaultStartPos.x = _currGridSize / 2;
            //Debug.Log("Default Pos : " + _defaultStartPos.x);
            Vector3 startPos = new Vector3(_defaultStartPos.x, _defaultStartPos.y, _defaultStartPos.z);

            if (GameController.Instance.maxGridSize < DataHandler.CurrGridSize &&
                DataHandler.UnlockGridIndex >= LevelHandler.Instance.lockedGridList.Count - 1)
            {
                //Debug.Log("Before Max Grid Unlocked !!");
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
                    Vector3 globalPos = gmObj.transform.TransformPoint(startPos);
                    gmObj.transform.localScale = new Vector3(_currGridSize, _currGridSize, _currGridSize / 2);
                    gridTileScript.DefaultGridData(startPos, gmObj.transform.rotation, globalPos);
                    //Debug.Log("Start Pos : " + startPos);
                    gmObj.transform.position = BottomOfScreenPoint();
                    gmObj.SetActive(false);
                    gmObj.name = $"Grid_{i}_{j}";
                    gridTileScript.AssignInfo(this);
                    gridTileScript.GridID = new Vector2Int(i, j);
                    //LevelHandler.Instance.totalGridsList.Add(gridTileScript);
                    LevelHandler.AddGridToList(LevelHandler.Instance.totalGridsList,
                        gridTileScript);

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
                        LevelHandler.AddGridToList(LevelHandler.Instance.totalBuyingGridList, gridTileScript);
                        gridTileScript.GetText().gameObject.SetActive(false);
                        gridTileScript.isGridActive = false;
                        gridTileScript.isFullLocked = true;
                        LevelHandler.AddGridToList(LevelHandler.Instance.lockedGridList, gridTileScript);
                        gridTileScript.SetLockStatus();
                        gridTileScript.SetLockTextAmount(LevelHandler.Instance.coinToUnlockNextGrid);
                    }
                    else
                    {
                        LevelHandler.AddGridToList(LevelHandler.Instance.unlockedGridList, gridTileScript);
                        LevelHandler.AddGridToList(LevelHandler.Instance.gridAvailableOnScreenList, gridTileScript);
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

            if (DataHandler.CurrGridSize == GameController.Instance.maxGridSize &&
                DataHandler.UnlockGridIndex >= LevelHandler.Instance.lockedGridList.Count - 1)
            {
                //Debug.Log("After Max Grid Unlocked !!");
                DataHandler.IsMaxGridOpened = 1;
            }

            UnlockPreviousGrids(true);
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

        void UnlockPreviousGrids(bool isCalledAtStart)
        {
            List<GridTile> list = new List<GridTile>(LevelHandler.Instance.lockedGridList);
            //Debug.Log("Unlock Grid Index : " + DataHandler.UnlockGridIndex);
            for (int i = 0; i < list.Count; i++)
            {
                GridTile tile = list[i];

                if (i < DataHandler.UnlockGridIndex)
                {
                    LevelHandler.Instance.UnlockingGrid(tile);
                    //Debug.Log("Unlocked Tile Name From Locked List : " + tile.name);
                    tile.DeactivateLockStatus();
                    continue;
                }

                //Debug.Log("Tile Unlocked : " + tile + " at : " + i);
                LevelHandler.Instance.UnlockNextGridForCoins(isCalledAtStart);

                break;
            }
        }

        public void GridPlacement()
        {
            LevelHandler.Instance.SetLevelRunningBool(false);
            Debug.Log("Grid Placement Entered !");
            StartCoroutine(PlaceGrids());
        }

        private IEnumerator PlaceGrids()
        {
            SoundManager.instance.PlaySound(SoundManager.SoundType.CardDeck);
            UIManager.Instance.catIdleAnimator.Play("Empty");
            if (LevelHandler.Instance.totalGridsList.Count > 0)
            {
                foreach (GridTile gridTile in LevelHandler.Instance.totalGridsList)
                {
                    yield return new WaitForSeconds(timeToWaitForEachGrid / 2);

                    gridTile.gameObject.SetActive(true);

                    if (!gridTile.isBlastAfterWordComplete)
                    {
                        gridTile.transform.DOLocalMove(gridTile.defaultLocalGridPos, timeToPlaceGrid)
                            .SetEase(gridPlacementEase);
                    }
                    else
                    {
                        LevelHandler.AddGridToList(LevelHandler.Instance.wordCompletedGridList, gridTile);
                        LevelHandler.RemoveGridFromList(LevelHandler.Instance.gridAvailableOnScreenList, gridTile);
                    }
                }
            }
            
            if (DataHandler.HelperLevelCompleted == 0)
            {
                if (DataHandler.HelperIndex < GameController.Instance.helper.clickDealIndex)
                {
                    DataHandler.HelperIndex = 0;
                }
                else if (DataHandler.HelperIndex > GameController.Instance.helper.clickDealIndex)
                {
                    DataHandler.HelperIndex = GameController.Instance.helper.clickDealIndex + 1;
                }

                LevelHandler.Instance.SetLevelRunningBool(false);
                GameController.Instance.helper.PlayHelper();
            }
            else
            {
                yield return new WaitForSeconds(timeToWaitForEachGrid / 2);
                
                UIManager.Instance.CheckAllButtonStatus();
                LevelHandler.Instance.SetLevelRunningBool();
            }
        }

        private void AssignGridData(GridTile gridTileScript, int row, int column)
        {
            string str = LevelHandler.Instance.GridStringData[row][column].ToString().ToUpper();
            gridTileScript.GridTextData = str;

            if (string.IsNullOrWhiteSpace(str))
            {
                gridTileScript.isGridActive = false;
            }
        }
    }
}