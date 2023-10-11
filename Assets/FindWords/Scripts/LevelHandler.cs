using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace YugantLoyaLibrary.FindWords
{
    public class LevelHandler : MonoBehaviour
    {
        public delegate void NewLetterDelegate(GridTile gridTile);

        public NewLetterDelegate onNewLetterAddEvent, onRemoveLetterEvent;

        public delegate void GameCompleteDelegate();

        private GameCompleteDelegate _onGameCompleteEvent;

        [Header("References")] [HideInInspector]
        public Camera cam;

        public ParticleSystem touchEffectRef;
        public EnglishDictWords englishDictWords;
        private Renderer _touchEffectRenderer;
        [SerializeField] Level currLevel;
        [HideInInspector] public int quesGridCount;
        public int showGridTillLevel = 4;
        [FormerlySerializedAs("coinPerLevel")] public int coinPerLetter = 10;
        [Header("Level Info")] public char[][] gridData;
        public float timeToShowHint = 0.5f, timeToRotateGrid = 0.5f;
        public List<Color> levelLineColorList = new List<Color>();

        public List<GridTile> totalGridsList,
            inputGridsList,
            wordCompletedGridList,
            unlockedGridList,
            buyGridList,
            buyQuesGridList;

        public List<QuesTile> quesTileList;
        public List<string> selectedWords;
        char[] _vowelCharArr = new[] { 'a', 'e', 'i', 'o', 'u' };
        bool _isLevelRunning = true;
        private int _colorIndex;
        private QuesTile _lastQuesTile;
        
        private void OnEnable()
        {
            onNewLetterAddEvent += AddNewLetter;
            onRemoveLetterEvent += RemoveLetter;
            _onGameCompleteEvent += LevelComplete;
        }

        private void OnDisable()
        {
            onNewLetterAddEvent -= AddNewLetter;
            onRemoveLetterEvent -= RemoveLetter;
            _onGameCompleteEvent -= LevelComplete;
        }

        private void Awake()
        {
            Init();
        }

        void Init()
        {
            //Debug.Log("Level Handler Init Called !");
            cam = Camera.main;
            totalGridsList = new List<GridTile>();
            inputGridsList = new List<GridTile>();
            wordCompletedGridList = new List<GridTile>();
            _touchEffectRenderer = touchEffectRef.GetComponent<Renderer>();
        }

        public void LevelStartInit()
        {
            _isLevelRunning = true;
        }

        public QuesTile LastQuesTile
        {
            get => _lastQuesTile;
            set => _lastQuesTile = value;
        }

        public void GetGridData()
        {
            // TextAsset levelTextFile = GameController.instance.GetGridDataOfLevel();
            // //string levelData = levelTextFile.text.Trim();
            // string levelData = levelTextFile.text;
            // //Debug.Log("Level Data String : " + levelData);

            // string[] lines = levelData.Split('\n');
            // //Debug.Log("Lines : " + lines.Length);

            int row = currLevel.gridSize.x;
            int col = currLevel.gridSize.y;
            gridData = new char[row][];


            int fillVowel = (row * col) / 3;
            int[][] tempArr = new int[row][];

            for (int i = 0; i < fillVowel; i++)
            {
            }

            for (int i = 0; i < row; i++)
            {
                gridData[i] = new char[col];

                for (int j = 0; j < col; j++)
                {
                    gridData[i][j] = GenerateRandom_ASCII_Code()[0];
                }
            }


            // List<LevelDataInfo.WordInfo> list = GameController.instance.GetLevelDataInfo().words;
            //
            // foreach (var word in list)
            // {
            //     LevelWords levelWords = new LevelWords
            //     {
            //         wordInfo = word
            //     };
            //
            //     wordList.Add(levelWords);
            // }
        }

        public string GenerateRandom_ASCII_Code()
        {
            int randomAsciiVal = UnityEngine.Random.Range(065, 091);
            char letter = (char)randomAsciiVal;

            return letter.ToString();
        }

        public void SetLevelRunningBool(bool canTouch = true)
        {
            _isLevelRunning = canTouch;
        }

        public bool GetLevelRunningBool()
        {
            return _isLevelRunning;
        }

        public void AssignLevel(Level levelScript)
        {
            currLevel = levelScript;
        }

        void AddNewLetter(GridTile gridTileObj)
        {
            if (gridTileObj != null)
            {
                int index = FindEmptyQuesGrid();

                if (index != -1)
                {
                    Vector3 pos = quesTileList[index].transform.position;

                    // if (gridTileObj.defaultGridPos == gridTileObj.transform.position)
                    //     return;

                    if (index == quesTileList.Count - 1)
                    {
                        _isLevelRunning = false;
                        LastQuesTile = quesTileList[index];
                    }

                    quesTileList[index].isAssigned = true;
                    inputGridsList.Add(gridTileObj);
                    StartCoroutine(LevelRunningStatus(true, gridTileObj.reachTime + 0.1f));
                    gridTileObj.Move(pos, true, quesTileList[index]);
                }
            }
        }

        void RemoveLetter(GridTile gridTileObj)
        {
            if (gridTileObj != null)
            {
                inputGridsList.Remove(gridTileObj);
                gridTileObj.Move(gridTileObj.defaultGridPos, false, gridTileObj.placedOnQuesTile);
                StartCoroutine(LevelRunningStatus(true, gridTileObj.reachTime + 0.1f));
            }
        }

        IEnumerator LevelRunningStatus(bool isActive, float time)
        {
            yield return new WaitForSeconds(time);
            _isLevelRunning = isActive;
        }

        private int FindEmptyQuesGrid()
        {
            for (int i = 0; i < quesTileList.Count; i++)
            {
                QuesTile quesTile = quesTileList[i];

                if (!quesTile.isAssigned)
                {
                    return i;
                }
            }

            return -1;
        }

        public void UpdateQuesList(QuesTile quesTileScript)
        {
            quesTileList.Add(quesTileScript);
        }


        public void CheckAllGridBuyed()
        {
            if (buyGridList.Count <= 0)
            {
                DataHandler.instance.CurrGridSize++;
                _isLevelRunning = false;
                float time = currLevel.timeToWaitForEachGrid;
                float timeToPlaceGrids = currLevel.timeToPlaceGrid;
                StartCoroutine(ReturnToDeck(totalGridsList, time, timeToPlaceGrids, false));
                StartCoroutine(
                    GameController.instance.StartGameAfterCertainTime(timeToPlaceGrids +
                                                                      (time * (totalGridsList.Count + 1))));
            }
        }

        public void CheckAnswer()
        {
            _isLevelRunning = false;
            bool isBlocksUsed = quesTileList.All(quesTile => quesTile.isAssigned);

            if (!isBlocksUsed)
                return;

            //Check Word Exists or Not !
            string word = WordFormed();
            bool doWordExist = englishDictWords.Search(word);

            if (doWordExist)
            {
                WordComplete();
            }
            else
            {
                GridsBackToPos();
            }
        }


        private void AddCoin()
        {
            UIManager.instance.CoinCollectionAnimation(coinPerLetter * quesGridCount);
            Debug.Log("Coin Per Word To Add: " + coinPerLetter * quesGridCount);
        }

        void WordComplete()
        {
            for (int index = 0; index < inputGridsList.Count; index++)
            {
                var gridTile = inputGridsList[index];
                wordCompletedGridList.Add(gridTile);
                gridTile.Blast();
                gridTile.SetQuesTileStatus(gridTile.placedOnQuesTile, false, gridTile.blastTime / 2);

                if (index == inputGridsList.Count - 1)
                {
                    AddCoin();
                }
            }

            RevertQuesData();
        }

        private void GridsBackToPos()
        {
            foreach (var gridTile in inputGridsList)
            {
                gridTile.Move(gridTile.defaultGridPos, false, gridTile.placedOnQuesTile);
                gridTile.SetQuesTileStatus(gridTile.placedOnQuesTile, false, gridTile.blastTime / 2);
                StartCoroutine(LevelRunningStatus(true, gridTile.reachTime + 0.1f));
            }

            LastQuesTile = null;
            RevertQuesData();
        }

        public void RevertQuesData()
        {
            foreach (QuesTile quesTile in quesTileList)
            {
                quesTile.RevertData();
            }


            inputGridsList.Clear();
        }

        string WordFormed()
        {
            string str = "";
            foreach (QuesTile quesTile in quesTileList)
            {
                str += quesTile.QuesTextData;
            }

            return str;
        }

        private void LevelComplete()
        {
            _isLevelRunning = false;
            UIManager.instance.CoinCollectionAnimation(coinPerLetter);

            //GameController.instance.NextLevel();
        }

        public IEnumerator ReturnToDeck(List<GridTile> gridLists, float waitTime, float timeToPlaceGrids,
            bool shouldReturnToGridPlace = true)
        {
            Vector2 pos = currLevel.BottomOfScreenPoint();

            foreach (GridTile gridTile in gridLists)
            {
                yield return new WaitForSeconds(waitTime);
                gridTile.DeckAnimation(timeToPlaceGrids, pos, shouldReturnToGridPlace);
            }

            yield return null;
        }

        public void ShowHint()
        {
        }

        private void PlayHintAnimation(GridTile gridTile, int index)
        {
        }

        void RemoveHint()
        {
            
        }
    }
}