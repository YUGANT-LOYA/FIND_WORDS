using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace YugantLoyaLibrary.FindWords
{
    public class LevelHandler : MonoBehaviour
    {
        public static LevelHandler instance;

        public delegate void NewLetterDelegate(GridTile gridTile);

        public NewLetterDelegate onNewLetterAddEvent, onRemoveLetterEvent;

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
        [HideInInspector] public bool isHintAvailInButton;

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
            CreateSingleton();
        }

        void CreateSingleton()
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


        private void FillNewWordInWordLeftList()
        {
            List<PickWordDataInfo.PickingDataInfo> pickInfoList =
                GameController.instance.GetPickDataInfo().pickDataInfoList;

            wordsLeftList.Clear();
            //DataHandler.PickDataIndex = 0;
            int index = 10;
            foreach (PickWordDataInfo.PickingDataInfo info in pickInfoList)
            {
                if (info.quesLetterCount == DataHandler.UnlockedQuesLetter)
                {
                    for (var i = DataHandler.PickDataIndex; i < info.wordList.Count; i++)
                    {
                        var s = info.wordList[i];
                        if (index > 0)
                        {
                            DataHandler.PickDataIndex++;
                            wordsLeftList.Add(s.Trim());
                        }

                        index--;

                        if (i == info.wordList.Count - 1 && index >= 0)
                        {
                            i = DataHandler.PickDataIndex = 0;
                        }
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
        }

        private string[] PickDefinedData()
        {
            List<DefinedLevelScriptable.DefinedLevelInfo> definedLevelList = GameController.instance
                .GetDefinedLevelScriptable()
                .definedLevelInfoList;

            for (var index = 0; index < definedLevelList.Count; index++)
            {
                var levelInfo = definedLevelList[index];
                if (levelInfo.gridSize == DataHandler.CurrGridSize)
                {
                    //DataHandler.HelperWordIndex++;
                    DataHandler.CurrDefinedLevel = index;
                    string[] data = levelInfo.gridData.Split('\n');
                    hintAvailList = new List<string>(levelInfo.hintList);
                    return data;
                }
            }

            return null;
        }

        private void ReadAlreadyCreatedGrid()
        {
            Debug.Log("Read Already Created Grid Called !!");
            List<char> readCharList = new List<char>(SaveManager.Instance.state.gridDataList);
            int index = 0;
            gridData = new char[DataHandler.CurrGridSize][];
            for (var i = 0; i < gridData.Length; i++)
            {
                //Debug.Log(" I : " + i);
                gridData[i] = new char[DataHandler.CurrGridSize];

                for (var j = 0; j < gridData[i].Length; j++)
                {
                    //Debug.Log("readCharList[index] : " + readCharList[index]);
                    //Debug.Log(" gridData[i][j] : " + gridData[i][j]);

                    gridData[i][j] = readCharList[index];
                    //Debug.Log("J : " + j);
                    index++;
                }
            }

            //FillWordLeftListRemainingWord();
        }

        private void GetNewGridDataCreated()
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

            if (DataHandler.CurrDefinedLevel <
                GameController.instance.GetDefinedLevelScriptable().definedLevelInfoList.Count)
            {
                string[] line = PickDefinedData();

                if (line != null && line.Length > 0)
                {
                    foreach (string str in line)
                    {
                        char[] charArr = str.ToCharArray();
                        foreach (char c in charArr)
                        {
                            Debug.Log("C : " + c);
                            if (c != '-')
                            {
                                unlockStr += c;
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("Defined Data Empty !");
                }
            }

            if (unlockStr.Length == 0)
            {
                for (var i = 0; i < unlockedGridWord; i++)
                {
                    Debug.Log("Pick Index Val : " + DataHandler.PickDataIndex);

                    tempStr += wordsLeftList[0];

                    if (i != unlockedGridWord - 1)
                    {
                        //Will Not Enter for Last Word !
                        hintAvailList.Add(wordsLeftList[0]);
                        wordsLeftList.RemoveAt(0);
                        //DataHandler.PickDataIndex++;
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

                // if (DataHandler.HelperWordIndex >= GameController.instance.maxHelperIndex)
                // {
                //     unlockStr = ShuffleString(unlockStr, 1);
                // }
                unlockStr = ShuffleString(unlockStr);
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
                //Debug.Log("Loaded Data Str  : " + str);
                wordsLeftList.Add(str);
            }
        }


        private string GetWholeGridData()
        {
            string mainStr = "";

            int totalLetter = unlockedGridList.Count;
            Debug.Log("Total Letter : " + totalLetter);
            int unlockedGridWord = totalLetter / DataHandler.UnlockedQuesLetter + 1;
            Debug.Log("Unlock Words Pick Count : " + unlockedGridWord);
            Debug.Log("Unlock Ques Letter : " + DataHandler.UnlockedQuesLetter);
            hintAvailList.Clear();
            string tempStr = "";

            List<DefinedLevelScriptable.DefinedLevelInfo> definedLevelInfoList = GameController.instance
                .GetDefinedLevelScriptable()
                .definedLevelInfoList;

            if (DataHandler.CurrDefinedLevel <
                definedLevelInfoList.Count)
            {
                Debug.Log("PREDEFINED LEVEL RUNNING !!");
                string[] line = definedLevelInfoList[DataHandler.CurrDefinedLevel].gridData.Split('\n');

                if (line.Length > 0 && definedLevelInfoList[DataHandler.CurrDefinedLevel].gridSize ==
                    DataHandler.CurrGridSize)
                {
                    hintAvailList = new List<string>(definedLevelInfoList[DataHandler.CurrDefinedLevel].hintList);

                    foreach (string str in line)
                    {
                        char[] charArr = str.ToCharArray();
                        foreach (char c in charArr)
                        {
                            Debug.Log("C : " + c);

                            if (c != '-')
                            {
                                mainStr += c;
                            }
                        }
                    }
                }
            }


            if (mainStr.Length == 0)
            {
                hintAvailList.Clear();
                for (var i = 0; i < unlockedGridWord; i++)
                {
                    Debug.Log("Pick Index Val : " + DataHandler.PickDataIndex);

                    tempStr += wordsLeftList[0];
                    if (i != unlockedGridWord - 1)
                    {
                        //Will Not Enter for Last Word !
                        hintAvailList.Add(wordsLeftList[0]);
                        wordsLeftList.RemoveAt(0);
                        //DataHandler.PickDataIndex++;
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

                // if (DataHandler.HelperWordIndex >= GameController.instance.maxHelperIndex)
                // {
                //     mainStr = ShuffleString(mainStr);
                // }

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

            foreach (string word in wordsLeftList)
            {
                bool isWordSatisfying = HasCommonLetters(gridString, word, commonLetters, out string commonString);

                if (isWordSatisfying)
                {
                    gridExistingString = commonString;
                    tempWord = word;
                    break;
                }
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
                    //DataHandler.PickDataIndex++;
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

        string ShuffleString(string concatenatedString)
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
            string data = "";
            int random = Random.Range(0, gridAvailableOnScreenList.Count);
            data = gridAvailableOnScreenList[random].GridTextData;

            string listWord = "";

            //return the word that start with same letter of gridData which we selected .
            foreach (string str in wordsLeftList)
            {
                if (str[0].ToString() == data)
                {
                    listWord = str;
                    Debug.Log("List Word : " + listWord);
                    break;
                }
            }

            Debug.Log("Char in String : " + data);
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

                    if (!inputGridsList.Contains(gridTileObj))
                    {
                        inputGridsList.Add(gridTileObj);
                    }

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

        public bool CheckAllGridBought()
        {
            if (totalBuyingGridList.Count > 0 ||
                DataHandler.CurrGridSize >= GameController.instance.maxGridSize)
                return false;

            _isLevelRunning = false;
            float time = currLevel.timeToWaitForEachGrid;
            float timeToPlaceGrids = currLevel.timeToPlaceGrid;
            StartCoroutine(ReturnToDeck(unlockedGridList, time, timeToPlaceGrids, false));
            StartCoroutine(
                GameController.instance.StartGameAfterCertainTime(timeToPlaceGrids +
                                                                  (time * (totalGridsList.Count + 1))));
            return true;
        }

        public void CheckAnswer()
        {
            _isLevelRunning = false;
            UIManager.instance.CanTouch(false);

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

            Debug.Log("Word : " + word + " Word Length : " + word.Length);
            //bool doWordExist = GameController.instance.Search(word);

            //bool doWordExistInFullDict = englishDictWords.SearchInFullDict(word);
            bool isCommonWord = englishDictWords.CompareWordsInGame(word, out bool doExist);

            if (isCommonWord && doExist)
            {
                Debug.LogError("Word UnCommon Found!");
                UIManager.instance.toastMessageScript.ShowNewWordFoundToast();
            }


            if (doExist)
            {
                selectedWords.Add(word);
                //DataHandler.HelperWordIndex++;

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
                SoundManager.instance.PlaySound(SoundManager.SoundType.CorrectSound);
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

                if (index == 0)
                {
                    time = gridTile.blastTime / 2 + gridTile.blastEffectAfterTime;
                    AddCoin();
                }

                wordCompletedGridList.Add(gridTile);
                gridAvailableOnScreenList.Remove(gridTile);
                gridTile.CallBlastAfterTime();
                gridTile.SetQuesTileStatus(gridTile.placedOnQuesTile, false,
                    time);
            }

            DataHandler.IqLevel++;
            UIManager.instance.iqLevelText.text = $"IQ : {DataHandler.IqLevel.ToString()}";
            StartCoroutine(ResetLevelHandlerData(time, true));
            //RevertQuesData();
            StartCoroutine(DataResetAfterGridAnimation(time));
        }

        private bool CheckGridLeft()
        {
            if (wordCompletedGridList.Count == unlockedGridList.Count)
            {
                foreach (GridTile gridTile in unlockedGridList)
                {
                    gridTile.transform.position = currLevel.BottomOfScreenPoint();
                }

                wordCompletedGridList.Clear();
                return false;
            }

            return true;
        }

        private void GridsBackToPos(bool calledByHint = false)
        {
            Debug.Log("Grid Back To Pos Entered !");
            float time = 0;
            for (var index = 0; index < inputGridsList.Count; index++)
            {
                GridTile gridTile = inputGridsList[index];

                if (index == 0)
                {
                    time = gridTile.reachTime + gridTile.blastEffectAfterTime;
                }

                //inputGridsList.Remove(gridTile);
                gridTile.MoveAfter(gridTile.defaultGridPos, false, gridTile.placedOnQuesTile,
                    gridTile.blastEffectAfterTime);
                gridTile.SetQuesTileStatus(gridTile.placedOnQuesTile, false,
                    time);
            }

            StartCoroutine(LevelRunningStatus(true, time));
            StartCoroutine(DataResetAfterGridAnimation(time, calledByHint));

            Debug.Log("Grid Back To Pos Exited !");
        }

        IEnumerator DataResetAfterGridAnimation(float time, bool isCalledByHint = false)
        {
            yield return new WaitForSeconds(time);
            UIManager.instance.CanTouch(true);
            Debug.Log("Data Reset After Grid Animation !");

            inputGridsList.Clear();
            LastQuesTile = null;

            if (!isCalledByHint)
            {
                RevertQuesData();
            }
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
            string mainStr = "";

            if (shouldReturnToGridPlace)
            {
                DataHandler.CurrDefinedLevel++;
                mainStr = GetWholeGridData();
                Debug.Log("Whole Grid Data Main Str : " + mainStr);
            }

            int index = 0;
            float time = timeToPlaceGrids / gridLists.Count;

            foreach (GridTile gridTile in gridLists)
            {
                yield return new WaitForSeconds(time);

                if (gridTile.isBlastAfterWordComplete)
                {
                    gridTile.ObjectStatus(false);
                }

                string gridString = mainStr.Length > index ? mainStr[index].ToString() : " ";

                gridTile.DeckAnimation(timeToPlaceGrids, pos, gridString, shouldReturnToGridPlace);
                index++;
            }

            if (shouldReturnToGridPlace)
            {
                StartCoroutine(ResetLevelHandlerData(timeToPlaceGrids));
            }

            Invoke(nameof(RevertQuesData), timeToPlaceGrids);

            yield return null;
        }

        public IEnumerator ResetLevelHandlerData(float time, bool doCheckGridLeft = false)
        {
            yield return new WaitForSeconds(time);

            if (doCheckGridLeft)
            {
                Debug.Log("Checking Grid Left !");
                bool isGridLeft = CheckGridLeft();
                Debug.Log("Grid Left Bool : " + isGridLeft);

                if (gridAvailableOnScreenList.Count < DataHandler.CurrTotalQuesSize && isGridLeft)
                {
                    Debug.Log("GRID CHECK IF");
                    GameController.instance.Deal(false);
                }
                else if (isGridLeft)
                {
                    Debug.Log("GRID CHECK ELSE IF");
                    bool isHintAvail = CheckHintStatus(out string finalStr);
                    Debug.Log("IF Is Hint Avail : " + isHintAvail);
                    UIManager.instance.HintStatus(isHintAvail);
                }
                else
                {
                    Debug.Log("GRID CHECK ELSE ");
                    GameController.instance.ShuffleGrid(false);
                }
            }
            else
            {
                Debug.Log("Not Checking Grid Left !");
                bool isHintAvail = CheckHintStatus(out string finalStr);
                Debug.Log("ELSE Is Hint Avail : " + isHintAvail);
                UIManager.instance.HintStatus(isHintAvail);
            }

            LastQuesTile = null;
            inputGridsList.Clear();
            SetLevelRunningBool(true);
        }

        public bool CheckHintStatus(out string passingStr)
        {
            Debug.Log("Check Hint Status Called !");

            List<GridTile> tileList = new List<GridTile>(gridAvailableOnScreenList);

            Debug.Log("Grid Avail On Screen List Count : " + gridAvailableOnScreenList.Count);

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
                    if (hintStr.Length == DataHandler.UnlockedQuesLetter && LettersExistInWord(hintStr, gridString))
                    {
                        passingStr = hintStr;
                        Debug.Log("Hint In Hint Avail List : " + passingStr);
                        return true;
                    }
                }
            }


            passingStr = AnyWordExists();
            if (passingStr.Length > 0)
            {
                Debug.Log("Hint Found In Hint Avail List !");
                return true;
            }

            Debug.Log("Hint Not Found!");
            passingStr = "";
            return false;
        }

        private void FindHint()
        {
            Debug.Log("Find Hint Entered !");
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
            GridsBackToPos(true);
            FindHint();
            UIManager.SetCoinData(GameController.instance.hintUsingCoin, -1);
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

            if (totalRemainingList.Count < DataHandler.CurrTotalQuesSize)
                return finalStr;

            for (int i = 0; i < totalRemainingList.Count; i++)
            {
                List<GridTile> gridsRemainingList = new List<GridTile>(totalRemainingList);
                Debug.Log("GridsRemainingList Count : " + gridsRemainingList.Count);
                string word = gridsRemainingList[i].GridTextData;
                hintTiles.Add(gridsRemainingList[i]);
                Debug.Log("HINT GRID : " + gridsRemainingList[i].name, gridsRemainingList[i].gameObject);
                Debug.Log("Word First Letter Selected : " + word);
                int index = 1;
                List<MainDictionary.WordLengthDetailedInfo> dictWordList =
                    GameController.instance.GetWordListOfLength(count,
                        gridsRemainingList[i].GridTextData);

                Debug.Log("Word List Length : " + dictWordList.Count);
                Debug.Log("Char Selected : " + dictWordList[0].wordStartChar);
                gridsRemainingList.Remove(gridsRemainingList[i]);

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
            Debug.Log("Save Grid Lock System Called !");
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

            Debug.Log("Save Grid Lock System ForEach Completed !");
            SaveManager.Instance.state.gridDataList = new List<char>(gridLockStatusList);
            Debug.Log("Save Grid Lock System ForEach Completed GAP!");
            SaveManager.Instance.UpdateState();
            //SaveManager.Instance.Save();
            Debug.Log("Save Grid Lock System Saved Successfully !");
        }

        private void SaveGridOnScreenSystem()
        {
            Debug.Log("Save Grid On Screen System Called !");
            List<GridTile> totalList = new List<GridTile>(totalGridsList);
            List<bool> gridScreenList = new List<bool>();

            // 0 : Grid Tile is not inside the Screen..
            // 1 : Grid Tile is inside the Screen..
            foreach (var gridTile in totalList)
            {
                gridScreenList.Add(gridTile.isBlastAfterWordComplete);
            }

            Debug.Log("Save Grid On Screen System ForEach Completed !");
            SaveManager.Instance.state.gridOnScreenList = new List<bool>(gridScreenList);
            SaveManager.Instance.UpdateState();
            //SaveManager.Instance.Save();
            Debug.Log("Save Grid On Screen System Saved Successfully !");
        }

        private void SaveHintList()
        {
            Debug.Log("Save Hint List Called !");
            List<string> hintList = new List<string>(hintAvailList);
            SaveManager.Instance.state.hintList = new List<string>(hintList);
            Debug.Log("Save Hint List Assignment Completed !");
            SaveManager.Instance.UpdateState();
            //SaveManager.Instance.Save();
            Debug.Log("Save Hint List System Saved !");
        }

        private void SaveWordLeftList()
        {
            Debug.Log("Save Word Left List Called !");
            List<string> wordList = new List<string>(wordsLeftList);
            SaveManager.Instance.state.wordLeftList = new List<string>(wordList);
            Debug.Log("Save Word Left List Assignment Completed !");
            SaveManager.Instance.UpdateState();
            //SaveManager.Instance.Save();
            Debug.Log("Save Word Left List System Saved !");
        }

        public void SaveSystem()
        {
            Debug.Log("Save System Function Called !");
            SaveGridLockSystem();
            SaveGridOnScreenSystem();
            SaveHintList();
            SaveWordLeftList();
            Debug.Log("Save System Function Completed !");
        }
    }
}