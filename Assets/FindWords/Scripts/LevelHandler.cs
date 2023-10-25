using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YugantLibrary.ParkingOrderGame;
using Random = UnityEngine.Random;

namespace YugantLoyaLibrary.FindWords
{
    public class LevelHandler : MonoBehaviour
    {
        public delegate void NewLetterDelegate(GridTile gridTile);

        public NewLetterDelegate onNewLetterAddEvent, onRemoveLetterEvent;

        public delegate void GameCompleteDelegate();

        private GameCompleteDelegate _onGameCompleteEvent;

        public EnglishDictWords englishDictWords;
        private Renderer _touchEffectRenderer;
        [SerializeField] Level currLevel;
        public int coinPerWord = 10;
        [HideInInspector] public int coinToUnlockNextGrid;
        [Header("Level Info")] public char[][] gridData;

        public List<GridTile> totalGridsList = new List<GridTile>(),
            inputGridsList = new List<GridTile>(),
            wordCompletedGridList = new List<GridTile>(),
            unlockedGridList = new List<GridTile>(),
            //This will not remove grids from it after unlocking also, becoz it is needed for unlocking next grid for coins.
            lockedGridList = new List<GridTile>(),
            gridAvailableOnScreenList = new List<GridTile>(),
            totalBuyingGridList = new List<GridTile>();

        public List<QuesTile> quesTileList = new List<QuesTile>();
        public List<string> selectedWords = new List<string>(), wordsLeftList = new List<string>();
        private bool _isLevelRunning = true;
        private int _colorIndex;
        private QuesTile _lastQuesTile;
        public List<string> hintAvailList = new List<string>();

        private void OnEnable()
        {
            onNewLetterAddEvent += AddNewLetter;
            onRemoveLetterEvent += RemoveLetter;
        }

        private void OnDisable()
        {
            onNewLetterAddEvent -= AddNewLetter;
            onRemoveLetterEvent -= RemoveLetter;
        }

        private void Awake()
        {
            Init();
        }

        void Init()
        {
        }

        public void LevelStartInit()
        {
            _isLevelRunning = true;
        }

        public void FillWordLeftListRemainingWord()
        {
            List<PickWordDataInfo.PickingDataInfo> pickInfoList =
                GameController.instance.GetPickDataInfo().pickDataInfoList;

            wordsLeftList.Clear();

            foreach (PickWordDataInfo.PickingDataInfo info in pickInfoList)
            {
                if (info.quesLetterCount == DataHandler.UnlockedQuesLetter)
                {
                    for (var i = 0; i < info.wordList.Count; i++)
                    {
                        if (i >= DataHandler.PickDataIndex)
                        {
                            var s = info.wordList[i];
                            wordsLeftList.Add(s.Trim());
                        }
                    }
                }
            }
        }


        public void FillNewWordInWordLeftList()
        {
            List<PickWordDataInfo.PickingDataInfo> pickInfoList =
                GameController.instance.GetPickDataInfo().pickDataInfoList;

            wordsLeftList.Clear();
            DataHandler.PickDataIndex = 0;

            foreach (PickWordDataInfo.PickingDataInfo info in pickInfoList)
            {
                if (info.quesLetterCount == DataHandler.UnlockedQuesLetter)
                {
                    foreach (string s in info.wordList)
                    {
                        wordsLeftList.Add(s.Trim());
                    }
                }
            }
        }

        public QuesTile LastQuesTile
        {
            get => _lastQuesTile;
            set => _lastQuesTile = value;
        }

        public void AssignGridData()
        {
            // if (DataHandler.CurrDefinedLevel <
            //     GameController.instance.GetDefinedLevelScriptable().definedLevelInfoList.Count)
            // {
            //     string[] line = GameController.instance.GetDefinedLevelScriptable()
            //         .definedLevelInfoList[DataHandler.CurrDefinedLevel].gridData.Split('\n');
            //
            //     int x = 0, y = 0;
            //
            //     char[][] arrData = new char[line.Length][];
            //
            //     foreach (string str in line)
            //     {
            //         char[] charArr = str.ToCharArray();
            //         arrData[x] = new char[charArr.Length];
            //         y = 0;
            //         foreach (char c in charArr)
            //         {
            //             Debug.Log("C : " + c);
            //             arrData[x][y] = c;
            //             y++;
            //         }
            //
            //         x++;
            //     }
            //
            //     gridData = arrData;
            // }
            // else
            // {
            if (DataHandler.NewGridCreated == 0)
            {
                DataHandler.NewGridCreated = 1;
                Debug.Log("New Data Created !!");
                FillNewWordInWordLeftList();
                GetNewGridDataCreated();
            }
            else
            {
                Debug.Log("Loaded Previous Data !!");
                LoadWordLeftList();
                ReadAlreadyCreatedGrid();
            }
            //}
        }

        private void ReadAlreadyCreatedGrid()
        {
            Debug.Log("Read Already Created Grid Called !!");
            List<char> readCharList = SaveManager.Instance.state.gridDataList;
            int index = 0;
            gridData = new char[DataHandler.CurrGridSize][];
            for (var i = 0; i < gridData.Length; i++)
            {
                gridData[i] = new char[DataHandler.CurrGridSize];

                for (var j = 0; j < gridData[i].Length; j++)
                {
                    gridData[i][j] = readCharList[index];
                    index++;
                }
            }

            //FillWordLeftListRemainingWord();
        }

        public void GetNewGridDataCreated()
        {
            int row = currLevel.gridSize.x;
            int col = currLevel.gridSize.y;

            Debug.Log("Total Word To Find Q : " + DataHandler.UnlockedQuesLetter);

            string unlockStr = "";
            Debug.Log("Unlock String Count : " + unlockStr.Length);
            int totalLetter = (row - 1) * (col - 1);
            Debug.Log("Total Letter : " + totalLetter);
            int unlockedGridWord = totalLetter / DataHandler.UnlockedQuesLetter + 1;
            Debug.Log("Unlock Words Pick Count : " + unlockedGridWord);

            string tempStr = "";

            for (var i = 0; i < unlockedGridWord; i++)
            {
                Debug.Log("Pick Index Val : " + DataHandler.PickDataIndex);

                tempStr += wordsLeftList[0];

                if (i != unlockedGridWord - 1)
                {
                    //Will Not Enter for Last Word !
                    hintAvailList.Add(wordsLeftList[0]);
                    wordsLeftList.RemoveAt(0);
                    DataHandler.PickDataIndex++;
                }

                if (wordsLeftList.Count <= 0)
                {
                    FillNewWordInWordLeftList();
                }
            }

            for (int i = 0; i < tempStr.Trim().Length; i++)
            {
                if (totalLetter > 0)
                {
                    unlockStr += tempStr[i];
                    totalLetter--;
                }
            }

            if (DataHandler.HelperWordIndex >= GameController.instance.maxHelperIndex)
            {
                unlockStr = ShuffleString(unlockStr, 1);
            }

            Debug.Log("Unlock String Data " + unlockStr);

            gridData = new char[row][];
            int unlockIndex = 0;
            int totalIndex = 0;
            for (int i = 0; i < row; i++)
            {
                gridData[i] = new char[col];

                for (int j = 0; j < col; j++)
                {
                    if ((i != row - 1) && j != (col - 1))
                    {
                        //Unlocked Data
                        Debug.Log("Grid Data : " + unlockStr[unlockIndex]);
                        gridData[i][j] = unlockStr[unlockIndex];
                        unlockIndex++;
                    }

                    Debug.Log("Total Index : " + totalIndex);
                    totalIndex++;
                }
            }
        }

        private void LoadWordLeftList()
        {
            Debug.Log("Loaded Word Left List Called !!");
            if (SaveManager.Instance.state.wordLeftList.Count <= 0)
                return;

            Debug.Log("Loaded Word Left List ForEach Called !!");
            wordsLeftList.Clear();

            foreach (string str in SaveManager.Instance.state.wordLeftList)
            {
                Debug.Log("Loaded Data Str  : " + str);
                wordsLeftList.Add(str);
            }
        }


        public string GetWholeGridData()
        {
            string mainStr = "";

            int totalLetter = unlockedGridList.Count;
            Debug.Log("Total Letter : " + totalLetter);
            int unlockedGridWord = totalLetter / DataHandler.UnlockedQuesLetter + 1;
            Debug.Log("Unlock Words Pick Count : " + unlockedGridWord);
            Debug.Log("Unlock Ques Letter : " + DataHandler.UnlockedQuesLetter);
            hintAvailList.Clear();
            string tempStr = "";

            for (var i = 0; i < unlockedGridWord; i++)
            {
                Debug.Log("Pick Index Val : " + DataHandler.PickDataIndex);

                tempStr += wordsLeftList[0];
                if (i != unlockedGridWord - 1)
                {
                    //Will Not Enter for Last Word !
                    hintAvailList.Add(wordsLeftList[0]);
                    wordsLeftList.RemoveAt(0);
                    DataHandler.PickDataIndex++;
                }

                if (wordsLeftList.Count <= 0)
                {
                    FillNewWordInWordLeftList();
                }
            }

            for (int i = 0; i < tempStr.Trim().Length; i++)
            {
                if (totalLetter > 0)
                {
                    mainStr += tempStr[i];
                    totalLetter--;
                }
            }

            Debug.Log("Temp Str " + tempStr);
            Debug.Log("Unlock String Data " + mainStr);

            if (DataHandler.HelperWordIndex >= GameController.instance.maxHelperIndex)
            {
                mainStr = ShuffleString(mainStr);
            }

            return mainStr;
        }


        public string PickRandomWordFormingLetters(List<GridTile> list, int commonLetters,
            out string gridExistingString)
        {
            gridExistingString = "";
            string gridString = "";
            foreach (GridTile gridTile in list)
            {
                gridString += gridTile.GridTextData;
            }

            string tempWord = "";
            int count = 0;
            foreach (string word in wordsLeftList)
            {
                bool isWordSatisfying = HasCommonLetters(gridString, word, commonLetters, out string commonString);

                if (isWordSatisfying)
                {
                    gridExistingString = commonString;
                    tempWord = word;
                    break;
                }

                count++;
            }

            return tempWord.ToLower();
        }

        static bool HasCommonLetters(string word1, string word2, int commonLettersCount, out string commonString)
        {
            int commonCount = 0;
            commonString = "";
            foreach (char letter in word1)
            {
                if (word2.Contains(letter))
                {
                    commonCount++;
                    commonString += letter.ToString();
                    if (commonCount >= commonLettersCount)
                    {
                        Debug.Log("Common String : " + commonString);
                        return true;
                    }
                }
            }

            return false;
        }

        public string GetDataAccordingToGrid(int totalLetters)
        {
            string mainStr = "";
            Debug.Log("Unlock String Count : " + mainStr.Length);
            Debug.Log("Total Letter : " + totalLetters);
            int unlockedGridWord = totalLetters / DataHandler.UnlockedQuesLetter + 1;
            Debug.Log("Unlock Words Pick Count : " + unlockedGridWord);

            string tempStr = "";


            for (var i = 0; i < unlockedGridWord; i++)
            {
                Debug.Log("Pick Index Val : " + DataHandler.PickDataIndex);

                tempStr += wordsLeftList[0];

                if (i != unlockedGridWord - 1)
                {
                    //Will Not Enter for Last Word !
                    hintAvailList.Add(wordsLeftList[0]);
                    wordsLeftList.RemoveAt(0);
                    DataHandler.PickDataIndex++;
                }

                if (wordsLeftList.Count <= 0)
                {
                    FillNewWordInWordLeftList();
                }
            }

            for (int i = 0; i < tempStr.Trim().Length; i++)
            {
                if (totalLetters > 0)
                {
                    mainStr += tempStr[i];
                    totalLetters--;
                }
            }

            Debug.Log("Unlock String Data " + mainStr);
            mainStr = ShuffleString(mainStr);

            Debug.Log("Unlock String Data " + mainStr);

            return mainStr;
        }

        string ShuffleString(string concatenatedString, int shuffleAfter = 0)
        {
            //Use Shuffle After

            char[] charArray = concatenatedString.ToCharArray();
            System.Random random = new System.Random();
            int n = charArray.Length;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                //Swapping character using Deconstruction 
                (charArray[k], charArray[n]) = (charArray[n], charArray[k]);
            }

            return new string(charArray);
        }

        //Get UnAvailable Letter which is not in Screen Avail Grid List , word is randomly selected from the Word Left List. 
        public char GetUnAvailableLetterInRandomWord()
        {
            string gridData = "";
            int random = Random.Range(0, gridAvailableOnScreenList.Count);
            gridData = gridAvailableOnScreenList[random].GridTextData;

            string listWord = "";

            //return the word that start with same letter of gridData which we selected .
            foreach (string str in wordsLeftList)
            {
                if (str[0].ToString() == gridData)
                {
                    listWord = str;
                    Debug.Log("List Word : " + listWord);
                    break;
                }
            }

            Debug.Log("Char in String : " + gridData);
            char ch = NonExistingLetterInGrid(listWord);
            Debug.Log("Char Found !" + ch);
            return ch;
        }

        char NonExistingLetterInGrid(string word, int index = -1)
        {
            char mainChar = ' ';
            Debug.Log("List word In Non Existing Func : " + word);
            for (var i = 0; i < word.Length; i++)
            {
                var ch = word[i];
                bool isLetterThere = false;
                foreach (GridTile tile in gridAvailableOnScreenList)
                {
                    Debug.Log("CH : " + ch);
                    Debug.Log("Tile Data : " + tile.GridTextData);

                    if (ch.ToString() == tile.GridTextData)
                    {
                        isLetterThere = true;
                        break;
                    }
                }

                if (!isLetterThere)
                {
                    Debug.Log("CH : " + ch);

                    if (i == word.Length - 1)
                    {
                        hintAvailList.Add(word);
                    }
                    else
                    {
                        bool isWordThere = CheckIfWholeWordExistsInGrids(word, ch);
                        Debug.Log("IS WORD THERE : " + isWordThere);

                        if (isWordThere)
                        {
                            hintAvailList.Add(word);
                        }
                    }

                    mainChar = ch;
                    return mainChar;
                }
            }

            //Will only come here , when it have found all letters inside the grid of the word.

            if (mainChar == ' ')
            {
                Debug.Log("EMPTY CHAR ");

                if (wordsLeftList.Count <= 0)
                {
                    FillNewWordInWordLeftList();
                }

                //In Case Index Increased to 5, we will print random Letter in the Box.
                if (index >= 5)
                {
                    char ch = GameController.RandomVowel();
                    return ch;
                }

                index++;
                return NonExistingLetterInGrid(wordsLeftList[index], index);
            }

            return mainChar;
        }

        //Check The Word That if all letters exists in the grids available on the Screen (From Index = 0 or that specified).
        private bool CheckIfWholeWordExistsInGrids(string word, char singleChar = ' ')
        {
            List<GridTile> availGridList = new List<GridTile>(gridAvailableOnScreenList);

            foreach (char ch in word)
            {
                //This If statement is only be true for single char does not match the word char.
                if (ch != singleChar)
                {
                    bool isCharAvail = false;
                    foreach (GridTile gridTile in availGridList)
                    {
                        if (gridTile.GridTextData == ch.ToString())
                        {
                            isCharAvail = true;
                            availGridList.Remove(gridTile);
                            break;
                        }
                    }

                    if (!isCharAvail)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public char GetLastUnMatchedLetter(string word, string textWord, int index = 0)
        {
            if (index >= word.Length || index >= textWord.Length)
                return ' ';

            if (word[index] != textWord[index])
            {
                Debug.Log($"Letter Not Same {word} , {textWord} , {index} ");
                return word[index];
            }

            index++;
            return GetLastUnMatchedLetter(word, textWord, index);
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
                    Debug.Log("Ques Selected To Move : " + quesTileList[index]);
                    Vector3 pos = quesTileList[index].transform.position;

                    if (index == DataHandler.UnlockedQuesLetter - 1)
                    {
                        _isLevelRunning = false;
                        LastQuesTile = quesTileList[index];
                    }

                    quesTileList[index].isAssigned = true;
                    inputGridsList.Add(gridTileObj);
                    StartCoroutine(LevelRunningStatus(true, gridTileObj.reachTime + 0.1f));
                    gridTileObj.MoveAfter(pos, true, quesTileList[index]);
                }
            }
        }

        void RemoveLetter(GridTile gridTileObj)
        {
            if (gridTileObj != null)
            {
                inputGridsList.Remove(gridTileObj);
                gridTileObj.MoveAfter(gridTileObj.defaultGridPos, false, gridTileObj.placedOnQuesTile);
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
            for (int i = 0; i < DataHandler.UnlockedQuesLetter; i++)
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
            if (quesTileScript.isUnlocked)
            {
                quesTileList.Add(quesTileScript);
            }
        }

        public void CheckAllGridBuyed()
        {
            if (totalBuyingGridList.Count <= 0 && DataHandler.CurrGridSize < GameController.instance.maxGridSize)
            {
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

            bool isBlocksUsed = true;

            foreach (QuesTile quesTile in quesTileList)
            {
                if (quesTile.isUnlocked && !quesTile.isAssigned)
                {
                    isBlocksUsed = false;
                    break;
                }
            }

            if (!isBlocksUsed)
                return;

            //Check Word Exists or Not !
            string word = WordFormed();

            //bool doWordExist = GameController.instance.Search(word);

            bool doWordExist = englishDictWords.Search(word);

            Debug.Log("Do Word Exist : " + doWordExist);

            if (doWordExist)
            {
                selectedWords.Add(word);
                DataHandler.HelperWordIndex++;

                string temp = word.Trim().ToLower();

                if (wordsLeftList.Contains(temp))
                {
                    Debug.Log("Word Removed From Word Left List !");
                    wordsLeftList.Remove(temp);
                }

                if (hintAvailList.Contains(temp))
                {
                    hintAvailList.Remove(temp);
                }

                WordComplete();
            }
            else
            {
                Vibration.Vibrate(25);
                SoundManager.instance.PlaySound(SoundManager.SoundType.WrongSound);
                GridsBackToPos();
                UIManager.instance.ShakeCam();
                UIManager.instance.WrongEffect();
            }
        }

        private void AddCoin()
        {
            UIManager.instance.CoinCollectionAnimation(coinPerWord);
            Debug.Log("Coin Per Word To Add: " + coinPerWord);
        }

        void WordComplete()
        {
            float time = 0;
            for (int index = 0; index < inputGridsList.Count; index++)
            {
                GridTile gridTile = inputGridsList[index];
                wordCompletedGridList.Add(gridTile);
                gridAvailableOnScreenList.Remove(gridTile);
                gridTile.CallBlastAfterTime();
                gridTile.SetQuesTileStatus(gridTile.placedOnQuesTile, false,
                    gridTile.blastTime / 2 + gridTile.blastEffectAfterTime);

                if (index == inputGridsList.Count - 1)
                {
                    time = gridTile.blastTime / 2;
                    AddCoin();
                }
            }

            DataHandler.IqLevel++;
            UIManager.instance.iqLevelText.text = $"IQ : {DataHandler.IqLevel.ToString()}";
            StartCoroutine(ResetLevelHandlerData(time, true));
            RevertQuesData();
        }

        private void CheckGridLeft()
        {
            if (wordCompletedGridList.Count == unlockedGridList.Count)
            {
                foreach (GridTile gridTile in unlockedGridList)
                {
                    gridTile.transform.position = currLevel.BottomOfScreenPoint();
                }

                wordCompletedGridList.Clear();
                //GameController.instance.Deal();
                GameController.instance.ShuffleGrid(false);
            }
        }

        private void GridsBackToPos()
        {
            foreach (var gridTile in inputGridsList)
            {
                gridTile.MoveAfter(gridTile.defaultGridPos, false, gridTile.placedOnQuesTile,
                    gridTile.blastEffectAfterTime);
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
                if (quesTile.isUnlocked)
                {
                    quesTile.RevertData();
                }
            }


            inputGridsList.Clear();
        }

        string WordFormed()
        {
            string str = "";
            foreach (QuesTile quesTile in quesTileList)
            {
                if (quesTile.isUnlocked)
                {
                    str += quesTile.QuesTextData.ToLower();
                }
            }

            return str;
        }

        public IEnumerator ReturnToDeck(List<GridTile> gridLists, float waitTime, float timeToPlaceGrids,
            bool shouldReturnToGridPlace = true)
        {
            Vector2 pos = currLevel.BottomOfScreenPoint();
            float time = waitTime * gridLists.Count;
            string mainStr = "";

            if (shouldReturnToGridPlace)
            {
                mainStr = GetWholeGridData();
            }

            int index = 0;
            string gridString;
            foreach (GridTile gridTile in gridLists)
            {
                yield return new WaitForSeconds(waitTime);

                if (gridTile.isBlastAfterWordComplete)
                {
                    gridTile.ObjectStatus(false);
                }

                gridString = mainStr.Length > index ? mainStr[index].ToString() : " ";

                gridTile.DeckAnimation(timeToPlaceGrids, pos, gridString, shouldReturnToGridPlace);
                index++;
            }

            if (shouldReturnToGridPlace)
            {
                StartCoroutine(ResetLevelHandlerData(waitTime));
            }

            yield return null;
        }

        public IEnumerator ResetLevelHandlerData(float time, bool doCheckGridLeft = false)
        {
            yield return new WaitForSeconds(time);
            LastQuesTile = null;
            SetLevelRunningBool(true);

            bool isHintAvail = CheckHintStatus(out string finalStr);
            UIManager.instance.HintStatus(isHintAvail);


            if (doCheckGridLeft)
            {
                CheckGridLeft();
            }
        }

        public bool CheckHintStatus(out string passingStr)
        {
            Debug.Log("Check Hint Status Called !");
            List<GridTile> tileList = new List<GridTile>(gridAvailableOnScreenList);

            string gridString = "";

            foreach (GridTile gridTile in tileList)
            {
                gridString += gridTile.GridTextData;
            }

            if (hintAvailList.Count > 0)
            {
                GameController.ShuffleList(hintAvailList);

                foreach (string hintStr in hintAvailList)
                {
                    if (LettersExistInWord(hintStr, gridString))
                    {
                        passingStr = hintStr;
                        return true;
                    }
                }
            }

            Debug.Log("Hint Found In Hint Avail List !");
            passingStr = AnyWordExists();

            if (passingStr.Length > 0)
            {
                return true;
            }

            passingStr = "";
            return false;
        }

        private void FindHint()
        {
            CheckHintStatus(out string finalStr);

            Debug.Log($"Hint Str : {finalStr}, Hint Str Length :  {finalStr.Length}");

            int index = 0;
            int count = DataHandler.UnlockedQuesLetter;

            if (finalStr.Length == count)
            {
                foreach (QuesTile quesTile in quesTileList)
                {
                    if (quesTile.isUnlocked)
                    {
                        quesTile.QuesTextData = finalStr[index].ToString().ToUpper();
                        index++;
                    }
                }
            }
        }

        public void ShowHint()
        {
            _isLevelRunning = false;

            GridsBackToPos();
            FindHint();

            _isLevelRunning = true;
        }

        static bool LettersExistInWord(string word1, string word2)
        {
            // Create dictionaries to store character frequencies
            Dictionary<char, int> charCount1 = new Dictionary<char, int>();
            Dictionary<char, int> charCount2 = new Dictionary<char, int>();

            // Convert both words to lowercase to make the comparison case-insensitive
            word1 = word1.ToLower();
            word2 = word2.ToLower();

            // Count character frequencies in word1
            foreach (char c in word1)
            {
                if (charCount1.ContainsKey(c))
                    charCount1[c]++;
                else
                    charCount1[c] = 1;
            }

            // Count character frequencies in word2
            foreach (char c in word2)
            {
                if (charCount2.ContainsKey(c))
                    charCount2[c]++;
                else
                    charCount2[c] = 1;
            }

            // Check if all characters in word1 exist in word2
            foreach (char c in charCount1.Keys)
            {
                if (!charCount2.ContainsKey(c) || charCount2[c] < charCount1[c])
                    return false;
            }

            return true;
        }

        private string AnyWordExists()
        {
            int count = DataHandler.UnlockedQuesLetter;
            List<GridTile> hintTiles = new List<GridTile>();
            List<GridTile> totalRemainingList = new List<GridTile>(gridAvailableOnScreenList);
            GameController.ShuffleList(totalRemainingList);
            // Debug.Log("Grid Remaining Counts : " + gridsRemainingList.Count);
            string finalStr = "";

            for (int i = 0; i < totalRemainingList.Count; i++)
            {
                List<GridTile> gridsRemainingList = new List<GridTile>(totalRemainingList);
                Debug.Log("GridsRemainingList Count : " + gridsRemainingList.Count);
                string word = gridsRemainingList[0].GridTextData;
                gridsRemainingList.Remove(gridsRemainingList[0]);
                hintTiles.Add(gridsRemainingList[0]);
                Debug.Log("HINT GRID : " + gridsRemainingList[0].name, gridsRemainingList[0].gameObject);

                Debug.Log("Word First Letter Selected : " + word);
                int index = 1;

                List<MainDictionary.WordLengthDetailedInfo> dictWordList =
                    GameController.instance.GetWordListOfLength(count,
                        gridsRemainingList[0].GridTextData);

                Debug.Log("Word List Length : " + dictWordList.Count);
                Debug.Log("Char Selected : " + dictWordList[0].wordStartChar);

                for (int j = 0; j < dictWordList.Count; j++)
                {
                    if (dictWordList[j].wordStartChar.ToString() == word)
                    {
                        List<string> wordList = dictWordList[j].wordList;
                        Debug.Log("LINE COUNT : " + wordList.Count);

                        foreach (string str in wordList)
                        {
                            //str = Each Word in Text File
                            if (!string.IsNullOrWhiteSpace(str))
                            {
                                word = str[0].ToString();
                                Debug.Log("Text Word : " + str);
                                bool isWordFormed = FindWordThroughCharacter(index, word, str,
                                    new List<GridTile>(gridsRemainingList), hintTiles,
                                    out finalStr);

                                if (!isWordFormed)
                                {
                                    hintTiles.Clear();
                                }
                                else
                                {
                                    Debug.Log("Word Found in List");
                                    return finalStr;
                                }
                            }
                        }

                        Debug.Log("Word : " + word);
                        word += wordList[index];
                    }
                }

                hintTiles.Clear();
            }

            Debug.Log("Hint Does Not Exists !");

            return finalStr;
        }


        bool FindWordThroughCharacter(int index, string formedWord, string word, List<GridTile> gridRemainingList,
            List<GridTile> hintTileList,
            out string finalStr)
        {
            Debug.Log("Grid Remaining Count : " + gridRemainingList.Count);
            Debug.Log("Formed Word : " + formedWord);
            foreach (GridTile gridTile in gridRemainingList)
            {
                Debug.Log("GRID NAME : " + gridTile.gameObject.name, gridTile.gameObject);
                string tempWord = formedWord + gridTile.GridTextData;
                string subStringWord = word.Substring(0, index + 1);
                Debug.Log("Sub String : " + subStringWord + "  " + subStringWord.Length);
                Debug.Log("Temp String : " + tempWord + "  " + tempWord.Length);

                if (tempWord == subStringWord)
                {
                    Debug.Log("Word Formed : " + tempWord);

                    if (tempWord == subStringWord && tempWord.Length == DataHandler.UnlockedQuesLetter)
                    {
                        hintTileList.Add(gridTile);
                        Debug.Log($"Matching Word {tempWord}!!");
                        finalStr = tempWord;
                        Debug.Log("Hint List Count : " + hintTileList.Count);

                        foreach (GridTile tile in hintTileList)
                        {
                            GameObject obj = tile.gameObject;
                            Debug.Log("HINT GRID : " + obj.name, obj);
                        }

                        return true;
                    }

                    hintTileList.Add(gridTile);
                    gridRemainingList.Remove(gridTile);
                    index++;
                    return FindWordThroughCharacter(index, tempWord, word, gridRemainingList, hintTileList,
                        out finalStr);
                }
            }

            hintTileList.Clear();
            finalStr = "";
            return false;
        }

        public void SetCoinPerWord()
        {
            foreach (CoinHandlerScriptable.WordCompleteCoinData wordInfo in GameController.instance
                         .GetCoinDataScriptable().wordCompleteCoinDataList)
            {
                if (wordInfo.quesLetterCount == DataHandler.CurrTotalQuesSize)
                {
                    coinPerWord = wordInfo.coinPerWord;
                }
            }
        }

        public void UnlockNextGridForCoins()
        {
            //Unlock next grid using coins...
            if (DataHandler.UnlockGridIndex < lockedGridList.Count)
            {
                GridTile gridTile = lockedGridList[DataHandler.UnlockGridIndex];

                int unlockAmount = GameController.instance.GetCoinDataScriptable()
                    .quesUnlockDataList[DataHandler.CoinGridUnlockIndex];

                coinToUnlockNextGrid = unlockAmount;
                gridTile.SetLockTextAmount(coinToUnlockNextGrid);

                Debug.Log(" Unlock Next Grid Coin Called for " + gridTile.gameObject.name);
                gridTile.SetCurrentLockStatus(true);
                gridTile.isCurrLock = true;
            }
        }

        public void ClearAllLists()
        {
            inputGridsList.Clear();
            unlockedGridList.Clear();
            totalGridsList.Clear();
            quesTileList.Clear();
            totalBuyingGridList.Clear();
            wordCompletedGridList.Clear();
            gridAvailableOnScreenList.Clear();
            hintAvailList.Clear();
            wordsLeftList.Clear();
            lockedGridList.Clear();
        }

        public void ClearInGameList()
        {
            inputGridsList.Clear();
            wordCompletedGridList.Clear();
            hintAvailList.Clear();
        }

        private void SaveGridLockSystem()
        {
            List<GridTile> totalList = new List<GridTile>(totalGridsList);
            List<char> gridLockStatusList = new List<char>();

            // 0 : Unlocked Grid Tile..
            // 1 : Curr Lock Grid To Open..
            // 2 : Fully Locked Grid , cannot be opened yet..
            foreach (var gridTile in totalList)
            {
                if (!gridTile.isCurrLock && !gridTile.isFullLocked)
                {
                    //Grid is opened...
                    gridLockStatusList.Add(gridTile.GridTextData[0]);
                }
                else if (gridTile.isCurrLock)
                {
                    //This is next Tile which will open with coins...
                    gridLockStatusList.Add('*');
                }
                else
                {
                    //This tile is fully Locked and only opens when it changes to isCurrLock..
                    gridLockStatusList.Add('-');
                }
            }

            SaveManager.Instance.state.gridDataList = new List<char>(gridLockStatusList);
            SaveManager.Instance.UpdateState();
        }

        private void SaveGridOnScreenSystem()
        {
            List<GridTile> totalList = new List<GridTile>(totalGridsList);
            List<bool> gridScreenList = new List<bool>();

            // 0 : Grid Tile is not inside the Screen..
            // 1 : Grid Tile is inside the Screen..
            foreach (var gridTile in totalList)
            {
                if (!gridTile.isBlastAfterWordComplete)
                {
                    gridScreenList.Add(false);
                }
                else
                {
                    gridScreenList.Add(true);
                }
            }

            SaveManager.Instance.state.gridOnScreenList = new List<bool>(gridScreenList);
            SaveManager.Instance.UpdateState();
        }

        private void SaveHintList()
        {
            List<string> hintList = new List<string>(hintAvailList);
            SaveManager.Instance.state.hintList = new List<string>(hintList);
            SaveManager.Instance.UpdateState();
        }

        private void SaveWordLeftList()
        {
            List<string> wordList = new List<string>(wordsLeftList);
            SaveManager.Instance.state.wordLeftList = new List<string>(wordList);
            SaveManager.Instance.UpdateState();
        }

        public void SaveSystem()
        {
            SaveGridLockSystem();
            SaveGridOnScreenSystem();
            SaveHintList();
            SaveWordLeftList();
        }
    }
}