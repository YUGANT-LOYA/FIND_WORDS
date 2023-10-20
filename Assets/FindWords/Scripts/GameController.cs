using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

namespace YugantLoyaLibrary.FindWords
{
    public class GameController : MonoBehaviour
    {
        public static GameController instance;
        public bool isTesting;
        private static readonly char[] Vowels = { 'a', 'e', 'i', 'o', 'u' };

        private static readonly char[] Consonants =
        {
            'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n',
            'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z'
        };

        [Range(4, 7)] public int startingGridSize = 5;

        [Tooltip("Level Without Shuffling the Letter and copy exact letters for keeping some level easy !")]
        public int maxHelperIndex = 3;

        [HideInInspector] public int startingQuesSize = 3, maxGridSize = 7;
        public int defaultIq = 5;

        [Header("References")] [SerializeField]
        private PickWordDataInfo pickWordDataInfo;

        [SerializeField] private CoinHandlerScriptable coinHandlerScriptable;
        [SerializeField] private DefinedLevelScriptable definedLevelInfo;
        [SerializeField] private GridCamScriptable gridCamScriptable;
        [SerializeField] private MainDictionary mainDictionary;
        [SerializeField] private LevelHandler levelHandler;
        [SerializeField] private Transform levelContainer;
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        public Transform coinContainerTran;
        public int coinPoolSize = 10, shuffleUsingCoin = 30, dealUsingCoin = 10, hintUsingCoin = 20;
        private Level _currLevel;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            CreateSingleton();
            Vibration.Init();
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

        private void Start()
        {
            if (DataHandler.FirstTimeGameOpen == 0)
            {
                DataHandler.FirstTimeGameOpen = 1;
                GameStartInfo();
            }

            levelHandler.englishDictWords.UpdateEnglishDict();
            Init();
            StartGame();
        }

        void GameStartInfo()
        {
            Debug.Log("First Time Game Start Info Called !!");
            UIManager.instance.coinText.text = DataHandler.TotalCoin.ToString();
            DataHandler.CurrDefinedLevel = 0;
            DataHandler.PickDataIndex = 0;
            DataHandler.HelperWordIndex = 0;
            DataHandler.CurrGridSize = startingGridSize;
            UIManager.instance.iqLevelText.text = $"IQ : {DataHandler.IqLevel.ToString()}";
        }

        private void Init()
        {
            UIManager.instance.coinText.text = DataHandler.TotalCoin.ToString();
        }

        public IEnumerator StartGameAfterCertainTime(float time)
        {
            Debug.Log($"Game Restarting After {time}");

            DataHandler.CurrTotalQuesSize = DataHandler.CurrGridSize;
            DataHandler.UnlockedQuesLetter = DataHandler.CurrTotalQuesSize;
            DataHandler.CurrGridSize++;
            DataHandler.UnlockGridIndex = 0;
            DataHandler.NewGridCreated = 0;
            yield return new WaitForSeconds(time);

            StartGame();
        }

        private void StartGame()
        {
            ResetData();

            //Give Condition for Updating The Word Left List !
            levelHandler.UpdateWordLeftList();

            ClearContainer(levelContainer);
            CreateLevel();
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
            _currLevel.gridSize = new Vector2Int(DataHandler.CurrGridSize, DataHandler.CurrGridSize);

            if (DataHandler.NewGridCreated == 0)
            {
                DataHandler.NewGridCreated = 1;
                Debug.Log("New Data Created !!");
                levelHandler.GetGridLetterData();
            }
            else
            {
                Debug.Log("Loaded Previous Data !!");
                levelHandler.ReadAlreadyCreatedGrid();
            }

            _currLevel.StartInit();
            levelHandler.LevelStartInit();
        }

        public GridCamScriptable GetGridCamScriptable()
        {
            return gridCamScriptable;
        }

        public DefinedLevelScriptable GetDefinedLevelInfoScriptable()
        {
            return definedLevelInfo;
        }

        public PickWordDataInfo GetPickDataInfo()
        {
            return pickWordDataInfo;
        }

        public CoinHandlerScriptable GetCoinDataScriptable()
        {
            return coinHandlerScriptable;
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

        private void ResetData()
        {
            levelHandler.ClearAllLists();
        }

        void ResetInGameData()
        {
            levelHandler.ClearInGameList();
        }

        public void ShuffleGrid()
        {
            if (!levelHandler.GetLevelRunningBool() || DataHandler.TotalCoin < shuffleUsingCoin)
                return;

            UIManager.SetCoinData(shuffleUsingCoin, -1);
            StartCoroutine(UIManager.instance.UpdateReducedCoinText(0f, shuffleUsingCoin, 0.5f));

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
            if (!levelHandler.GetLevelRunningBool() || levelHandler.wordCompletedGridList.Count <= 0 ||
                DataHandler.TotalCoin < dealUsingCoin)
                return;

            UIManager.SetCoinData(dealUsingCoin, -1);
            List<GridTile> list = new List<GridTile>(levelHandler.wordCompletedGridList);
            ShuffleList(list);
            StartCoroutine(BackToDeckAnim(list));
            StartCoroutine(UIManager.instance.UpdateReducedCoinText(0f, dealUsingCoin, 0.5f));
            UIManager.instance.DealButtonEffect();
        }

        public void Hint()
        {
            if (!levelHandler.GetLevelRunningBool() || DataHandler.TotalCoin < hintUsingCoin)
                return;

            UIManager.SetCoinData(hintUsingCoin, -1);
            levelHandler.ShowHint();
            StartCoroutine(UIManager.instance.UpdateReducedCoinText(0f, hintUsingCoin, 0.5f));
        }

        IEnumerator BackToDeckAnim(List<GridTile> list)
        {
            string randomPickedWord = "";
            List<GridTile> tempList = new List<GridTile>(levelHandler.gridAvailableOnScreenList);
            int total = list.Count;
            string remainingLetter = "";
            string gridExistingString = "";
            List<string> hintFitStringList = new List<string>();

            while (total > 0)
            {
                Debug.Log("Forming Word From Screen Letters");
                string tempStr =
                    levelHandler.PickRandomWordFormingLetters(tempList, 2, out gridExistingString);

                Debug.Log("Temp Word : " + tempStr);

                if (tempStr.Length > 0)
                {
                    remainingLetter = tempStr;

                    foreach (char repeatedChar in gridExistingString)
                    {
                        Debug.Log("Repeated Char" + repeatedChar);
                        remainingLetter = RemoveCharFromString(remainingLetter, repeatedChar);
                        Debug.Log("Remaining Letter : " + remainingLetter);


                        foreach (GridTile tile in tempList)
                        {
                            if (tile.GridTextData != repeatedChar.ToString()) continue;

                            Debug.Log("Removed Tile : " + tile.name);
                            tempList.Remove(tile);
                            break;
                        }
                    }

                    Debug.Log("Temp List Count : " + tempList.Count);

                    foreach (GridTile tile in tempList)
                    {
                        Debug.Log("Temp Element : " + tile);
                    }

                    hintFitStringList.Add(tempStr);
                    randomPickedWord += remainingLetter.Trim();
                }
                else
                {
                    hintFitStringList.Clear();
                    Debug.Log("Printing Data From Word Left List ");
                    randomPickedWord = levelHandler.GetDataAccordingToGrid(list.Count);
                    total -= randomPickedWord.Length;
                }

                total -= remainingLetter.Length;
                Debug.Log("Total : " + total);
                //Debug.Log("RANDOM Pick Letter : " + randomPickedWord);
            }

            foreach (string s in hintFitStringList)
            {
                levelHandler.hintAvailList.Add(s);
                Debug.Log("Hint Added : " + s);
            }

            Debug.Log("Remaining Letter : " + remainingLetter);
            //Debug.Log("RANDOM Pick Letter : " + randomPickedWord);
            //Debug.Log("Temp STR Count : " + tempList.Count);
            //Debug.Log("RANDOM PICK VAL : " + randomPickedWord);

            int index = 0;
            foreach (GridTile gridTile in list)
            {
                yield return new WaitForSeconds(_currLevel.timeToWaitForEachGrid);
                //Debug.Log("Moving To Grid Place Again !");
                gridTile.isBlastAfterWordComplete = false;
                gridTile.GridTextData = randomPickedWord[index].ToString();
                levelHandler.gridAvailableOnScreenList.Add(gridTile);
                gridTile.MoveTowardsGrid();
                levelHandler.wordCompletedGridList.Remove(gridTile);
                index++;
            }

            StartCoroutine(ResetLevelHandlerData(_currLevel.timeToWaitForEachGrid * list.Count));
        }

        string RemoveCharFromString(string originalString, char charToRemove)
        {
            string modifiedString = originalString;
            int indexToRemove = 0;

            for (var index = 0; index < modifiedString.Length; index++)
            {
                var c = modifiedString[index];
                if (c != charToRemove) continue;
                indexToRemove = index;
                break;
            }

            if (indexToRemove < 0 || indexToRemove >= modifiedString.Length)
            {
                return modifiedString;
            }

            return modifiedString.Substring(0, indexToRemove) + modifiedString.Substring(indexToRemove + 1);
        }

        public static char RandomConsonent()
        {
            int randomIndex = UnityEngine.Random.Range(0, Consonants.Length);
            return Consonants[randomIndex];
        }

        public static char RandomVowel()
        {
            int randomIndex = UnityEngine.Random.Range(0, Vowels.Length);
            return Vowels[randomIndex];
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
            //levelHandler.SetHintButtonActivationStatus();
        }

        public List<MainDictionary.WordLengthDetailedInfo> GetWordListOfLength(int wordLength,
            string startingLetter)
        {
            List<MainDictionary.WordLengthDetailedInfo>
                wordList = new List<MainDictionary.WordLengthDetailedInfo>();

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

        private bool IsWordAvailable(string word, List<MainDictionary.MainDictionaryInfo> dictInfoList)
        {
            for (int i = 0; i < dictInfoList.Count; i++)
            {
                if (dictInfoList[i].wordLength != word.Length)
                    continue;

                List<MainDictionary.WordLengthDetailedInfo> wordInfoList = dictInfoList[i].wordsInfo;

                for (int j = 0; j < wordInfoList.Count; j++)
                {
                    if (wordInfoList[j].wordStartChar == word[0])
                    {
                        Debug.Log("First Char of Word : " + word[0]);
                        List<string> wordList = wordInfoList[j].wordList;

                        foreach (string str in wordList)
                        {
                            // Debug.Log("Search Word : " + str);
                            // Debug.Log("Search Word Length : " + str.Length);
                            // Debug.Log(" Word Length: " + word.Length);

                            if (string.Equals(str.Trim(), word, StringComparison.CurrentCultureIgnoreCase))
                            {
                                return true;
                            }

                            // bool isWordFound = IsWordSame(index, word, str);
                            //
                            // if (isWordFound)
                            // {
                            //     Debug.Log(word + " Word Found !!");
                            //     return true;
                            // }
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