using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace YugantLoyaLibrary.FindWords
{
    public class Level : MonoBehaviour
    {
        [Header("Main Info")]
        //public Vector2 lineOffset;
        private LevelHandler _levelHandler;

        private Camera _cam;
        public Vector2Int gridSize;
        private float _camOrthographicSize;
        public int totalWordToFind = 4, quesInOneRow = 5;
        public Material gridMaterial;
        private readonly int[] _randomScreenPointArr = { -1, 1 };
        public Ease gridPlacementEase;
        Vector3 _gridContainerSize, _defaultStartPos;
        public float defaultStartYPos = 1.5f;

        [Tooltip("The Grid Should come inside camera's orthographic size, so there should be offset from one side")]
        public float camGridOffset = 0.5f, screenOffset = 50f;

        [Tooltip("The Distance outside the screen from where the grid should come and place in the grid")]
        public float distanceFromScreen = 5f;

        [Tooltip("Time to place Grid in its original Position")]
        public float timeToPlaceGrid = 0.4f;

        [SerializeField] Button restartButton, hintButton;
        public Transform quesGridTrans, rotationContainer, midPanelContainerTrans;
        [SerializeField] private Transform gridContainer;
        private float _currGridWidth, _currGridHeight, _lineRendererWidth = 0.4f, _currGridSize, _currQuesSize;
        [SerializeField] private float gridSpacing = 0.1f, quesSpacing = 0.2f;

        [Header("Input Data Info")] TextMeshProUGUI _levelNumText;

        public string LevelNumData
        {
            get => _levelNumText.text;
            set => _levelNumText.text = value;
        }

        public void StartInit()
        {
            _cam = Camera.main;
            if (_cam != null) _camOrthographicSize = _cam.orthographicSize;
            //Debug.Log("Level StartInit Called !");
            SetGridSize();
            CreateGrid();
            SetQuestionGrid();
            _levelNumText = GameController.instance.uiManager.levelText;
            LevelNumData = $"Level {DataHandler.instance.CurrLevelNumber + 1}";
        }

        public void FillData(LevelHandler handler)
        {
            AssignLevelHandler(handler);
        }

        void SetQuestionGrid()
        {
            float row = (float)totalWordToFind / quesInOneRow;
            float quesTileSize = _camOrthographicSize / 2 + camGridOffset;

            float spacingX = (totalWordToFind - 1) * quesSpacing;
            float spacingY = (int)row * quesSpacing;

            float gridWidth = (quesTileSize - spacingX) / totalWordToFind;
            float gridHeight = (quesTileSize - spacingY) / totalWordToFind;

            if (gridWidth > gridHeight)
            {
                _currQuesSize = gridWidth;
            }
            else
            {
                _currQuesSize = gridHeight;
            }

            Debug.Log("ROW VAL : " + row);
            Vector2 startPos;
            if (row > 1)
            {
                for (int i = 0; i < totalWordToFind; i++)
                {
                }
            }
            else
            {
                startPos = new Vector2(-_camOrthographicSize / 2 + _currQuesSize + camGridOffset,
                    quesGridTrans.position.y);
                GameObject quesPrefab = DataHandler.instance.quesPrefab;

                for (int i = 0; i < totalWordToFind; i++)
                {
                    GameObject gmObj = Instantiate(quesPrefab, quesGridTrans);
                    QuesTile quesTileScript = gmObj.GetComponent<QuesTile>();
                    gmObj.transform.localScale = Vector3.one * _currQuesSize;
                    gmObj.transform.position = startPos;
                    gmObj.name = $"Ques_{i}";
                    _levelHandler.UpdateQuesList(quesTileScript);
                    startPos.x += quesSpacing + _currQuesSize;
                }
            }
            
            
        }

        public float GetLineRendererWidth()
        {
            return _lineRendererWidth;
        }

        private void SetLineRendererWidth()
        {
            var size = _currGridWidth > _currGridHeight ? _currGridHeight : _currGridWidth;
            _lineRendererWidth = size / 250f;
        }

        public void SetLineRendererPoint(int index, Vector2 mousePos)
        {
            RectTransform gridRect = gridContainer.GetComponent<RectTransform>();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRect, mousePos, _levelHandler.cam,
                out var canvasMousePos);
        }

        private void AssignLevelHandler(LevelHandler handler)
        {
            _levelHandler = handler;
        }

        public Transform GetGridContainerTrans()
        {
            return gridContainer;
        }

        void SetGridSize()
        {
            float totalGridPlacementSize = _camOrthographicSize - (camGridOffset * 2);
            _gridContainerSize = new Vector3(totalGridPlacementSize, totalGridPlacementSize, totalGridPlacementSize);
            float spacingX = (gridSize.y - 1) * gridSpacing;
            float spacingY = (gridSize.x - 1) * gridSpacing;

            float gridWidth = (_gridContainerSize.x - spacingX) / (gridSize.y);
            float gridHeight = (_gridContainerSize.y - spacingY) / (gridSize.x);

            _currGridWidth = gridWidth;
            _currGridHeight = gridHeight;

            if (_currGridHeight > _currGridWidth)
            {
                _currGridSize = _currGridWidth;
            }
            else
            {
                _currGridSize = _currGridHeight;
            }
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
                    randomX = Random.Range(-orthographicSize / 2 - distanceFromScreen,
                        -orthographicSize / 2 - (distanceFromScreen * 2));
                    randomY = Random.Range(-orthographicSize / 2, orthographicSize);
                }
                else if (_randomScreenPointArr[xPoint] == 1)
                {
                    //When the side is selected as Right Side.
                    randomX = Random.Range(orthographicSize / 2 + distanceFromScreen,
                        orthographicSize / 2 + (distanceFromScreen * 2));
                    randomY = Random.Range(-orthographicSize / 2, orthographicSize);
                }
            }
            else
            {
                int yPoint = Random.Range(0, _randomScreenPointArr.Length);

                if (_randomScreenPointArr[yPoint] == -1)
                {
                    //When the side is selected as Bottom Side.
                    randomX = Random.Range(-orthographicSize / 2, -orthographicSize / 2 - distanceFromScreen);
                    randomY = Random.Range(-orthographicSize / 2 - distanceFromScreen,
                        -orthographicSize / 2 - (distanceFromScreen * 2));
                }
                else if (_randomScreenPointArr[yPoint] == 1)
                {
                    //When the side is selected as Top Side.
                    randomX = Random.Range(orthographicSize, orthographicSize + distanceFromScreen);
                    randomY = Random.Range(orthographicSize / 2 + distanceFromScreen,
                        orthographicSize / 2 + (distanceFromScreen * 2));
                }
            }

            return new Vector2(randomX, randomY);
        }

        private void CreateGrid()
        {
            GameObject gridPrefab = DataHandler.instance.gridPrefab;
            _defaultStartPos.y = defaultStartYPos;
            _defaultStartPos.x = -_camOrthographicSize / 2 + camGridOffset + _currGridWidth / 2;
            Vector3 startPos = new Vector3(_defaultStartPos.x, _defaultStartPos.y, _defaultStartPos.z);
            //Debug.Log("Start Pos : " + startPos);
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    GameObject gmObj = Instantiate(gridPrefab, gridContainer);
                    GridTile gridTileScript = gmObj.GetComponent<GridTile>();

                    //Assigning New Material to each grid.
                    gmObj.GetComponent<Renderer>().material = new Material(gridMaterial);
                    gridTileScript.gridMaterial = gmObj.GetComponent<Renderer>().material;
                    gmObj.transform.localScale = Vector3.one * _currGridSize;
                    gridTileScript.DefaultGridSize(startPos);
                    gmObj.transform.position = GetRandomPointOutOfScreen();
                    gmObj.name = $"Grid_{i}_{j}";
                    gridTileScript.AssignInfo(this);
                    gridTileScript.GridID = new Vector2Int(i, j);
                    _levelHandler.totalGridsList.Add(gridTileScript);
                    AssignGridData(gridTileScript, i, j);
                    gmObj.transform.DOLocalMove(startPos, timeToPlaceGrid).SetEase(gridPlacementEase);

                    gridTileScript.SetGridBg(DataHandler.instance.CurrLevelNumber < _levelHandler.showGridTillLevel);

                    startPos.x += gridSpacing + _currGridSize;
                }

                startPos = new Vector3(_defaultStartPos.x,
                    _defaultStartPos.y - ((i + 1) * _currGridHeight) - (gridSpacing * (i + 1)),
                    _defaultStartPos.z);
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