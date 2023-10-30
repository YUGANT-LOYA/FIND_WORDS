using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    public class GameController : MonoBehaviour
    {
        public static GameController instance;

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

        [SerializeField] private DefinedLevelScriptable definedLevelScriptable;
        [SerializeField] private CoinHandlerScriptable coinHandlerScriptable;
        [SerializeField] private GridCamScriptable gridCamScriptable;
        [SerializeField] private MainDictionary mainDictionary;
        [SerializeField] private Transform levelContainer;
        public Transform coinContainerTran;
        public int coinPoolSize = 10, shuffleUsingCoin = 30, dealUsingCoin = 10, hintUsingCoin = 20;
        private Level _currLevel;

        private void Awake()
        {
            CreateSingleton();
        }

        public void MakeInstance()
        {
            Debug.Log("Make Instance Called !!");
            Application.targetFrameRate = 120;
            Vibration.Init();
        }

        private void OnEnable()
        {
            Debug.Log("OnEnable Called !!");
        }
        
        private void CreateSingleton()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            MakeInstance();
            Init();
        }

        void GameStartInfo()
        {
            Debug.Log("First Time Game Start Info Called !!");
            DataHandler.CurrGridSize = startingGridSize;
            UIManager.instance.iqLevelText.text = $"IQ : {DataHandler.IqLevel.ToString()}";
            LevelHandler.instance.SaveSystem();
        }

        private void Init()
        {
            if (DataHandler.FirstTimeGameOpen == 0)
            {
                DataHandler.FirstTimeGameOpen = 1;
                GameStartInfo();
            }

            Debug.Log("Loading Dict !!");
            LevelHandler.instance.englishDictWords.UpdateFullEnglishDict();
            LevelHandler.instance.englishDictWords.UpdateFilteredEnglishDict();
            Debug.Log("Dict Loaded !!");

            Debug.Log("Save Manager (Word Left List) Count : " + SaveManager.Instance.state.wordLeftList.Count);
            Debug.Log("Save Manager (Hint List) Count : " + SaveManager.Instance.state.hintList.Count);
            Debug.Log("Save Manager (Grid On Screen List) Count : " +
                      SaveManager.Instance.state.gridOnScreenList.Count);
            Debug.Log("Save Manager (Grid Data List) Count : " + SaveManager.Instance.state.gridDataList.Count);

            StartGame();
            Debug.Log("Game Created !");
        }

        public IEnumerator StartGameAfterCertainTime(float time)
        {
            Debug.Log($"Game Restarting After {time}");

            DataHandler.CurrTotalQuesSize = DataHandler.CurrGridSize;
            DataHandler.UnlockedQuesLetter = DataHandler.CurrTotalQuesSize;
            DataHandler.CurrGridSize++;
            Debug.Log("Curr Grid Size : " + DataHandler.CurrGridSize);
            DataHandler.UnlockGridIndex = 0;
            DataHandler.NewGridCreated = 0;
            DataHandler.PickDataIndex = 0;
            UIManager.instance.coinText.text = DataHandler.TotalCoin.ToString();
            
            yield return new WaitForSeconds(time);

            StartGame();
            _currLevel.GridPlacement();
        }

        private void StartGame()
        {
            UIManager.instance.coinText.text = DataHandler.TotalCoin.ToString();
            LevelHandler.instance.ClearAllLists();
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
            LevelHandler.instance.AssignLevel(_currLevel);
            _currLevel.gridSize = new Vector2Int(DataHandler.CurrGridSize, DataHandler.CurrGridSize);
            LevelHandler.instance.AssignGridData();
            _currLevel.StartInit();
        }

        public GridCamScriptable GetGridCamScriptable()
        {
            return gridCamScriptable;
        }

        public DefinedLevelScriptable GetDefinedLevelScriptable()
        {
            return definedLevelScriptable;
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

        public void ShuffleGrid(bool isCalledByPlayer = true)
        {
            if (!LevelHandler.instance.GetLevelRunningBool())
                return;

            if (isCalledByPlayer)
            {
                if (DataHandler.TotalCoin < shuffleUsingCoin)
                    return;

                UIManager.SetCoinData(shuffleUsingCoin, -1);
                StartCoroutine(UIManager.instance.UpdateReducedCoinText(0f, shuffleUsingCoin, 0.5f));
            }

            LevelHandler.instance.SetLevelRunningBool(false);
            SoundManager.instance.PlaySound(SoundManager.SoundType.ClickSound);
            LevelHandler.instance.ClearInGameList();
            float time = _currLevel.timeToWaitForEachGrid;
            float timeToPlaceGrids = _currLevel.timeToPlaceGrid;
            List<GridTile> list = new List<GridTile>(LevelHandler.instance.unlockedGridList);
            //ShuffleList(list);
            StartCoroutine(LevelHandler.instance.ReturnToDeck(list, time, timeToPlaceGrids));
            //LevelHandler.instance.RevertQuesData();
        }

        public void Deal(bool isCalledByPlayer = true)
        {
            if (!LevelHandler.instance.GetLevelRunningBool() || LevelHandler.instance.wordCompletedGridList.Count <= 0)
                return;

            if (isCalledByPlayer)
            {
                if (DataHandler.TotalCoin < dealUsingCoin)
                    return;

                UIManager.SetCoinData(dealUsingCoin, -1);
                StartCoroutine(UIManager.instance.UpdateReducedCoinText(0f, dealUsingCoin, 0.5f));
            }

            LevelHandler.instance.SetLevelRunningBool(false);
            SoundManager.instance.PlaySound(SoundManager.SoundType.ClickSound);
            List<GridTile> list = new List<GridTile>(LevelHandler.instance.wordCompletedGridList);
            ShuffleList(list);
            StartCoroutine(BackToDeckAnim(list));

            UIManager.instance.DealButtonEffect();
        }

        public void Hint(bool isCalledByPlayer = true)
        {
            if (!LevelHandler.instance.GetLevelRunningBool())
                return;

            //LevelHandler.instance.SetLevelRunningBool(false);

            if (isCalledByPlayer)
            {
                if (DataHandler.TotalCoin < hintUsingCoin)
                {
                    UIManager.instance.toastMessageScript.ShowHintToast();
                    return;
                }

                StartCoroutine(UIManager.instance.UpdateReducedCoinText(0f, hintUsingCoin, 0.5f));
            }

            SoundManager.instance.PlaySound(SoundManager.SoundType.ClickSound);
            LevelHandler.instance.ShowHint();
        }

        IEnumerator BackToDeckAnim(List<GridTile> list)
        {
            string randomPickedWord = "";
            List<GridTile> tempList = new List<GridTile>(LevelHandler.instance.gridAvailableOnScreenList);
            int total = list.Count;
            string remainingLetter = "";
            List<string> hintFitStringList = new List<string>();

            while (total > 0)
            {
                Debug.Log("Forming Word From Screen Letters");
                string tempStr =
                    LevelHandler.instance.PickRandomWordFormingLetters(tempList, 2, out string gridExistingString);

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
                    randomPickedWord = LevelHandler.instance.GetDataAccordingToGrid(list.Count);
                    total -= randomPickedWord.Length;
                }

                total -= remainingLetter.Length;
                Debug.Log("Total : " + total);
                //Debug.Log("RANDOM Pick Letter : " + randomPickedWord);
            }

            foreach (string s in hintFitStringList)
            {
                LevelHandler.instance.hintAvailList.Add(s);
                Debug.Log("Hint Added : " + s);
            }

            Debug.Log("Remaining Letter : " + remainingLetter);
            Debug.Log("RANDOM Pick Letter : " + randomPickedWord);
            //Debug.Log("Temp STR Count : " + tempList.Count);
            //Debug.Log("RANDOM PICK VAL : " + randomPickedWord);

            int index = 0;

            SoundManager.instance.PlaySound(SoundManager.SoundType.CardDeckSound);

            foreach (GridTile gridTile in list)
            {
                yield return new WaitForSeconds(_currLevel.timeToWaitForEachGrid);
                Debug.Log("Moving To Grid Place Again !");
                gridTile.isBlastAfterWordComplete = false;
                gridTile.GridTextData = randomPickedWord[index].ToString();
                LevelHandler.instance.gridAvailableOnScreenList.Add(gridTile);
                gridTile.MoveTowardsGrid();
                LevelHandler.instance.wordCompletedGridList.Remove(gridTile);
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
            LevelHandler.instance.LastQuesTile = null;
            LevelHandler.instance.SetLevelRunningBool(true);
            bool isHintAvail = LevelHandler.instance.CheckHintStatus(out string finalStr);
            UIManager.instance.HintStatus(isHintAvail);
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

        private void OnApplicationPause(bool isGameActive)
        {
            isGameActive = !isGameActive;

            if (isGameActive) return;

            Debug.Log("Game Pause Called !");
            LevelHandler.instance.SaveSystem();
            Debug.Log("Game Pause (Word Left List) Count : " + SaveManager.Instance.state.wordLeftList.Count);
            Debug.Log("Game Pause (Hint List) Count : " + SaveManager.Instance.state.hintList.Count);
            Debug.Log("Game Pause (Grid On Screen List) Count : " +
                      SaveManager.Instance.state.gridOnScreenList.Count);
            Debug.Log("Game Pause (Grid Data List) Count : " + SaveManager.Instance.state.gridDataList.Count);
        }

        private void OnApplicationQuit()
        {
            Debug.Log("Application Quit Called !");
            LevelHandler.instance.SaveSystem();
            Debug.Log("Game Quit (Word Left List) Count : " + SaveManager.Instance.state.wordLeftList.Count);
            Debug.Log("Game Quit (Hint List) Count : " + SaveManager.Instance.state.hintList.Count);
            Debug.Log("Game Quit (Grid On Screen List) Count : " +
                      SaveManager.Instance.state.gridOnScreenList.Count);
            Debug.Log("Game Quit (Grid Data List) Count : " + SaveManager.Instance.state.gridDataList.Count);
        }
    }
}