using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using YugantLoyaLibrary.FindWord;

namespace YugantLoyaLibrary.FindWords
{
    public class GameController : MonoBehaviour
    {
        public static GameController instance;
        public static int LevelAttempts;
        private static readonly char[] Vowels = { 'a', 'e', 'i', 'o', 'u' };

        private static readonly char[] Consonants =
        {
            'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n',
            'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z'
        };

        [Range(4, 7)] public int startingGridSize = 5;
        [HideInInspector] public int startingQuesSize = 3, maxGridSize = 7;
        public int defaultIq = 1, changeBgAfter = 3;

        [Header("References")] [SerializeField]
        private PickWordDataInfo pickWordDataInfo;

        public WordCompleteComments wordCommentScript;
        public HelperScript helper;
        [SerializeField] private DefinedLevelScriptable definedLevelScriptable;
        [SerializeField] private CoinHandlerScriptable coinHandlerScriptable;
        [SerializeField] private GridCamScriptable gridCamScriptable;
        [SerializeField] private MainDictionary mainDictionary;
        [SerializeField] private Transform levelContainer;
        public Transform coinContainerTran;
        public int coinPoolSize = 10, shuffleUsingCoin = 30, dealUsingCoin = 10, hintUsingCoin = 20;
        private Level _currLevel;
        private bool isDealHelper, isShuffleHelper;

        private void Awake()
        {
            CreateSingleton();
        }

        public void MakeInstance()
        {
            //Debug.Log("Make Instance Called !!");
            Application.targetFrameRate = 120;
            Vibration.Init();
        }

        private void OnEnable()
        {
            //Debug.Log("OnEnable Called !!");
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
            DataHandler.instance.SetBg();
            Init();
        }


        void GameStartInfo()
        {
            //Debug.Log("First Time Game Start Info Called !!");
            DataHandler.CurrGridSize = startingGridSize;
            UIManager.Instance.iqExperienceText.text = $" {DataHandler.IqExpLevel.ToString()}";
            LevelHandler.Instance.SaveSystem();
        }

        private void Init()
        {
            if (DataHandler.FirstTimeGameOpen == 0)
            {
                DataHandler.FirstTimeGameOpen = 1;
                GameStartInfo();
            }
            
            UIManager.Instance.iqExperienceText.text = $" {DataHandler.IqExpLevel.ToString()}";
            //Debug.Log("Loading Dict !!");
            LevelHandler.Instance.englishDictWords.UpdateFullEnglishDict();
            LevelHandler.Instance.englishDictWords.UpdateFilteredEnglishDict();
            //Debug.Log("Dict Loaded !!");

            //Debug.Log("Save Manager (Word Left List) Count : " + SaveManager.Instance.state.wordLeftList.Count);
            //Debug.Log("Save Manager (Hint List) Count : " + SaveManager.Instance.state.hintList.Count);
            // Debug.Log("Save Manager (Grid On Screen List) Count : " +
            //           SaveManager.Instance.state.gridOnScreenList.Count);
            //Debug.Log("Save Manager (Grid Data List) Count : " + SaveManager.Instance.state.gridDataList.Count);

            StartGame();
            //Debug.Log("Game Created !");
            
            LionStudiosManager.LevelStart(DataHandler.LevelNum, LevelAttempts, 0);
            GAScript.LevelStart(DataHandler.LevelNum.ToString());
        }

        public IEnumerator StartGameAfterCertainTime(float time)
        {
            //Debug.Log($"Game Restarting After {time}");

            DataHandler.CurrTotalQuesSize = DataHandler.CurrGridSize;
            DataHandler.UnlockedQuesLetter = DataHandler.CurrTotalQuesSize;
            DataHandler.CurrGridSize++;
            //Debug.Log("Curr Grid Size : " + DataHandler.CurrGridSize);
            DataHandler.UnlockGridIndex = 0;
            DataHandler.NewGridCreated = 0;
            DataHandler.PickDataIndex = 0;
            UIManager.Instance.coinText.text = DataHandler.TotalCoin.ToString();

            foreach (GridTile tile in LevelHandler.Instance.totalGridsList)
            {
                tile.ResetWholeData();
            }

            LevelHandler.Instance.ClearInGameList();

            yield return new WaitForSeconds(time);

            StartGame();
            _currLevel.GridPlacement();
        }

        private void StartGame()
        {
            UIManager.Instance.coinText.text = DataHandler.TotalCoin.ToString();
            LevelHandler.Instance.ClearAllLists();
            ClearContainer(levelContainer);
            CreateLevel();
        }

        void CreateLevel()
        {
            GameObject level = Instantiate(DataHandler.instance.levelPrefab, levelContainer);
            _currLevel = level.GetComponent<Level>();
            AssignLevelData();
            _currLevel.StartInit();
        }

        private void AssignLevelData()
        {
            LevelHandler.Instance.AssignLevel(_currLevel);
            _currLevel.gridSize = new Vector2Int(DataHandler.CurrGridSize, DataHandler.CurrGridSize);
            LevelHandler.Instance.AssignGridData();
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
            if (!LevelHandler.Instance.GetLevelRunningBool())
                return;

            if (DataHandler.HelperLevelCompleted == 0)
            {
                //Debug.Log("Shuffle HelperLevelCompleted is Not Completed!");
                return;
            }

            if (isCalledByPlayer)
            {
                if (DataHandler.TotalCoin < shuffleUsingCoin)
                {
                    UIManager.Instance.toastMessageScript.ShowNoShuffleFoundToast();
                    SoundManager.instance.PlaySound(SoundManager.SoundType.ErrorMessage);
                    return;
                }


                Vibration.Vibrate(20);
                LevelHandler.Instance.quesHintStr = null;
                UIManager.SetCoinData(shuffleUsingCoin, -1);
                StartCoroutine(UIManager.Instance.UpdateReducedCoinText(0f, shuffleUsingCoin, 0.5f));
            }

            LevelHandler.Instance.SetLevelRunningBool(false);
            SoundManager.instance.PlaySound(SoundManager.SoundType.Click);
            UIManager.Instance.ShuffleButtonClicked();
            float time = _currLevel.timeToWaitForEachGrid;
            float timeToPlaceGrids = _currLevel.timeToPlaceGrid;
            List<GridTile> list = new List<GridTile>(LevelHandler.Instance.unlockedGridList);
            //ShuffleList(list);
            LevelHandler.Instance.DisableWordCompleteGameObject();
            StartCoroutine(LevelHandler.Instance.ReturnToDeck(list, time, timeToPlaceGrids));
            LevelHandler.Instance.ClearInGameList();
        }

        public void Deal(bool isCalledByPlayer = true)
        {
            if (!LevelHandler.Instance.GetLevelRunningBool())
                return;

            if (DataHandler.HelperLevelCompleted == 0)
            {
                //Debug.Log("Deal HelperLevelCompleted is Not Completed!");
                if (DataHandler.HelperIndex != helper.clickDealIndex)
                {
                    //Debug.Log("IF Helper Index Same in Deal And Function Returns !");
                    return;
                }
                else
                {
                    //Debug.Log("ELSE Helper Index Same in Deal And Function Returns !");
                    UIManager.Instance.DealButtonEffect();
                    SoundManager.instance.PlaySound(SoundManager.SoundType.Click);
                    StartCoroutine(HelperDeal());
                    return;
                }
            }

            if (isCalledByPlayer)
            {
                if (DataHandler.TotalCoin < dealUsingCoin || LevelHandler.Instance.wordCompletedGridList.Count <= 0)
                {
                    SoundManager.instance.PlaySound(SoundManager.SoundType.ErrorMessage);
                    UIManager.Instance.toastMessageScript.ShowNoDealFoundToast();
                    return;
                }

                Vibration.Vibrate(20);
                UIManager.SetCoinData(dealUsingCoin, -1);
                StartCoroutine(UIManager.Instance.UpdateReducedCoinText(0f, dealUsingCoin, 0.5f));
            }

            LevelHandler.Instance.SetLevelRunningBool(false);
            SoundManager.instance.PlaySound(SoundManager.SoundType.Click);
            List<GridTile> list = new List<GridTile>(LevelHandler.Instance.wordCompletedGridList);
            ShuffleList(list);
            StartCoroutine(BackToDeckAnim(list));

            UIManager.Instance.DealButtonEffect();
        }

        IEnumerator HelperDeal()
        {
            //Debug.Log("Helper Deal Called !");
            int index = 0;
            string randomPickedWord = "YOB";
            foreach (GridTile gridTile in LevelHandler.Instance.wordCompletedGridList)
            {
                yield return new WaitForSeconds(_currLevel.timeToWaitForEachGrid);
                gridTile.isBlastAfterWordComplete = false;
                gridTile.GridTextData = randomPickedWord[index].ToString();
                LevelHandler.Instance.gridAvailableOnScreenList.Add(gridTile);
                gridTile.MoveTowardsGrid();
                index++;
            }

            helper.canvasHelperHand.SetActive(false);
            //Debug.Log("Helper Deal For Each Completed !");
            LevelHandler.Instance.wordCompletedGridList.Clear();
            DataHandler.HelperIndex++;
            helper.ClickTile(0.6f);
        }

        public void Hint(bool isCalledByPlayer = true)
        {
            if (!LevelHandler.Instance.GetLevelRunningBool() ||
                !string.IsNullOrEmpty(LevelHandler.Instance.quesHintStr))
                return;

            if (DataHandler.HelperLevelCompleted == 0)
            {
                //Debug.Log("Hint HelperLevelCompleted is Not Completed!");
                return;
            }

            if (isCalledByPlayer)
            {
                if (DataHandler.TotalCoin < hintUsingCoin)
                {
                    SoundManager.instance.PlaySound(SoundManager.SoundType.ErrorMessage);
                    UIManager.Instance.toastMessageScript.ShowHintToast();
                    return;
                }

                bool isHintAvail =
                    LevelHandler.Instance.CheckWordExistOrNot(out bool hintButtonStatus, out string hintStr);

                LevelHandler.Instance.quesHintStr = hintStr;

                if (!isHintAvail)
                {
                    SoundManager.instance.PlaySound(SoundManager.SoundType.ErrorMessage);
                    UIManager.Instance.toastMessageScript.ShowNoWordFoundToast();
                    return;
                }

                UIManager.Instance.HintButtonClicked();
                //UIManager.instance.HintStatus(false);
                Vibration.Vibrate(20);
                StartCoroutine(UIManager.Instance.UpdateReducedCoinText(0f, hintUsingCoin, 0.5f));
            }

            SoundManager.instance.PlaySound(SoundManager.SoundType.Click);

            LevelHandler.Instance.ShowHint();
        }

        IEnumerator BackToDeckAnim(List<GridTile> list)
        {
            string randomPickedWord = "";
            List<GridTile> tempList = new List<GridTile>(LevelHandler.Instance.gridAvailableOnScreenList);
            int total = list.Count;
            string remainingLetter = "";
            List<string> hintFitStringList = new List<string>();

            while (total > 0)
            {
                //Debug.Log("Forming Word From Screen Letters");
                string tempStr =
                    LevelHandler.Instance.PickRandomWordFormingLetters(tempList, 2, out string gridExistingString);

                Debug.Log("Temp Word : " + tempStr);

                if (tempStr.Length > 0)
                {
                    remainingLetter = tempStr;

                    foreach (char repeatedChar in gridExistingString)
                    {
                        //Debug.Log("Repeated Char" + repeatedChar);
                        remainingLetter = RemoveCharFromString(remainingLetter, repeatedChar);
                        //Debug.Log("Remaining Letter : " + remainingLetter);


                        foreach (GridTile tile in tempList)
                        {
                            if (tile.GridTextData != repeatedChar.ToString()) continue;

                            //Debug.Log("Removed Tile : " + tile.name);
                            tempList.Remove(tile);
                            break;
                        }
                    }

                    //Debug.Log("Temp List Count : " + tempList.Count);

                    // foreach (GridTile tile in tempList)
                    // {
                    //     Debug.Log("Temp Element : " + tile);
                    // }

                    hintFitStringList.Add(tempStr);
                    randomPickedWord += remainingLetter.Trim();
                }
                else
                {
                    hintFitStringList.Clear();
                    //Debug.Log("Printing Data From Word Left List ");
                    randomPickedWord = LevelHandler.Instance.GetDataAccordingToGrid(list.Count);
                    total -= randomPickedWord.Length;
                }

                total -= remainingLetter.Length;
                //Debug.Log("Total : " + total);
                //Debug.Log("RANDOM Pick Letter : " + randomPickedWord);
            }

            foreach (string s in hintFitStringList)
            {
                LevelHandler.Instance.hintAvailList.Add(s);
                //Debug.Log("Hint Added : " + s);
            }

            //Debug.Log("Remaining Letter : " + remainingLetter);
            //Debug.Log("RANDOM Pick Letter : " + randomPickedWord);
            //Debug.Log("Temp STR Count : " + tempList.Count);
            //Debug.Log("RANDOM PICK VAL : " + randomPickedWord);

            int index = 0;

            SoundManager.instance.PlaySound(SoundManager.SoundType.CardDeck);

            foreach (GridTile gridTile in list)
            {
                yield return new WaitForSeconds(_currLevel.timeToWaitForEachGrid);
                //Debug.Log("Moving To Grid Place Again !");
                gridTile.isBlastAfterWordComplete = false;
                gridTile.GridTextData = randomPickedWord[index].ToString();
                LevelHandler.Instance.gridAvailableOnScreenList.Add(gridTile);
                gridTile.MoveTowardsGrid();
                LevelHandler.Instance.wordCompletedGridList.Remove(gridTile);
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
            LevelHandler.Instance.LastQuesTile = null;
            LevelHandler.Instance.SetLevelRunningBool(true);
            LevelHandler.Instance.CheckWordExistOrNot(out bool hintButtonStatus, out string hintStr);
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
            //Debug.Log("DictInfo List Count : " + dictInfoList.Count);
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
                        //Debug.Log("First Char of Word : " + word[0]);
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
                //Debug.Log($"{word} Word Same !! ");

                if (word.Length - 1 == index)
                {
                    //Debug.Log($"{word} Word Detected !! ");
                    return true;
                }

                index++;
                return IsWordSame(index, word, textWord);
            }

            //Debug.Log($"Letter Not Same {word} , {textWord} , {index} ");
            return false;
        }

        private void OnApplicationPause(bool isGameActive)
        {
            isGameActive = !isGameActive;

            if (isGameActive) return;

            //Debug.Log("Game Pause Called !");
            LevelHandler.Instance.SaveSystem();
            //Debug.Log("Game Pause (Word Left List) Count : " + SaveManager.Instance.state.wordLeftList.Count);
            //Debug.Log("Game Pause (Hint List) Count : " + SaveManager.Instance.state.hintList.Count);
            // Debug.Log("Game Pause (Grid On Screen List) Count : " +
            //           SaveManager.Instance.state.gridOnScreenList.Count);
            //Debug.Log("Game Pause (Grid Data List) Count : " + SaveManager.Instance.state.gridDataList.Count);
        }

        private void OnApplicationQuit()
        {
            //Debug.Log("Application Quit Called !");
            LevelHandler.Instance.SaveSystem();
            //Debug.Log("Game Quit (Word Left List) Count : " + SaveManager.Instance.state.wordLeftList.Count);
            //Debug.Log("Game Quit (Hint List) Count : " + SaveManager.Instance.state.hintList.Count);
            // Debug.Log("Game Quit (Grid On Screen List) Count : " +
            //           SaveManager.Instance.state.gridOnScreenList.Count);
            //Debug.Log("Game Quit (Grid Data List) Count : " + SaveManager.Instance.state.gridDataList.Count);
        }
    }
}