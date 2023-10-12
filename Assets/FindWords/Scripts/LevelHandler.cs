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

        public EnglishDictWords englishDictWords;
        private Renderer _touchEffectRenderer;
        [SerializeField] Level currLevel;
        [HideInInspector] public int quesGridCount;
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
            // totalGridsList = new List<GridTile>();
            // inputGridsList = new List<GridTile>();
            // wordCompletedGridList = new List<GridTile>();
            // unlockedGridList = new List<GridTile>();
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
            int minLetter = GameController.instance.GetDifficultyInfo().minQuesLetter;

            if (minLetter < DataHandler.instance.CurrQuesSize)
            {
                currLevel.totalWordToFind = DataHandler.instance.CurrQuesSize;
            }
            else
            {
                currLevel.totalWordToFind = minLetter;
            }

            Debug.Log("Total Word To Find Q : " + currLevel.totalWordToFind);

            TextAsset wordTextFile = GameController.instance.GetDifficultyInfo().wordTextFile;
            string data = wordTextFile.text.Trim();
            string[] lines = data.Split('\n');
            Debug.Log("Lines : " + lines.Length);

            List<string> totalWords = new List<string>();
            foreach (string s in lines)
            {
                totalWords.Add(s.ToLower());
            }

            foreach (string word in totalWords)
            {
                if (selectedWords.Count == 0 || !selectedWords.Contains(word.ToLower()))
                {
                    wordsLeftList.Add(word);
                }
            }

            int row = currLevel.gridSize.x;
            int col = currLevel.gridSize.y;

            int unlockedGridWord = ((row - 1) * (col - 1) / currLevel.totalWordToFind) + 1;
            string unlockStr = GetDataFromList(unlockedGridWord).Trim();
            Debug.Log("Unlock Str Length : " + unlockStr.Length);

            int lockedWords = ((row + col - 1) / currLevel.totalWordToFind) + 1;
            List<string> randomWords = SelectRandomWords(wordsLeftList, lockedWords);
            string lockedStr = string.Join("", randomWords);
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

            gridFillString = GetDataFromList(fillGridStringLength);
        }

        string GetDataFromList(int totalCharsToBeSelected)
        {
            string finalStr = "";

            List<string> randomWords = SelectRandomWords(wordsLeftList, totalCharsToBeSelected);

            //string concatenatedString = string.Join("", randomWords);
            //finalStr = concatenatedString;
            finalStr = ConcatenateAndShuffleStrings(randomWords);
            return finalStr;
        }

        static string ConcatenateAndShuffleStrings(List<string> stringList)
        {
            string concatenatedString = string.Join("", stringList);
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
            gridTile.GridTextData = gridFillString[0].ToString();
            UpdateGridFillString();
        }

        string UpdateGridFillString()
        {
            if (string.IsNullOrEmpty(gridFillString) || gridFillString.Length <= 1)
            {
                gridFillString = GetDataFromList(fillGridStringLength);
                // Handle the case where the input is empty or only has one character.
                return string.Empty;
            }

            gridFillString = gridFillString.Substring(1);
            //Debug.Log("Grid Fill String : " + gridFillString);

            return gridFillString;
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
                gridAvailableOnScreenList.Remove(gridTile);
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
                str += quesTile.QuesTextData.ToLower();
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

                if (gridTile.isBlastAfterWordComplete)
                {
                    gridTile.ObjectStatus(false);
                }

                gridTile.DeckAnimation(timeToPlaceGrids, pos, shouldReturnToGridPlace);
            }

            StartCoroutine(nameof(ResetLevelHandlerData), waitTime);
            yield return null;
        }

        public IEnumerator ResetLevelHandlerData(float time)
        {
            yield return new WaitForSeconds(time);
            LastQuesTile = null;
            SetLevelRunningBool(true);
        }

        public void ShowHint()
        {
            _isLevelRunning = false;
            GridsBackToPos();
            string final = AnyWordExists();

            Debug.Log("FINAL STR : " + final);
            _isLevelRunning = true;
        }

        private string AnyWordExists()
        {
            string word = "";
            List<GridTile> hintTiles = new List<GridTile>();
            List<GridTile> tileAlreadySelected = new List<GridTile>();
            List<GridTile> gridsRemainingList = new List<GridTile>(gridAvailableOnScreenList);
            Debug.Log("Grid Remaining Counts : " + gridsRemainingList.Count);
            string finalStr = "";
            for (int i = 0; i < gridAvailableOnScreenList.Count; i++)
            {
                gridsRemainingList.Remove(gridAvailableOnScreenList[i]);
                word = gridAvailableOnScreenList[i].GridTextData.ToLower();
                Debug.Log("Word First Letter Selected : " + word);
                int index = 1;

                List<MainDictionary.WordLengthDetailedInfo> wordList =
                    GameController.instance.GetWordListOfLength(quesGridCount,
                        gridAvailableOnScreenList[i].GridTextData.ToLower());

                Debug.Log("Word List Length : " + wordList.Count);
                Debug.Log("Char Selected : " + wordList[i].wordStartChar);

                for (int j = 0; j < wordList.Count; j++)
                {
                    if (wordList[j].wordStartChar.ToString() == word)
                    {
                        TextAsset file = wordList[j].wordText;
                        string[] lines = file.text.Split('\n');

                        foreach (string str in lines)
                        {
                            if (!string.IsNullOrWhiteSpace(str))
                            {
                                word = str[0].ToString();
                                Debug.Log("Text Word : " + str);
                                bool isWordFormed = FindWordThroughCharacter(index, word, str, gridsRemainingList,
                                    out finalStr);

                                if (!isWordFormed)
                                {
                                    hintTiles.Clear();
                                    gridsRemainingList = new List<GridTile>(gridAvailableOnScreenList);
                                }
                                else
                                {
                                    Debug.Log("Word Found in List");
                                    return finalStr;
                                }
                            }
                        }

                        //hintTiles.Add();
                        Debug.Log("Word : " + word);
                        word += wordList[index].ToString();
                    }
                }
            }


            if (word.Length == currLevel.totalWordToFind)
            {
                Debug.Log("Hint Exists !");
            }
            else
            {
                Debug.Log("Hint Does Not Exists !");
            }

            return finalStr;
        }


        bool FindWordThroughCharacter(int index, string formedWord, string word, List<GridTile> gridRemainingList,
            out string finalStr)
        {
            Debug.Log("Grid Remaining Count : " + gridRemainingList.Count);
            Debug.Log("Formed Word : " + formedWord);
            foreach (GridTile gridTile in gridRemainingList)
            {
                string tempWord = formedWord + gridTile.GridTextData.ToLower();
                string subStringWord = word.Substring(0, index + 1);
                Debug.Log("Sub String : " + subStringWord + "  " + subStringWord.Length);
                Debug.Log("Temp String : " + tempWord + "  " + tempWord.Length);
                if (tempWord == subStringWord)
                {
                    Debug.Log("Word Formed : " + tempWord);

                    if (tempWord == subStringWord && tempWord.Length == quesGridCount)
                    {
                        Debug.Log($"Matching Word {tempWord}!!");
                        finalStr = tempWord;
                        return true;
                    }

                    gridRemainingList.Remove(gridTile);
                    index++;
                    return FindWordThroughCharacter(index, tempWord, word, gridRemainingList, out finalStr);
                }
            }

            finalStr = "";
            return false;
        }

        private void PlayHintAnimation(GridTile gridTile, int index)
        {
        }

        void RemoveHint()
        {
        }
    }
}