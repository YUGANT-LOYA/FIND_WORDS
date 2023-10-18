using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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

        DefinedLevelScriptable _definedLevelScriptable;
        private List<DefinedLevelScriptable.DefinedLevelInfo> _definedLevelInfo;

        public EnglishDictWords englishDictWords;
        private Renderer _touchEffectRenderer;
        [SerializeField] Level currLevel;
        [HideInInspector] public int totalQuesGridCount;
        public int coinPerLetter = 10;
        [Header("Level Info")] public char[][] gridData;

        public List<GridTile> totalGridsList = new List<GridTile>(),
            inputGridsList = new List<GridTile>(),
            wordCompletedGridList = new List<GridTile>(),
            unlockedGridList = new List<GridTile>(),
            gridAvailableOnScreenList = new List<GridTile>(),
            buyGridList = new List<GridTile>(),
            buyQuesGridList = new List<GridTile>();

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
            //Debug.Log("Level Handler Init Called !");
            cam = Camera.main;
        }

        public void SetHelperLevelDataScriptable()
        {
            _definedLevelScriptable = GameController.instance.GetDefinedLevelInfoScriptable();
            _definedLevelInfo =
                new List<DefinedLevelScriptable.DefinedLevelInfo>(_definedLevelScriptable.definedLevelInfoList);
        }

        public void LevelStartInit()
        {
            _isLevelRunning = true;
        }

        public void FillWordListAtStartOfGame()
        {
            List<PickWordDataInfo.PickingDataInfo> pickInfoList =
                GameController.instance.GetPickDataInfo().pickDataInfoList;

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


        public void UpdateWordLeftList()
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

        public void GetGridLetterData()
        {
            int row = currLevel.gridSize.x;
            int col = currLevel.gridSize.y;

            QuestionSizeInfo.SizeInfo sizeInfo = GameController.instance.GetQuestionDataInfo()
                .difficultyInfos[DataHandler.CurrQuesInfoIndex];

            Debug.Log("Total Word To Find Q : " + DataHandler.UnlockedQuesLetter);

            FillWordListAtStartOfGame();

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
                    UpdateWordLeftList();
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

            if (DataHandler.HelperWordIndex > GameController.instance.maxHelperIndex)
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

        public void AssignWholeGridData()
        {
            string mainStr = "";
            Debug.Log("Unlock String Count : " + mainStr.Length);
            int totalLetter = (currLevel.gridSize.x - 1) * (currLevel.gridSize.x - 1);
            Debug.Log("Total Letter : " + totalLetter);
            int unlockedGridWord = totalLetter / DataHandler.UnlockedQuesLetter + 1;
            Debug.Log("Unlock Words Pick Count : " + unlockedGridWord);

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
                    UpdateWordLeftList();
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

            Debug.Log("Unlock String Data " + mainStr);

            if (DataHandler.HelperWordIndex > GameController.instance.maxHelperIndex)
            {
                mainStr = ShuffleString(mainStr);
            }

            int index = 0;

            foreach (GridTile gridTile in unlockedGridList)
            {
                gridTile.GridTextData = mainStr[index].ToString();
                index++;
            }
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
                    UpdateWordLeftList();
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

            int index = 0;

            Debug.Log("Unlock String Data " + mainStr);
            mainStr = ShuffleString(mainStr);

            Debug.Log("Unlock String Data " + mainStr);
            // foreach (GridTile gridTile in list)
            // {
            //     gridTile.GridTextData = mainStr[index].ToString();
            //     index++;
            // }

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
                    UpdateWordLeftList();
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

        public char GetLastUnMatchedLetter(string word, string textWord, int index = 0)
        {
            if (index >= word.Length || index >= textWord.Length)
                return ' ';

            if (word[index] != textWord[index])
            {
                Debug.Log($"Letter Not Same {word} , {textWord} , {index} ");
                return word[index];
            }
            else
            {
                return GetLastUnMatchedLetter(word, textWord, index);
            }

            index++;
            return ' ';
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
            if (buyGridList.Count <= 0 && DataHandler.CurrGridSize < GameController.instance.maxGridSize)
            {
                DataHandler.CurrGridSize++;

                // if (DataHandler.CurrGridSize > GameController.instance.maxGridSize)
                // {
                //     DataHandler.CurrGridSize = GameController.instance.maxGridSize;
                // }


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

            // TextAsset file = GameController.instance.get

            // if (wordsLeftList.Contains(word))
            // {
            //     selectedWords.Add(word);
            //     DataHandler.HelperWordIndex++;
            //
            //     foreach (string str in wordsLeftList)
            //     {
            //         if (str == word)
            //         {
            //             wordsLeftList.Remove(word);
            //         }
            //     }
            //
            //     WordComplete();
            //     return;
            // }
            //

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
            }
        }

        private void AddCoin()
        {
            int count = DataHandler.UnlockedQuesLetter;
            UIManager.instance.CoinCollectionAnimation(coinPerLetter * count);
            Debug.Log("Coin Per Word To Add: " + coinPerLetter * count);
        }

        void WordComplete()
        {
            float time = 0;
            for (int index = 0; index < inputGridsList.Count; index++)
            {
                GridTile gridTile = inputGridsList[index];
                wordCompletedGridList.Add(gridTile);
                gridAvailableOnScreenList.Remove(gridTile);
                gridTile.Blast();
                gridTile.SetQuesTileStatus(gridTile.placedOnQuesTile, false, gridTile.blastTime / 2);

                if (index == inputGridsList.Count - 1)
                {
                    time = gridTile.blastTime / 2;
                    AddCoin();
                }
            }

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
                GameController.instance.ShuffleGrid();
            }
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
            StartCoroutine(UpdateGridDataAfterATime(time));

            foreach (GridTile gridTile in gridLists)
            {
                yield return new WaitForSeconds(waitTime);

                if (gridTile.isBlastAfterWordComplete)
                {
                    gridTile.ObjectStatus(false);
                }

                gridTile.DeckAnimation(timeToPlaceGrids, pos, shouldReturnToGridPlace);
            }

            if (shouldReturnToGridPlace)
            {
                StartCoroutine(ResetLevelHandlerData(waitTime));
            }

            yield return null;
        }

        IEnumerator UpdateGridDataAfterATime(float time)
        {
            yield return new WaitForSeconds(time);
            AssignWholeGridData();
        }

        public IEnumerator ResetLevelHandlerData(float time, bool doCheckGridLeft = false)
        {
            yield return new WaitForSeconds(time);
            LastQuesTile = null;
            SetLevelRunningBool(true);

            if (doCheckGridLeft)
            {
                CheckGridLeft();
            }
        }

        public void FindHint()
        {
            List<GridTile> tileList = new List<GridTile>(gridAvailableOnScreenList);
            string gridString = "";
            bool isHintThere = false;
            string finalStr = "";

            foreach (GridTile gridTile in tileList)
            {
                gridString += gridTile.GridTextData;
            }

            if (hintAvailList.Count > 0)
            {
                foreach (string hintStr in hintAvailList)
                {
                    if (LettersExistInWord(hintStr, gridString))
                    {
                        finalStr = hintStr;
                        isHintThere = true;
                        break;
                    }
                }
            }


            if (!isHintThere)
            {
                Debug.Log("Hint Found In Hint Avail List !");
                finalStr = AnyWordExists();
            }

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
            else
            {
                UIManager.instance.HintStatus(false);
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
                        gridsRemainingList[i].GridTextData);

                Debug.Log("Word List Length : " + dictWordList.Count);
                Debug.Log("Char Selected : " + dictWordList[i].wordStartChar);

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
                                    //gridsRemainingList.Remove(totalRemainingList[0]);
                                }
                                else
                                {
                                    Debug.Log("Word Found in List");
                                    return finalStr;
                                }
                            }
                        }

                        Debug.Log("Word : " + word);
                        word += wordList[index].ToString();
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
    }
}