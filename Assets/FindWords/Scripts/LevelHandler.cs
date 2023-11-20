using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace YugantLoyaLibrary.FindWords
{
    public class LevelHandler : MonoBehaviour
    {
        public static LevelHandler Instance;

        public delegate void NewLetterDelegate(GridTile gridTile);

        public NewLetterDelegate OnNewLetterAddEvent, OnRemoveLetterEvent;

        public GameObject boxContainer;
        public EnglishDictWords englishDictWords;
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
            rentGridList = new List<GridTile>(),
            gridAvailableOnScreenList = new List<GridTile>(),
            totalBuyingGridList = new List<GridTile>();

        public List<QuesTile> quesTileList = new List<QuesTile>();

        public List<string> selectedWords = new List<string>(),
            wordsLeftList = new List<string>();

        private bool _isLevelRunning = true;
        private int _colorIndex;
        private QuesTile _lastQuesTile;
        public List<string> hintAvailList = new List<string>();
        [HideInInspector] public bool isHintAvailInButton;
        public string quesHintStr;
        private int _correctWordCompleteInLine, _nextCommentAfter, _wrongClickCount;


        private void OnEnable()
        {
            OnNewLetterAddEvent += AddNewLetter;
            OnRemoveLetterEvent += RemoveLetter;
        }

        private void OnDisable()
        {
            OnNewLetterAddEvent -= AddNewLetter;
            OnRemoveLetterEvent -= RemoveLetter;
        }

        private void Awake()
        {
            CreateSingleton();
        }

        void CreateSingleton()
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
                //Debug.Log("New Data Created !!");
                FillNewWordInWordLeftList();
                GetNewGridDataCreated();
            }
            else
            {
                //Debug.Log("Loaded Previous Data !!");
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
            //Debug.Log("Read Already Created Grid Called !!");
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
        }

        private void GetNewGridDataCreated()
        {
            int row = currLevel.gridSize.x;
            int col = currLevel.gridSize.y;

            //Debug.Log("Total Word To Find Q : " + DataHandler.UnlockedQuesLetter);

            string unlockStr = "";
            //Debug.Log("Unlock String Count : " + unlockStr.Length);
            int totalLetter = (row - 1) * (col - 1);
            //Debug.Log("Total Letter : " + totalLetter);
            int unlockedGridWord = totalLetter / DataHandler.UnlockedQuesLetter + 1;
            //Debug.Log("Unlock Words Pick Count : " + unlockedGridWord);

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
                            //Debug.Log("C : " + c);
                            if (c != '-')
                            {
                                unlockStr += c;
                            }
                        }
                    }
                }
                else
                {
                    // Debug.LogError("Defined Data Empty !");
                }
            }

            if (unlockStr.Length == 0)
            {
                for (var i = 0; i < unlockedGridWord; i++)
                {
                    //Debug.Log("Pick Index Val : " + DataHandler.PickDataIndex);

                    if (wordsLeftList.Count <= 0)
                    {
                        FillNewWordInWordLeftList();
                    }

                    tempStr += wordsLeftList[0];

                    if (i != unlockedGridWord - 1)
                    {
                        //Will Not Enter for Last Word !
                        hintAvailList.Add(wordsLeftList[0]);
                        wordsLeftList.RemoveAt(0);
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

                unlockStr = ShuffleString(unlockStr);
            }


            //Debug.Log("Unlock String Data " + unlockStr);

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
                        //Debug.Log("Grid Data : " + unlockStr[unlockIndex]);
                        gridData[i][j] = unlockStr[unlockIndex];
                        unlockIndex++;
                    }

                    //Debug.Log("Total Index : " + totalIndex);
                    totalIndex++;
                }
            }
        }

        private void LoadWordLeftList()
        {
            //Debug.Log("Loaded Word Left List Called !!");
            if (SaveManager.Instance.state.wordLeftList.Count <= 0)
                return;

            //Debug.Log("Loaded Word Left List ForEach Called !!");
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
            //Debug.Log("Total Letter : " + totalLetter);
            int unlockedGridWord = totalLetter / DataHandler.UnlockedQuesLetter + 1;
            //Debug.Log("Unlock Words Pick Count : " + unlockedGridWord);
            //Debug.Log("Unlock Ques Letter : " + DataHandler.UnlockedQuesLetter);
            hintAvailList.Clear();
            string tempStr = "";

            List<DefinedLevelScriptable.DefinedLevelInfo> definedLevelInfoList = GameController.instance
                .GetDefinedLevelScriptable()
                .definedLevelInfoList;

            if (DataHandler.CurrDefinedLevel <
                definedLevelInfoList.Count)
            {
                //Debug.Log("PREDEFINED LEVEL RUNNING !!");
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
                            //Debug.Log("C : " + c);

                            if (c != '-')
                            {
                                mainStr += c;
                            }
                        }
                    }
                }
            }


            if (mainStr.Length < unlockedGridList.Count)
            {
                hintAvailList.Clear();
                for (var i = 0; i < unlockedGridWord; i++)
                {
                    //Debug.Log("Pick Index Val : " + DataHandler.PickDataIndex);

                    if (wordsLeftList.Count <= 0)
                    {
                        FillNewWordInWordLeftList();
                    }

                    tempStr += wordsLeftList[0];
                    Debug.Log("Temp Str : " + tempStr);
                    
                    if (i != unlockedGridWord - 1)
                    {
                        Debug.Log("Hint Added To List !" );
                        //Will Not Enter for Last Word !
                        hintAvailList.Add(wordsLeftList[0]);
                        wordsLeftList.RemoveAt(0);
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

                //Debug.Log("Temp Str " + tempStr);
                //Debug.Log("Unlock String Data " + mainStr);

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
                        //Debug.Log("Common String : " + commonString);
                        return true;
                    }
                }
            }

            return false;
        }

        public string GetDataAccordingToGrid(int totalLetters)
        {
            string mainStr = "";
            //Debug.Log("Unlock String Count : " + mainStr.Length);
            //Debug.Log("Total Letter : " + totalLetters);
            int unlockedGridWord = totalLetters / DataHandler.UnlockedQuesLetter + 1;
            //Debug.Log("Unlock Words Pick Count : " + unlockedGridWord);

            string tempStr = "";


            for (var i = 0; i < unlockedGridWord; i++)
            {
                //Debug.Log("Pick Index Val : " + DataHandler.PickDataIndex);

                if (wordsLeftList.Count <= 0)
                {
                    FillNewWordInWordLeftList();
                }

                tempStr += wordsLeftList[0];

                if (i != unlockedGridWord - 1)
                {
                    //Will Not Enter for Last Word !
                    hintAvailList.Add(wordsLeftList[0]);
                    wordsLeftList.RemoveAt(0);
                }
            }

            for (int i = 0; i < tempStr.Trim().Length; i++)
            {
                if (totalLetters >= 0)
                {
                    mainStr += tempStr[i];
                    totalLetters--;
                }
            }

            //Debug.Log("Unlock String Data " + mainStr);
            mainStr = ShuffleString(mainStr);

            //Debug.Log("Unlock String Data " + mainStr);

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
                    //Debug.Log("List Word : " + listWord);
                    break;
                }
            }

            //Debug.Log("Char in String : " + data);
            char ch = NonExistingLetterInGrid(listWord);
            //Debug.Log("Char Found !" + ch);
            return ch;
        }

        char NonExistingLetterInGrid(string word, int index = -1)
        {
            char mainChar = ' ';
            //Debug.Log("List word In Non Existing Func : " + word);
            for (var i = 0; i < word.Length; i++)
            {
                var ch = word[i];
                bool isLetterThere = false;
                foreach (GridTile tile in gridAvailableOnScreenList)
                {
                    //Debug.Log("CH : " + ch);
                    // Debug.Log("Tile Data : " + tile.GridTextData);

                    if (ch.ToString() == tile.GridTextData)
                    {
                        isLetterThere = true;
                        break;
                    }
                }

                if (!isLetterThere)
                {
                    //Debug.Log("CH : " + ch);

                    if (i == word.Length - 1)
                    {
                        hintAvailList.Add(word);
                    }
                    else
                    {
                        bool isWordThere = CheckIfWholeWordExistsInGrids(word, ch);
                        //Debug.Log("IS WORD THERE : " + isWordThere);

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
                //Debug.Log("EMPTY CHAR ");

                if (wordsLeftList.Count <= 0)
                {
                    //Debug.Log("Word Left List Empty !");
                    FillNewWordInWordLeftList();
                }

                index++;

                //In Case Index Increased to 5, we will print random Letter in the Box.
                if (index > 5 || wordsLeftList.Count <= index)
                {
                    //Debug.Log("Non Existing Letter Index Random Vowel Printed !!");
                    char ch = GameController.RandomVowel();
                    return ch;
                }

                //Debug.Log("Non Existing Index : " + index + " word Count : " + wordsLeftList.Count);

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
                //Debug.Log($"Letter Not Same {word} , {textWord} , {index} ");
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
                    //Debug.Log("Ques Selected To Move : " + quesTileList[index]);
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
                gridTileObj.MoveAfter(gridTileObj.defaultGlobalGridPos, false, gridTileObj.placedOnQuesTile);
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

        public void UpdateQuesList(QuesTile quesTileScript, int id)
        {
            quesTileScript.id = id;

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

            DisableWordCompleteGameObject();
            StartCoroutine(ReturnToDeck(unlockedGridList, time, timeToPlaceGrids, false));
            StartCoroutine(
                GameController.instance.StartGameAfterCertainTime(timeToPlaceGrids +
                                                                  (time * (totalGridsList.Count + 1))));
            return true;
        }

        public void CheckAnswer()
        {
            _isLevelRunning = false;
            UIManager.Instance.CanTouch(false);

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

            //Debug.Log("Word : " + word + " Word Length : " + word.Length);
            //bool doWordExist = GameController.instance.Search(word);
            bool isUnCommonWordFound = englishDictWords.CompareWordsInGame(word, out bool doExist);

            if (isUnCommonWordFound && doExist)
            {
                //Debug.Log("Word UnCommon Found!");
                UIManager.Instance.toastMessageScript.ShowNewWordFoundToast();
                SoundManager.instance.PlaySound(SoundManager.SoundType.NewWordFound);
            }


            if (doExist)
            {
                selectedWords.Add(word);
                //DataHandler.HelperWordIndex++;

                string temp = word.Trim().ToLower();

                if (wordsLeftList.Contains(temp))
                {
                    // Debug.Log("Word Removed From Word Left List !");
                    wordsLeftList.Remove(temp);
                }

                if (hintAvailList.Contains(temp))
                {
                    hintAvailList.Remove(temp);
                }

                WordComplete(isUnCommonWordFound);
                quesHintStr = null;

                if (!isUnCommonWordFound)
                {
                    SoundManager.instance.PlaySound(SoundManager.SoundType.Correct);
                }

                _wrongClickCount = 0;
                _correctWordCompleteInLine++;
                _nextCommentAfter--;

                if (_correctWordCompleteInLine >= 3 && _nextCommentAfter <= 0)
                {
                    _nextCommentAfter = Random.Range(2, 4);
                    GameController.instance.wordCommentScript.PickRandomCommentForWordComplete(
                        _correctWordCompleteInLine);
                }
            }
            else
            {
                Vibration.Vibrate(25);
                SoundManager.instance.PlaySound(SoundManager.SoundType.Wrong);
                GridsBackToPos();
                UIManager.Instance.ShakeCam();
                UIManager.Instance.WrongEffect();
                _correctWordCompleteInLine = 0;
                _wrongClickCount++;
            }
        }

        private void AddCoin(int extraCoin = 0)
        {
            UIManager.Instance.CoinCollectionAnimation(coinPerWord + extraCoin);
            //Debug.Log("Coin Per Word To Add: " + coinPerWord);
        }

        void WordComplete(bool isCommonWord)
        {
            float time = 0;
            for (int index = 0; index < inputGridsList.Count; index++)
            {
                GridTile gridTile = inputGridsList[index];

                if (index == 0)
                {
                    time = gridTile.blastTime / 2 + gridTile.blastEffectAfterTime;

                    if (isCommonWord)
                    {
                        AddCoin(5);
                    }
                    else
                    {
                        AddCoin();
                    }
                }

                wordCompletedGridList.Add(gridTile);
                gridAvailableOnScreenList.Remove(gridTile);
                gridTile.CallBlastAfterTime();
                gridTile.SetQuesTileStatus(gridTile.placedOnQuesTile, false,
                    time);
            }

            CheckIqExpLevel();

            StartCoroutine(ResetLevelHandlerData(time, true));
            StartCoroutine(DataResetAfterGridAnimation(time));
        }

        private void CheckIqExpLevel()
        {
            DataHandler.WordCompleteNum++;
            int unlockNextIqExp = 5;
            float oneWordBarVal = 1f / unlockNextIqExp;
            Slider iqSlider = UIManager.Instance.iqSlider;
            DataHandler.IqBarVal = iqSlider.value + oneWordBarVal;
            iqSlider.DOValue(DataHandler.IqBarVal, 0.5f).OnComplete(
                () =>
                {
                    float wordLeft = DataHandler.WordCompleteNum % unlockNextIqExp;
                    iqSlider.value = wordLeft / unlockNextIqExp;

                    if (wordLeft != 0) return;

                    DataHandler.IqExpLevel++;
                    UIManager.Instance.iqExperienceText.text = $"{DataHandler.IqExpLevel.ToString()}";
                    UIManager.Instance.iqSlider.value = 0f;
                    DataHandler.IqBarVal = 0;
                });
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
            //Debug.Log("Grid Back To Pos Entered !");
            float time = 0;
            for (var index = 0; index < inputGridsList.Count; index++)
            {
                GridTile gridTile = inputGridsList[index];

                if (index == 0)
                {
                    time = gridTile.reachTime + gridTile.blastEffectAfterTime;
                }

                gridTile.MoveAfter(gridTile.defaultGlobalGridPos, false, gridTile.placedOnQuesTile,
                    gridTile.blastEffectAfterTime);

                if (!calledByHint)
                {
                    gridTile.SetQuesTileStatus(gridTile.placedOnQuesTile, false,
                        time);
                }
            }

            StartCoroutine(LevelRunningStatus(true, time));
            StartCoroutine(DataResetAfterGridAnimation(time, calledByHint));

            //Debug.Log("Grid Back To Pos Exited !");
        }

        IEnumerator DataResetAfterGridAnimation(float time, bool isCalledByHint = false)
        {
            yield return new WaitForSeconds(time);
            UIManager.Instance.CanTouch(true);
            //Debug.Log("Data Reset After Grid Animation !");

            inputGridsList.Clear();
            LastQuesTile = null;

            if (!isCalledByHint && string.IsNullOrEmpty(quesHintStr))
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

        public void DisableWordCompleteGameObject()
        {
            foreach (GridTile tile in wordCompletedGridList)
            {
                tile.ObjectStatus(false);
            }
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
                //Debug.Log("Whole Grid Data Main Str : " + mainStr);

                //Debug.Log("Should Return To Grid Place Entered !");
                StartCoroutine(ResetLevelHandlerData(timeToPlaceGrids + 0.3f));
            }

            Invoke(nameof(RevertQuesData), timeToPlaceGrids - 0.1f);

            int index = 0, lockIndex = 0;
            float time = timeToPlaceGrids / gridLists.Count;
            List<GridTile> newUnlockGridList = new List<GridTile>();

            foreach (GridTile gridTile in gridLists)
            {
                if (lockedGridList.Contains(gridTile) && gridTile.isGridActive)
                {
                    newUnlockGridList.Add(gridTile);
                }
            }

            string newUnlockString = GetDataAccordingToGrid(newUnlockGridList.Count);

            for (var i = 0; i < gridLists.Count; i++)
            {
                var gridTile = gridLists[i];
                yield return new WaitForSeconds(time);

                // if (gridTile.isBlastAfterWordComplete)
                // {
                //     gridTile.ObjectStatus(false);
                // }

                if (lockedGridList.Contains(gridTile) && !gridTile.isGridActive)
                {
                    newUnlockGridList[i].DeckAnimation(timeToPlaceGrids, pos, newUnlockString[lockIndex].ToString(),
                        shouldReturnToGridPlace);
                    lockIndex++;
                }
                else
                {
                    string gridString = mainStr.Length > index
                        ? mainStr[index].ToString()
                        : "";

                    gridTile.DeckAnimation(timeToPlaceGrids, pos, gridString, shouldReturnToGridPlace);
                }

                index++;
            }

            yield return null;
        }

        private IEnumerator ResetLevelHandlerData(float time, bool doCheckGridLeft = false)
        {
            yield return new WaitForSeconds(time);

            //Debug.Log("Do Check Grid Left : " + doCheckGridLeft);

            if (doCheckGridLeft)
            {
                //Debug.Log("Checking Grid Left !");
                bool isGridLeft = CheckGridLeft();
                //Debug.Log("Grid Left Bool : " + isGridLeft);

                if (gridAvailableOnScreenList.Count < DataHandler.CurrTotalQuesSize && isGridLeft)
                {
                    //Debug.Log("GRID CHECK IF");
                    GameController.instance.Deal(false);
                }
                else if (isGridLeft)
                {
                    //Debug.Log("GRID CHECK ELSE IF");
                    CheckWordExistOrNot(out bool hintButtonStatus, out string hintStr);
                    //Debug.Log("IF Is Hint Avail : " + hintButtonStatus);
                }
                else
                {
                    //Debug.Log("GRID CHECK ELSE ");
                    GameController.instance.ShuffleGrid(false);
                }
            }
            
            // else
            // {
            //     //Debug.Log("Not Checking Grid Left !");
            //     CheckWordExistOrNot(out bool hintButtonStatus, out string hintStr);
            //     //Debug.Log("ELSE Is Hint Avail : " + hintButtonStatus);
            // }

            LastQuesTile = null;
            inputGridsList.Clear();
            SetLevelRunningBool(true);
        }

        public bool CheckHintStatus(out string passingStr)
        {
            //Debug.Log("Check Hint Status Called !");

            List<GridTile> tileList = new List<GridTile>(gridAvailableOnScreenList);

            //Debug.Log("Grid Avail On Screen List Count : " + gridAvailableOnScreenList.Count);

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

            Debug.Log("Hint Passing String : " + passingStr);

            if (passingStr.Length > 0)
            {
                //Debug.Log("Hint Found In Hint Avail List !");
                //Debug.Log("Data Handler Curr Hint : " + passingStr);
                DataHandler.CurrHint = passingStr;
                return true;
            }

            Debug.Log("Hint Not Found!");
            passingStr = "";
            return false;
        }

        bool CheckAllLetterInGrid()
        {
            List<GridTile> list = new List<GridTile>(gridAvailableOnScreenList);
            bool isHintWordThere = true;
            foreach (char c in DataHandler.CurrHint)
            {
                bool isCThere = false;
                foreach (GridTile tile in list)
                {
                    if (tile.GridTextData == c.ToString())
                    {
                        isCThere = true;
                        list.Remove(tile);
                    }
                }

                if (!isCThere)
                {
                    isHintWordThere = false;
                    break;
                }
            }

            Debug.Log("Is Hint is Avail is Grids : " + isHintWordThere);
            return isHintWordThere;
        }


        private void FindHint()
        {
            Debug.Log("Find Hint Entered !");
            CheckHintStatus(out string finalStr);

            Debug.Log($"Hint Str : {finalStr}, Hint Str Length :  {finalStr.Length}");

            int count = DataHandler.UnlockedQuesLetter;

            if (finalStr.Length == count)
            {
                quesHintStr = finalStr;
            }
        }

        public bool CheckWordExistOrNot(out bool hintButtonStatus, out string hintStr)
        {
            // if (CheckAllLetterInGrid())
            // {
            //     Debug.Log("Hint Already Exists !!");
            //     hintButtonStatus = UIManager.instance.HintStatus(true);
            //     hintStr = DataHandler.CurrHint;
            //     return true;
            // }


            bool isHintAvail = CheckHintStatus(out string finalStr);
            hintButtonStatus = UIManager.Instance.HintStatus(isHintAvail);
            hintStr = finalStr;

            if (!isHintAvail)
            {
                UIManager.Instance.EnableHint();
            }

            return isHintAvail;
        }

        public void ShowHint()
        {
            GridsBackToPos(true);

            if (inputGridsList.Count > 0)
            {
                SetLevelRunningBool(false);
            }

            if (!string.IsNullOrEmpty(quesHintStr) /*|| !string.IsNullOrEmpty(DataHandler.CurrHint)*/)
            {
                FindHint();
            }

            int index = 0;
            foreach (QuesTile quesTile in quesTileList)
            {
                if (quesTile.isUnlocked)
                {
                    quesTile.QuesTextData = quesHintStr[index].ToString().ToUpper();
                    index++;
                }
            }

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
            StringBuilder finalStr = new StringBuilder("");

            if (totalRemainingList.Count < DataHandler.CurrTotalQuesSize)
                return finalStr.ToString();

            for (int i = 0; i < totalRemainingList.Count; i++)
            {
                List<GridTile> gridsRemainingList = new List<GridTile>(totalRemainingList);
                //Debug.Log("GridsRemainingList Count : " + gridsRemainingList.Count);
                StringBuilder word = new StringBuilder(gridsRemainingList[i].GridTextData);
                hintTiles.Add(gridsRemainingList[i]);
                //Debug.Log("HINT GRID : " + gridsRemainingList[i].name, gridsRemainingList[i].gameObject);
                //Debug.Log("Word First Letter Selected : " + word);
                int index = 1;
                List<MainDictionary.WordLengthDetailedInfo> dictWordList =
                    GameController.instance.GetWordListOfLength(count,
                        gridsRemainingList[i].GridTextData);

                Debug.Log("Word List Length : " + dictWordList.Count);
                gridsRemainingList.Remove(gridsRemainingList[i]);

                for (int j = 0; j < dictWordList.Count; j++)
                {
                    //Debug.Log("Char Selected : " + dictWordList[i].wordStartChar);
                    if (dictWordList[j].wordStartChar.ToString() == word.ToString())
                    {
                        List<string> wordList = dictWordList[j].wordList;
                        Debug.Log("LINE COUNT : " + wordList.Count);

                        if (wordList.Count <= 0)
                            continue;

                        foreach (string str in wordList)
                        {
                            //str = Each Word in Text File
                            if (!string.IsNullOrWhiteSpace(str))
                            {
                                word = new StringBuilder(str[0].ToString());
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
                                    //Debug.Log("Word Found in List");
                                    return finalStr.ToString();
                                }
                            }
                        }

                        Debug.Log("Word : " + word + " Index : " + index);

                        if (index < wordList.Count)
                        {
                            Debug.Log("Word Append Letter : " + wordList[index]);
                            word.Append(wordList[index]);
                        }
                    }
                }

                //Debug.Log("Word Append Letter : " + wordList[index]);
                hintTiles.Clear();
            }

            Debug.Log("Hint Does Not Exists !");

            return finalStr.ToString();
        }


        bool FindWordThroughCharacter(int index, StringBuilder formedWord, string word,
            List<GridTile> gridRemainingList,
            List<GridTile> hintTileList,
            out StringBuilder finalStr)
        {
            //Debug.Log("Grid Remaining Count : " + gridRemainingList.Count);
            Debug.Log("Formed Word : " + formedWord);
            foreach (GridTile gridTile in gridRemainingList)
            {
                //Debug.Log("GRID NAME : " + gridTile.gameObject.name, gridTile.gameObject);
                StringBuilder tempWord = new StringBuilder(formedWord + gridTile.GridTextData);
                StringBuilder subStringWord = new StringBuilder(word.Substring(0, index + 1));
                Debug.Log("Sub String : " + subStringWord + "  " + subStringWord.Length);
                Debug.Log("Temp String : " + tempWord + "  " + tempWord.Length);

                if (tempWord.ToString() != subStringWord.ToString()) continue;


                Debug.Log("Word Formed : " + tempWord);

                if (tempWord.ToString() == subStringWord.ToString() &&
                    tempWord.Length == DataHandler.UnlockedQuesLetter)
                {
                    hintTileList.Add(gridTile);
                    Debug.Log($"Matching Word {tempWord}!!");
                    finalStr = tempWord;
                    //Debug.Log("Hint List Count : " + hintTileList.Count);

                    // foreach (GridTile tile in hintTileList)
                    // {
                    //     GameObject obj = tile.gameObject;
                    //     //Debug.Log("HINT GRID : " + obj.name, obj);
                    // }

                    return true;
                }

                hintTileList.Add(gridTile);
                gridRemainingList.Remove(gridTile);
                index++;
                return FindWordThroughCharacter(index, tempWord, word, gridRemainingList, hintTileList,
                    out finalStr);
            }

            hintTileList.Clear();
            finalStr = new StringBuilder("");
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

                //Debug.Log(" Unlock Next Grid Coin Called for " + gridTile.gameObject.name);
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
            rentGridList.Clear();
        }

        public void ClearInGameList()
        {
            foreach (GridTile gridTile in unlockedGridList)
            {
                gridTile.ResetWholeData();
            }

            inputGridsList.Clear();
            wordCompletedGridList.Clear();
            hintAvailList.Clear();
        }

        private void SaveGridLockSystem()
        {
            //Debug.Log("Save Grid Lock System Called !");
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

            // Debug.Log("Save Grid Lock System ForEach Completed !");
            SaveManager.Instance.state.gridDataList = new List<char>(gridLockStatusList);
            // Debug.Log("Save Grid Lock System ForEach Completed GAP!");
            SaveManager.Instance.UpdateState();
            //SaveManager.Instance.Save();
            //Debug.Log("Save Grid Lock System Saved Successfully !");
        }

        private void SaveGridOnScreenSystem()
        {
            //Debug.Log("Save Grid On Screen System Called !");
            List<GridTile> totalList = new List<GridTile>(totalGridsList);
            List<bool> gridScreenList = new List<bool>();

            // 0 : Grid Tile is not inside the Screen..
            // 1 : Grid Tile is inside the Screen..
            foreach (var gridTile in totalList)
            {
                gridScreenList.Add(gridTile.isBlastAfterWordComplete);
            }

            //Debug.Log("Save Grid On Screen System ForEach Completed !");
            SaveManager.Instance.state.gridOnScreenList = new List<bool>(gridScreenList);
            SaveManager.Instance.UpdateState();
            //SaveManager.Instance.Save();
            //Debug.Log("Save Grid On Screen System Saved Successfully !");
        }

        private void SaveHintList()
        {
            //Debug.Log("Save Hint List Called !");
            List<string> hintList = new List<string>(hintAvailList);
            SaveManager.Instance.state.hintList = new List<string>(hintList);
            //Debug.Log("Save Hint List Assignment Completed !");
            SaveManager.Instance.UpdateState();
            //SaveManager.Instance.Save();
            //Debug.Log("Save Hint List System Saved !");
        }

        private void SaveWordLeftList()
        {
            //Debug.Log("Save Word Left List Called !");
            List<string> wordList = new List<string>(wordsLeftList);
            SaveManager.Instance.state.wordLeftList = new List<string>(wordList);
            //Debug.Log("Save Word Left List Assignment Completed !");
            SaveManager.Instance.UpdateState();
            //SaveManager.Instance.Save();
            //Debug.Log("Save Word Left List System Saved !");
        }

        public void SaveSystem()
        {
            //Debug.Log("Save System Function Called !");
            SaveGridLockSystem();
            SaveGridOnScreenSystem();
            SaveHintList();
            SaveWordLeftList();
            //Debug.Log("Save System Function Completed !");
        }
    }
}