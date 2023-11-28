using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance;
        public static int LevelAttempts;
        private static readonly char[] Vowels = { 'a', 'e', 'i', 'o', 'u' };

        private static readonly char[] Consonants =
        {
            'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n',
            'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z'
        };

        [Range(5, 7)] public int startingGridSize = 5;
        [HideInInspector] public int startingQuesSize = 3, maxQuesSize = 6, maxGridSize = 7;
        public int defaultIq = 1, changeBgAfter = 3;

        [Header("References")] [SerializeField]
        private PickWordDataInfo pickWordDataInfo;

        public ParticleSystem confettiParticleSystem;
        public WordCompleteComments wordCommentScript;
        public HelperScript helper;
        [SerializeField] private DefinedLevelScriptable definedLevelScriptable;
        [SerializeField] private CoinHandlerScriptable coinHandlerScriptable;
        [SerializeField] private MainDictionary mainDictionary;
        [SerializeField] private Transform levelContainer;
        public Transform coinContainerTran;
        public int coinPoolSize = 10, shuffleUsingCoin = 30, dealUsingCoin = 10, hintUsingCoin = 20;
        private Level _currLevel;
        private bool _isDealHelper, _isShuffleHelper;

        private void Awake()
        {
            CreateSingleton();
        }

        private void MakeInstance()
        {
            Application.targetFrameRate = 120;
            Vibration.Init();
        }

        private void CreateSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            MakeInstance();
            DataHandler.Instance.SetBg();
            Init();
        }


        void GameStartInfo()
        {
            //Debug.Log("First Time Game Start Info Called !!");
            DataHandler.CurrGridSize = startingGridSize;
            DataHandler.CurrTotalQuesSize = DataHandler.UnlockedQuesLetter + 1;
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

            UIManager.Instance.iqExperienceText.text = $"{DataHandler.IqExpLevel.ToString()}";
            UIManager.Instance.iqSlider.value = DataHandler.IqBarVal;
            //Debug.Log("Loading Dict !!");
            LevelHandler.Instance.englishDictWords.UpdateFullEnglishDict();
            LevelHandler.Instance.englishDictWords.UpdateFilteredEnglishDict();
            //Debug.Log("Dict Loaded !!");

            StartGame();
            //Debug.Log("Game Created !");

            LionStudiosManager.LevelStart(DataHandler.LevelNum, LevelAttempts, 0);
            GAScript.LevelStart(DataHandler.LevelNum.ToString());
        }

        public IEnumerator StartGameAfterCertainTime(float time)
        {
            //Debug.Log($"Game Restarting After {time}");
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

            yield return new WaitForSeconds(2 * time / 4);

            UIManager.Instance.SmokeTransition(true);

            yield return new WaitForSeconds(time / 4);

            StartGame();
            Debug.Log("Start Game Function Completed !!");
            _currLevel.GridPlacement();
        }

        private void StartGame()
        {
            UIManager.Instance.coinText.text = DataHandler.TotalCoin.ToString();
            LevelHandler.Instance.ClearAllLists();
            LevelHandler.Instance.noHintExist = false;
            ClearContainer(levelContainer);
            CreateLevel();
        }

        void CreateLevel()
        {
            GameObject level = Instantiate(DataHandler.Instance.levelPrefab, levelContainer);
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

        public DefinedLevelScriptable GetDefinedLevelScriptable()
        {
            return definedLevelScriptable;
        }

        public PickWordDataInfo GetPickDataInfo()
        {
            return pickWordDataInfo;
        }

        public MainDictionary GetMainDictionary()
        {
            return mainDictionary;
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

            LevelHandler.Instance.SetLevelRunningBool(false);
            
            if (isCalledByPlayer)
            {
               
                if (DataHandler.TotalCoin < shuffleUsingCoin)
                {
                    UIManager.Instance.toastMessageScript.ShowNotEnoughCoinsToast();
                    SoundManager.instance.PlaySound(SoundManager.SoundType.ErrorMessage);
                    LevelHandler.Instance.SetLevelRunningBool();
                    return;
                }

                //Debug.Log("Shuffle Called !!");
                Vibration.Vibrate(20);
                LevelHandler.Instance.quesHintStr = "";
                UIManager.SetCoinData(shuffleUsingCoin, -1);
                StartCoroutine(UIManager.Instance.UpdateReducedCoinText(0f, shuffleUsingCoin));
                UIManager.Instance.ShuffleButtonClicked();
            }


            SoundManager.instance.PlaySound(SoundManager.SoundType.Click);
            float time = _currLevel.timeToWaitForEachGrid;
            float timeToPlaceGrids = _currLevel.timeToPlaceGrid;
            List<GridTile> list = new List<GridTile>(LevelHandler.Instance.unlockedGridList);
            //ShuffleList(list);
            LevelHandler.Instance.DisableWordCompleteGameObject();
            LevelHandler.Instance.ClearInGameList();
            StartCoroutine(LevelHandler.Instance.ReturnToDeck(list, time, timeToPlaceGrids));
        }
        
        public void Deal(bool isCalledByPlayer = true)
        {
            if (!LevelHandler.Instance.GetLevelRunningBool())
                return;
            
            LevelHandler.Instance.SetLevelRunningBool(false);

            if (DataHandler.HelperLevelCompleted == 0)
            {
                //Debug.Log("Deal HelperLevelCompleted is Not Completed!");
                if (DataHandler.HelperIndex != helper.clickDealIndex)
                {
                    //Debug.Log("IF Helper Index Same in Deal And Function Returns !");
                    LevelHandler.Instance.SetLevelRunningBool();
                    return;
                }
                else
                {
                    //Debug.Log("ELSE Helper Index Same in Deal And Function Returns !");
                    UIManager.Instance.DealButtonEffect();
                    SoundManager.instance.PlaySound(SoundManager.SoundType.Click);
                    StartCoroutine(HelperDeal());
                    LevelHandler.Instance.SetLevelRunningBool();
                    return;
                }
            }

            if (isCalledByPlayer)
            {
                if (DataHandler.TotalCoin < dealUsingCoin)
                {
                    SoundManager.instance.PlaySound(SoundManager.SoundType.ErrorMessage);
                    UIManager.Instance.toastMessageScript.ShowNotEnoughCoinsToast();
                    LevelHandler.Instance.SetLevelRunningBool();
                    return;
                }

                if (LevelHandler.Instance.wordCompletedGridList.Count <= 0)
                {
                    SoundManager.instance.PlaySound(SoundManager.SoundType.ErrorMessage);
                    UIManager.Instance.toastMessageScript.ShowNoDealFoundToast();
                    LevelHandler.Instance.SetLevelRunningBool();
                    return;
                }

                //Debug.Log("Deal Called !!");
                Vibration.Vibrate(20);
                UIManager.SetCoinData(dealUsingCoin, -1);
                StartCoroutine(UIManager.Instance.UpdateReducedCoinText(0f, dealUsingCoin));
                UIManager.Instance.DealButtonEffect();
            }

            SoundManager.instance.PlaySound(SoundManager.SoundType.Click);
            List<GridTile> list = new List<GridTile>(LevelHandler.Instance.wordCompletedGridList);
            //ShuffleList(list);
            StartCoroutine(BackToDeckAnim(list));
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
                LevelHandler.AddGridToList(LevelHandler.Instance.gridAvailableOnScreenList, gridTile);
                gridTile.MoveTowardsGrid();
                index++;
            }

            helper.canvasHelperHand.SetActive(false);
            //Debug.Log("Helper Deal For Each Completed !");
            LevelHandler.Instance.wordCompletedGridList.Clear();
            DataHandler.HelperIndex++;
            helper.ClickTile(1f);
        }

        public void Hint(bool isCalledByPlayer = true)
        {
            if (!LevelHandler.Instance.GetLevelRunningBool() ||
                LevelHandler.Instance.quesHintStr.Length == DataHandler.UnlockedQuesLetter)
                return;

            if (DataHandler.HelperLevelCompleted == 0)
            {
                //Debug.Log("Hint HelperLevelCompleted is Not Completed!");
                return;
            }

            bool isHintAvail = false;

            if (isCalledByPlayer)
            {
                if (DataHandler.TotalCoin < hintUsingCoin)
                {
                    SoundManager.instance.PlaySound(SoundManager.SoundType.ErrorMessage);
                    UIManager.Instance.toastMessageScript.ShowNotEnoughCoinsToast();
                    return;
                }


                isHintAvail = LevelHandler.Instance.CheckWordExistOrNot(out bool hintButtonStatus, out string hintStr);

                LevelHandler.Instance.quesHintStr = hintStr;

                if (!isHintAvail)
                {
                    SoundManager.instance.PlaySound(SoundManager.SoundType.ErrorMessage);
                    UIManager.Instance.toastMessageScript.ShowNoWordFoundToast();
                    return;
                }

                Debug.Log("Hint Called !!");
                UIManager.Instance.HintButtonClicked();
                //UIManager.instance.HintStatus(false);
                Vibration.Vibrate(20);
                StartCoroutine(UIManager.Instance.UpdateReducedCoinText(0f, hintUsingCoin));
            }

            SoundManager.instance.PlaySound(SoundManager.SoundType.Click);

            LevelHandler.Instance.ShowHint(isHintAvail);
        }

        IEnumerator BackToDeckAnim(List<GridTile> wordCompleteList)
        {
            StringBuilder randomPickedWord = new StringBuilder("");
            List<GridTile> tempList = new List<GridTile>(LevelHandler.Instance.gridAvailableOnScreenList);
            int total = wordCompleteList.Count;
            string remainingLetter = "";
            List<string> hintFitStringList = new List<string>();

            StringBuilder vowels = new StringBuilder("");
            string gridString = LevelHandler.Instance.GetStringOfAllAvailableGrids();
            int vowelCount = 0;
            bool isVowelLess = false;
            
            foreach (char c in gridString)
            {
                if (Vowels.Contains(c))
                {
                    vowelCount++;
                }
            }

            //Grid available are greater than or equal to 2/3 of total Grids, so check vowels and if vowels are less than give deal of vowels.
            if (gridString.Length >= 2 * LevelHandler.Instance.unlockedGridList.Count / 3)
            {
                //If There are less vowels and whole grid string contains consonents, so it is better to provide vowels rather than forming and giving letters of another words.
                if (vowelCount < LevelHandler.Instance.gridAvailableOnScreenList.Count / 3)
                {
                    Debug.Log("LESS VOWELS CALLED !!");
                    for (int i = 0; i < total; i++)
                    {
                        vowels.Append(RandomVowel());
                    }

                    randomPickedWord = vowels;
                    isVowelLess = true;
                }
                else
                {
                    Debug.Log("MORE VOWELS CALLED !!");
                }
            }
            
            
            if(!isVowelLess)
            {
                Debug.Log("NORMAL DEAL CALLED !!");
                while (total > 0)
                {
                    //Debug.Log("Forming Word From Screen Letters");

                    string tempStr = LevelHandler.Instance.PickRandomWordFormingLetters(tempList, 2,
                        out string gridExistingString);

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

                        hintFitStringList.Add(tempStr);
                        randomPickedWord.Append(remainingLetter.Trim());
                    }
                    else
                    {
                        hintFitStringList.Clear();
                        //Debug.Log("Printing Data From Word Left List ");
                        string str = LevelHandler.Instance.GetDataAccordingToGrid(wordCompleteList.Count);
                        randomPickedWord = new StringBuilder(str);
                        total -= randomPickedWord.Length;
                    }

                    total -= remainingLetter.Length;
                }

                //Debug.Log("Temp Word : " + tempStr);


                //Debug.Log("Total : " + total);
                //Debug.Log("RANDOM Pick Letter : " + randomPickedWord);

                if (hintFitStringList.Count > 0)
                {
                    foreach (string s in hintFitStringList)
                    {
                        LevelHandler.Instance.hintAvailList.Add(s);
                        //Debug.Log("Hint Added : " + s);
                    }
                }
            }

            //Debug.Log("Remaining Letter : " + remainingLetter);
            //Debug.Log("RANDOM Pick Letter : " + randomPickedWord);
            //Debug.Log("Temp STR Count : " + tempList.Count);
            //Debug.Log("RANDOM PICK VAL : " + randomPickedWord);

            int index = 0;

            SoundManager.instance.PlaySound(SoundManager.SoundType.CardDeck);

            StartCoroutine(ResetLevelHandlerData(_currLevel.timeToWaitForEachGrid * wordCompleteList.Count));

            foreach (GridTile gridTile in wordCompleteList)
            {
                yield return new WaitForSeconds(_currLevel.timeToWaitForEachGrid);
                //Debug.Log("Moving To Grid Place Again !");
                gridTile.isBlastAfterWordComplete = false;
                gridTile.GridTextData = randomPickedWord[index].ToString();
                LevelHandler.AddGridToList(LevelHandler.Instance.gridAvailableOnScreenList, gridTile);
                gridTile.MoveTowardsGrid();
                LevelHandler.RemoveGridFromList(LevelHandler.Instance.wordCompletedGridList, gridTile);
                index++;
            }
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
            LevelHandler.Instance.AfterBackToDeckAnim();
        }

        public MainDictionary.WordLengthDetailedInfo GetWordListOfLength(int wordLength,
            string startingLetter)
        {
            MainDictionary.WordLengthDetailedInfo
                wordList = new MainDictionary.WordLengthDetailedInfo();

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
                            wordList = wordInfoList[j];
                            //Debug.Log("Word List Count : " + wordList.Count);
                            return wordList;
                        }
                    }
                }
            }

            return wordList;
        }

        private void OnApplicationPause(bool isGameActive)
        {
            isGameActive = !isGameActive;

            if (isGameActive) return;

            LevelHandler.Instance.SaveSystem();
        }

        private void OnApplicationQuit()
        {
            LevelHandler.Instance.SaveSystem();
        }
    }
}