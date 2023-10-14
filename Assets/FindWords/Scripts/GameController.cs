using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace YugantLoyaLibrary.FindWords
{
    public class GameController : MonoBehaviour
    {
        public static GameController instance;
        public bool isTesting;
        private static readonly char[] _vowels = { 'a', 'e', 'i', 'o', 'u' };

        private static readonly char[] _consonants =
        {
            'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n',
            'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z'
        };

        [Range(4, 7)] public int startingGridSize = 5;
        [HideInInspector] public int startingQuesSize = 3, maxGridSize = 8;

        [Header("References")] [SerializeField]
        private DifficultyDataInfo difficultyDataInfo;

        [SerializeField] private DefinedLevelScriptable definedLevelInfo;
        [SerializeField] private GridCamScriptable gridCamScriptable;
        [SerializeField] private MainDictionary mainDictionary;
        [SerializeField] private LevelHandler levelHandler;
        [SerializeField] private Transform levelContainer;
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        public Transform coinContainerTran;
        public int coinPoolSize = 20;
        private Level _currLevel;
        private bool _isRestarting;
        [SerializeField] private float timeToSwitchToNextLevel = 1f;

        public enum CurrDifficulty
        {
            VERY_EASY,
            EASY,
            MODERATE,
            HARD,
            EXPERT
        }

        public CurrDifficulty currDiff = CurrDifficulty.VERY_EASY;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            CreateSingleton();
            Vibration.Init();
        }

        private void Start()
        {
            GameStartInfo();
            levelHandler.SetHelperLevelDataScriptable();
            StartGame();
        }

        private void CreateSingleton()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(this.gameObject);
            }
        }

        void CreateLevel()
        {
            GameObject level = Instantiate(DataHandler.instance.levelPrefab, levelContainer);
            _currLevel = level.GetComponent<Level>();
            AssignLevelData();
        }

        private void AssignLevelData()
        {
            _currLevel.FillData(levelHandler);
            levelHandler.AssignLevel(_currLevel);
            levelHandler.LevelStartInit();
            //_currLevel.gridSize = GetLevelDataInfo().gridSize;
            _currLevel.gridSize = new Vector2Int(DataHandler.CurrGridSize, DataHandler.CurrGridSize);
            levelHandler.GetGridData();
            _currLevel.StartInit();
        }

        public GridCamScriptable GetGridCamScriptable()
        {
            return gridCamScriptable;
        }

        public DefinedLevelScriptable GetDefinedLevelInfoScriptable()
        {
            return definedLevelInfo;
        }

        public DifficultyDataInfo.DifficultyInfo GetDifficultyInfo()
        {
            //Debug.Log("Curr Level Num : " + DataHandler.instance.CurrLevelNumber);
            return difficultyDataInfo.difficultyInfos[DataHandler.instance.CurrDifficultyNumber];
        }

        public DifficultyDataInfo GetDifficultyDataInfo()
        {
            return difficultyDataInfo;
        }

        void GameStartInfo()
        {
            DataHandler.instance.CurrDifficultyNumber = (int)currDiff;
            UIManager.instance.coinText.text = DataHandler.TotalCoin.ToString();
            DataHandler.CurrDefinedLevel = 0;
            DataHandler.HelperWordIndex = 0;
            DataHandler.CurrGridSize = startingGridSize;
            levelHandler.totalQuesGridCount = startingQuesSize;
            levelHandler.currUnlockedQuesGridCount = startingQuesSize;
            levelHandler.englishDictWords.UpdateEnglishDict();
        }

        void ClearContainer(Transform container)
        {
            if (container.childCount > 0)
            {
                for (int i = container.childCount - 1; i >= 0; i--)
                {
                    Destroy(container.GetChild(i).gameObject);
                }
            }
        }

        public Level GetCurrentLevel()
        {
            return _currLevel;
        }

        public LevelHandler GetLevelHandler()
        {
            return levelHandler;
        }

        public IEnumerator StartGameAfterCertainTime(float time)
        {
            Debug.Log($"Game Restarting After {time}");
            //_currLevel.
            DataHandler.instance.CurrDifficultyNumber++;
            int minLetter = GetDifficultyInfo().minQuesLetter;
            levelHandler.currUnlockedQuesGridCount = levelHandler.totalQuesGridCount;

            if (minLetter > DataHandler.CurrQuesSize)
            {
                DataHandler.CurrQuesSize = minLetter;
            }
            else if (minLetter == DataHandler.CurrQuesSize && minLetter != startingQuesSize)
            {
                levelHandler.totalQuesGridCount = DataHandler.CurrQuesSize + 1;
            }

            yield return new WaitForSeconds(time);
            StartGame();
        }

        private void StartGame()
        {
            ResetData();

            UIManager.instance.coinText.text = DataHandler.TotalCoin.ToString();

            if (!isTesting)
            {
                ClearContainer(levelContainer);
                CreateLevel();
            }
            else
            {
                Level level = FindObjectOfType<Level>();

                if (level != null)
                {
                    ClearContainer(level.GetGridContainerTrans());
                    _currLevel = level;
                    AssignLevelData();
                }
            }
        }

        public void RestartLevel()
        {
            StartGame();
        }

        private void RotateGridContainer()
        {
            Transform trans = _currLevel.rotationContainer;
            levelHandler.SetLevelRunningBool(false);
            Transform gridContainer = _currLevel.GetGridContainerTrans();
            if (_currLevel.gridSize.y == _currLevel.gridSize.x)
            {
                Debug.Log("Rotate If !");
                trans.DOScale(0.8f, levelHandler.timeToRotateGrid / 2).OnComplete(() =>
                {
                    trans.DOScale(1f, levelHandler.timeToRotateGrid / 2);
                });

                Quaternion localRotation = trans.localRotation;
                trans.DORotate(
                    new Vector3(localRotation.eulerAngles.x, localRotation.eulerAngles.y,
                        localRotation.eulerAngles.z + 90f), levelHandler.timeToRotateGrid).OnComplete(() =>
                {
                    _isRestarting = false;
                    levelHandler.SetLevelRunningBool(true);
                });

                for (int i = 0; i < gridContainer.childCount; i++)
                {
                    GameObject gm = gridContainer.GetChild(i).gameObject;
                    Quaternion rotation = gm.transform.localRotation;
                    gm.transform.DOLocalRotate(
                        new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z - 90f),
                        levelHandler.timeToRotateGrid);
                }
            }
            else
            {
                Debug.Log("Rotate Else !");

                trans.DOScale(0.8f, levelHandler.timeToRotateGrid).OnComplete(() =>
                {
                    trans.DOScale(1f, levelHandler.timeToRotateGrid);
                });

                Quaternion localRotation = trans.localRotation;
                trans.DORotate(
                    new Vector3(localRotation.eulerAngles.x, localRotation.eulerAngles.y,
                        localRotation.eulerAngles.z + 180f), levelHandler.timeToRotateGrid * 2).OnComplete(() =>
                {
                    _isRestarting = false;
                    levelHandler.SetLevelRunningBool(true);
                });

                for (int i = 0; i < gridContainer.childCount; i++)
                {
                    GameObject gm = gridContainer.GetChild(i).gameObject;
                    Quaternion rotation = gm.transform.localRotation;
                    gm.transform.DOLocalRotate(
                        new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z - 180f),
                        levelHandler.timeToRotateGrid * 2);
                }
            }
        }

        public void ResetData()
        {
            levelHandler.inputGridsList.Clear();
            levelHandler.unlockedGridList.Clear();
            levelHandler.totalGridsList.Clear();
            levelHandler.quesTileList.Clear();
            levelHandler.buyGridList.Clear();
            levelHandler.wordCompletedGridList.Clear();
            levelHandler.gridAvailableOnScreenList.Clear();
        }

        void ResetInGameData()
        {
            levelHandler.inputGridsList.Clear();
            levelHandler.wordCompletedGridList.Clear();
        }

        public void ShuffleGrid()
        {
            levelHandler.SetLevelRunningBool(false);
            ResetInGameData();
            float time = _currLevel.timeToWaitForEachGrid;
            float timeToPlaceGrids = _currLevel.timeToPlaceGrid;
            List<GridTile> list = new List<GridTile>(levelHandler.unlockedGridList);
            ShuffleList(list);
            StartCoroutine(levelHandler.ReturnToDeck(list, time, timeToPlaceGrids));
            levelHandler.RevertQuesData();
        }

        public void Deal()
        {
            if (levelHandler.wordCompletedGridList.Count > 0)
            {
                List<GridTile> list = new List<GridTile>(levelHandler.wordCompletedGridList);
                ShuffleList(list);
                StartCoroutine(BackToDeckAnim(list));
            }
        }

        public void Hint()
        {
            levelHandler.ShowHint();
        }

        IEnumerator BackToDeckAnim(List<GridTile> list)
        {
            int totalVowel = list.Count / 2 + 1;
            foreach (GridTile gridTile in list)
            {
                yield return new WaitForSeconds(_currLevel.timeToWaitForEachGrid);
                Debug.Log("Moving To Grid Place Again !");
                gridTile.isBlastAfterWordComplete = false;
                gridTile.GridTextData = totalVowel > 0 ? RandomVowel().ToString() : RandomConsonent().ToString();
                gridTile.MoveTowardsGrid();
                levelHandler.wordCompletedGridList.Remove(gridTile);
                totalVowel--;
            }

            levelHandler.SetHintButtonActivationStatus();
            StartCoroutine(ResetLevelHandlerData(_currLevel.timeToWaitForEachGrid * list.Count));
        }

        public static char RandomConsonent()
        {
            int randomIndex = UnityEngine.Random.Range(0, _consonants.Length);
            return _consonants[randomIndex];
        }

        public static char RandomVowel()
        {
            int randomIndex = UnityEngine.Random.Range(0, _vowels.Length);
            return _vowels[randomIndex];
        }

        public static void ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            for (int i = 0; i < n - 1; i++)
            {
                int j = UnityEngine.Random.Range(i, n);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        IEnumerator ResetLevelHandlerData(float time)
        {
            yield return new WaitForSeconds(time);
            levelHandler.LastQuesTile = null;
            levelHandler.SetLevelRunningBool(true);
            levelHandler.SetHintButtonActivationStatus();
        }


        void FadeScreen()
        {
            fadeCanvasGroup.DOFade(1f, timeToSwitchToNextLevel / 2f).OnComplete(() =>
            {
                StartGame();
                fadeCanvasGroup.DOFade(0f, timeToSwitchToNextLevel / 2f).OnComplete(() => { });
            });
        }

        public List<MainDictionary.WordLengthDetailedInfo> GetWordListOfLength(int wordLength, string startingLetter)
        {
            List<MainDictionary.WordLengthDetailedInfo> wordList = new List<MainDictionary.WordLengthDetailedInfo>();

            List<MainDictionary.MainDictionaryInfo> mainList = mainDictionary.dictInfoList;
            for (int i = 0; i < mainList.Count; i++)
            {
                //Debug.Log("Word Length = " + mainList[i].wordLength + " " + wordLength);
                if (mainList[i].wordLength == wordLength)
                {
                    List<MainDictionary.WordLengthDetailedInfo> wordInfoList = mainList[i].wordsInfo;

                    for (int j = 0; j < wordInfoList.Count; j++)
                    {
                        if (startingLetter == wordInfoList[j].wordStartChar.ToString().ToLower())
                        {
                            wordList = new List<MainDictionary.WordLengthDetailedInfo>(wordInfoList);
                            //Debug.Log("Word List Count : " + wordList.Count);
                            return wordList;
                        }
                    }
                }
            }

            return wordList;
        }

        public bool Search(string word)
        {
            List<MainDictionary.MainDictionaryInfo> dictInfoList = mainDictionary.dictInfoList;
            Debug.Log("DictInfo List Count : " + dictInfoList.Count);
            bool isWordThere = IsWordAvailable(word, dictInfoList);

            if (isWordThere)
                return true;

            return false;
        }

        public bool IsWordAvailable(string word, List<MainDictionary.MainDictionaryInfo> dictInfoList)
        {
            for (int i = 0; i < dictInfoList.Count; i++)
            {
                if (dictInfoList[i].wordLength != word.Length)
                    continue;

                List<MainDictionary.WordLengthDetailedInfo> wordInfoList = dictInfoList[i].wordsInfo;

                for (int j = 0; j < wordInfoList.Count; j++)
                {
                    int index = 0;

                    if (wordInfoList[j].wordStartChar == word[index])
                    {
                        Debug.Log("First Char of Word : " + word[index]);
                        index++;
                        TextAsset file = wordInfoList[j].wordText;
                        string[] lines = file.text.Split('\n');

                        foreach (string str in lines)
                        {
                            Debug.Log("Search Word : " + str);

                            bool isWordFound = IsWordSame(index, word, str);

                            if (isWordFound)
                            {
                                Debug.Log(word + " Word Found !!");
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public bool IsWordSame(int index, string word, string textWord)
        {
            if (word.Length < index || textWord.Length < index)
            {
                return false;
            }

            if (word[index] == textWord[index])
            {
                Debug.Log($"{word} Word Same !! ");

                if (word.Length - 1 == index)
                {
                    Debug.Log($"{word} Word Detected !! ");
                    return true;
                }

                index++;
                return IsWordSame(index, word, textWord);
            }

            Debug.Log($"Letter Not Same {word} , {textWord} , {index} ");
            return false;
        }
    }
}