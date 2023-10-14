using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [HideInInspector] public int totalQuesGridCount, currUnlockedQuesGridCount;
        [FormerlySerializedAs("coinPerLevel")] public int coinPerLetter = 10;
        [Header("Level Info")] public char[][] gridData;
        public float timeToShowHint = 0.5f, timeToRotateGrid = 0.5f;

        public List<GridTile> totalGridsList = new List<GridTile>(),
            inputGridsList = new List<GridTile>(),
            wordCompletedGridList = new List<GridTile>(),
            unlockedGridList = new List<GridTile>(),
            gridAvailableOnScreenList = new List<GridTile>(),
            buyGridList = new List<GridTile>(),
            buyQuesGridList = new List<GridTile>();

        public List<QuesTile> quesTileList = new List<QuesTile>();
        public List<string> selectedWords = new List<string>(), wordsLeftList = new List<string>();
        public int fillGridStringLength = 10;
        [SerializeField] private string gridFillString = "";
        private bool _isLevelRunning = true;
        private int _colorIndex;
        private QuesTile _lastQuesTile;
        public string hintAvailString = "";

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
            hintAvailString = "";
            GameController.instance.ResetData();
        }

        public QuesTile LastQuesTile
        {
            get => _lastQuesTile;
            set => _lastQuesTile = value;
        }

        public void GetGridData()
        {
            int minLetter = GameController.instance.GetDifficultyInfo().minQuesLetter;
            int row = currLevel.gridSize.x;
            int col = currLevel.gridSize.y;

            if (minLetter < DataHandler.CurrQuesSize)
            {
                currLevel.totalWordToFind = DataHandler.CurrQuesSize;
            }
            else
            {
                currLevel.totalWordToFind = minLetter;
            }

            Debug.Log("Total Word To Find Q : " + currLevel.totalWordToFind);

            string unlockStr = "", lockedStr = "";
            int unlockedGridWord = ((row - 1) * (col - 1) / currLevel.totalWordToFind) + 1;
            Debug.Log("Unlock Words Pick Count : " + unlockedGridWord);
            int lockedWords = ((row + col - 1) / currLevel.totalWordToFind) + 1;
            int num = DataHandler.CurrDefinedLevel;
            string data, fillStr;
            string[] lines;
            TextAsset textFile;

            if (num < _definedLevelInfo.Count &&
                DataHandler.CurrGridSize < GameController.instance.startingGridSize + 1)
            {
                textFile = _definedLevelScriptable.helperTextFile;
            }
            else
            {
                textFile = GameController.instance.GetDifficultyInfo().wordTextFile;
            }

            data = textFile.text.Trim();
            lines = data.Split('\n');
            Debug.Log("Lines : " + lines.Length);

            List<string> totalWords = new List<string>();

            foreach (string s in lines)
            {
                totalWords.Add(s.Trim().ToLower());
            }

            foreach (string word in totalWords)
            {
                if (selectedWords.Count == 0 || !selectedWords.Contains(word))
                {
                    wordsLeftList.Add(word);
                }
            }

            if (num < _definedLevelInfo.Count &&
                DataHandler.CurrGridSize < GameController.instance.startingGridSize + 1)
            {
                //Comes until Player's Level is Predefined and Grid Size is Minimum !!
                for (int i = DataHandler.HelperWordIndex; i < DataHandler.HelperWordIndex + unlockedGridWord; i++)
                {
                    Debug.Log("Defined Level Running !!");
                    unlockStr += lines[i].Trim();
                }

                DataHandler.HelperWordIndex += unlockedGridWord;

                if (_definedLevelInfo[num].shouldShuffle)
                {
                    unlockStr = ShuffleString(unlockStr);
                }
            }
            else
            {
                //Comes when Player Level is Increased !!

                unlockStr = GetDataFromList(unlockedGridWord, wordsLeftList).Trim();
            }

            Debug.Log("Unlock String Data " + unlockStr);

            List<string> randomWords = SelectRandomWords(wordsLeftList, lockedWords);
            lockedStr = string.Join("", randomWords);
            Debug.Log("Lock Str Length : " + unlockStr.Length);

            gridData = new char[row][];
            int unlockIndex = 0, lockIndex = 0;
            int totalIndex = 0;
            for (int i = 0; i < row; i++)
            {
                gridData[i] = new char[col];

                for (int j = 0; j < col; j++)
                {
                    if (i == row - 1 || j == col - 1)
                    {
                        //Locked Data
                        Debug.Log("Grid Data : " + lockedStr[lockIndex]);
                        gridData[i][j] = lockedStr[lockIndex];
                        lockIndex++;
                    }
                    else
                    {
                        //Unlocked Data
                        Debug.Log("Grid Data : " + unlockStr[unlockIndex]);
                        gridData[i][j] = unlockStr[unlockIndex];
                        unlockIndex++;
                    }

                    Debug.Log("Total Index : " + totalIndex);
                    totalIndex++;

                    //gridData[i][j] = GenerateRandom_ASCII_Code()[0];
                }
            }

            if (num > _definedLevelInfo.Count)
            {
                gridFillString = GetDataFromList(fillGridStringLength, wordsLeftList);
            }
        }

        string GetDataFromList(int totalCharsToBeSelected, List<string> leftList)
        {
            string finalStr = "";
            List<string> randomWords = SelectRandomWords(leftList, totalCharsToBeSelected);
            finalStr = ConcatenateAndShuffleStrings(randomWords);
            return finalStr;
        }

        string ConcatenateWordsWithoutShuffling(int numOfWords)
        {
            string data = _definedLevelScriptable.helperTextFile.text;
            string[] lines = data.Split('\n');
            string str = "";

            for (int i = DataHandler.HelperWordIndex; i < DataHandler.HelperWordIndex + numOfWords; i++)
            {
                str += lines[i];
            }

            return str;
        }

        static string ConcatenateAndShuffleStrings(List<string> stringList)
        {
            string concatenatedString = string.Join("", stringList);

            string shuffledString = ShuffleString(concatenatedString);

            return shuffledString;
        }

        static string ShuffleString(string concatenatedString)
        {
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

        private List<string> SelectRandomWords(List<string> wordList, int count)
        {
            if (count >= wordList.Count)
            {
                selectedWords = new List<string>(wordList);
                // Return the entire list if the requested count is greater than or equal to the list size.
                return wordList;
            }

            List<string> randomWords = new List<string>();
            System.Random random = new System.Random();

            while (randomWords.Count < count)
            {
                int randomIndex = random.Next(wordList.Count);
                string selectedWord = wordList[randomIndex].Trim();

                if (!randomWords.Contains(selectedWord))
                {
                    randomWords.Add(selectedWord);
                    Debug.Log("Random Word Selected : " + selectedWord);
                }
            }

            return randomWords;
        }

        public void UpdateTextDataForGrid(GridTile gridTile)
        {
            if (string.IsNullOrEmpty(gridFillString) || gridFillString.Length <= 0)
            {
                if (DataHandler.HelperWordIndex < _definedLevelInfo.Count &&
                    DataHandler.CurrGridSize < GameController.instance.startingGridSize + 1)
                {
                    gridFillString = ConcatenateWordsWithoutShuffling(1);
                }
                else
                {
                    gridFillString = GetDataFromList(fillGridStringLength, wordsLeftList);
                }
            }

            gridTile.GridTextData = gridFillString[0].ToString();
            gridFillString = gridFillString.Substring(1);
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

                    if (index == currUnlockedQuesGridCount - 1)
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
            for (int i = 0; i < currUnlockedQuesGridCount; i++)
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
            if (buyGridList.Count <= 0)
            {
                DataHandler.CurrGridSize++;

                if (DataHandler.CurrGridSize > GameController.instance.maxGridSize)
                {
                    DataHandler.CurrGridSize = GameController.instance.maxGridSize;
                }


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
            // bool isBlocksUsed = quesTileList.All(quesTile => quesTile.isAssigned);

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

            bool doWordExist = GameController.instance.Search(word);

            //bool doWordExist = englishDictWords.Search(word);

            Debug.Log("Do Word Exist : " + doWordExist);

            if (doWordExist)
            {
                selectedWords.Add(word);
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
            int count = currUnlockedQuesGridCount;
            UIManager.instance.CoinCollectionAnimation(coinPerLetter * count);
            Debug.Log("Coin Per Word To Add: " + coinPerLetter * count);
        }

        void WordComplete()
        {
            for (int index = 0; index < inputGridsList.Count; index++)
            {
                var gridTile = inputGridsList[index];
                wordCompletedGridList.Add(gridTile);
                gridAvailableOnScreenList.Remove(gridTile);
                gridTile.Blast();
                gridTile.SetQuesTileStatus(gridTile.placedOnQuesTile, false, gridTile.blastTime / 2);

                if (index == inputGridsList.Count - 1)
                {
                    AddCoin();
                }
            }

            RevertQuesData();
            SetHintButtonActivationStatus();
        }


        public void SetHintButtonActivationStatus()
        {
            hintAvailString = HintAvailableString();

            if (string.IsNullOrWhiteSpace(hintAvailString))
            {
                Debug.Log("Hint Not Found !!");
                UIManager.instance.HintStatus(false);
            }
            else
            {
                Debug.Log("Hint Found !!");
                UIManager.instance.HintStatus(true);
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

            int totalVowel = gridLists.Count / 2 + 1;
            foreach (GridTile gridTile in gridLists)
            {
                yield return new WaitForSeconds(waitTime);

                if (gridTile.isBlastAfterWordComplete)
                {
                    gridTile.ObjectStatus(false);
                }

                gridTile.DeckAnimation(totalVowel, timeToPlaceGrids, pos, shouldReturnToGridPlace);
                totalVowel--;
            }

            SetHintButtonActivationStatus();
            StartCoroutine(nameof(ResetLevelHandlerData), waitTime);
            yield return null;
        }

        public IEnumerator ResetLevelHandlerData(float time)
        {
            yield return new WaitForSeconds(time);
            LastQuesTile = null;
            SetLevelRunningBool(true);
        }

        public string HintAvailableString()
        {
            return AnyWordExists();
        }

        public void ShowHint()
        {
            _isLevelRunning = false;
            GridsBackToPos();

            string str = "";
            Debug.Log("Curr Unlock Grid Count " + currUnlockedQuesGridCount);
            Debug.Log("HintAvailString Length " + hintAvailString.Length);
            if (string.IsNullOrWhiteSpace(hintAvailString) ||
                string.IsNullOrEmpty(hintAvailString))
            {
                str = AnyWordExists();
                Debug.Log("FINDING NEW FINAL STR  : " + str);
            }
            else if (currUnlockedQuesGridCount != hintAvailString.Length)
            {
                str = AnyWordExists();
                Debug.Log("QUES GRID INCREASED, NEW FINAL STR  : " + str);
            }
            else
            {
                str = hintAvailString;
                Debug.Log("EXISTING FINAL STR : " + str);
            }

            int index = 0;
            int count = currUnlockedQuesGridCount;
            if (str.Length == count)
            {
                foreach (QuesTile quesTile in quesTileList)
                {
                    if (quesTile.isUnlocked)
                    {
                        quesTile.QuesTextData = str[index].ToString().ToUpper();
                        index++;
                    }
                }
            }
            else
            {
                UIManager.instance.HintStatus(false);
            }

            _isLevelRunning = true;
        }

        private string AnyWordExists()
        {
            int count = currUnlockedQuesGridCount;
            List<GridTile> hintTiles = new List<GridTile>();
            List<GridTile> totalRemainingList = new List<GridTile>(gridAvailableOnScreenList);
            GameController.ShuffleList(totalRemainingList);
            // Debug.Log("Grid Remaining Counts : " + gridsRemainingList.Count);
            string finalStr = "";

            for (int i = 0; i < totalRemainingList.Count; i++)
            {
                List<GridTile> gridsRemainingList = new List<GridTile>(totalRemainingList);
                string word = totalRemainingList[0].GridTextData.ToLower();
                gridsRemainingList.Remove(totalRemainingList[0]);
                hintTiles.Add(totalRemainingList[0]);
                Debug.Log("HINT GRID : " + totalRemainingList[i].name, totalRemainingList[i].gameObject);
                Debug.Log("Word First Letter Selected : " + word);
                int index = 1;

                List<MainDictionary.WordLengthDetailedInfo> wordList =
                    GameController.instance.GetWordListOfLength(count,
                        totalRemainingList[i].GridTextData.ToLower());

                Debug.Log("Word List Length : " + wordList.Count);
                Debug.Log("Char Selected : " + wordList[i].wordStartChar);

                for (int j = 0; j < wordList.Count; j++)
                {
                    if (wordList[j].wordStartChar.ToString() == word)
                    {
                        TextAsset file = wordList[j].wordText;
                        string[] lines = file.text.Split('\n');
                        Debug.Log("LINE COUNT : " + lines.Length);
                        foreach (string str in lines)
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
                                    hintAvailString = finalStr;
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
                string tempWord = formedWord + gridTile.GridTextData.ToLower();
                string subStringWord = word.Substring(0, index + 1);
                Debug.Log("Sub String : " + subStringWord + "  " + subStringWord.Length);
                Debug.Log("Temp String : " + tempWord + "  " + tempWord.Length);
                if (tempWord == subStringWord)
                {
                    Debug.Log("Word Formed : " + tempWord);

                    if (tempWord == subStringWord && tempWord.Length == currUnlockedQuesGridCount)
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
                // else
                // {  
                //     hintTileList.Clear();
                //     finalStr = "";
                //     return false;
                // }
            }

            hintTileList.Clear();
            finalStr = "";
            return false;
        }
    }
}