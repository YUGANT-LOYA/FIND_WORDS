using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

namespace YugantLoyaLibrary.FindWords
{
    public class GameController : MonoBehaviour
    {
        public static GameController instance;
        public bool isTesting;
        public int startingGridSize = 5, startingQuesSize = 3;

        [Header("References")] [SerializeField]
        private DifficultyDataInfo difficultyDataInfo;

        [SerializeField] private LevelHandler levelHandler;
        [SerializeField] private Transform levelContainer;
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        public Transform coinContainerTran;
        public int coinPoolSize = 20;
        private Level _currLevel;
        private bool _isRestarting;
        [SerializeField] private float timeToSwitchToNextLevel = 1f;

        public enum CurrDifficulty
        {
            VERY_EASY,
            EASY,
            MODERATE,
            HARD,
            EXPERT
        }

        public CurrDifficulty currDiff = CurrDifficulty.VERY_EASY;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            CreateSingleton();
        }

        private void Start()
        {
            GameStartInfo();
            StartGame();
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
            levelHandler.LevelStartInit();
            //_currLevel.gridSize = GetLevelDataInfo().gridSize;
            _currLevel.gridSize = new Vector2Int(DataHandler.instance.CurrGridSize, DataHandler.instance.CurrGridSize);
            levelHandler.GetGridData();
            _currLevel.StartInit();
        }

        public DifficultyDataInfo.DifficultyInfo GetDifficultyInfo()
        {
            //Debug.Log("Curr Level Num : " + DataHandler.instance.CurrLevelNumber);
            return difficultyDataInfo.difficultyInfos[DataHandler.instance.CurrDifficultyNumber];
        }

        public DifficultyDataInfo GetDifficultyDataInfo()
        {
            return difficultyDataInfo;
        }

        void GameStartInfo()
        {
            DataHandler.instance.CurrDifficultyNumber = 0;
            UIManager.instance.coinText.text = DataHandler.instance.initialCoins.ToString();
            DataHandler.instance.TotalCoin = DataHandler.instance.initialCoins;
            DataHandler.instance.CurrGridSize = startingGridSize;
            levelHandler.englishDictWords.UpdateEnglishDict();
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

        public IEnumerator StartGameAfterCertainTime(float time)
        {
            Debug.Log($"Game Restarting After {time}");
            yield return new WaitForSeconds(time);
            StartGame();
        }

        private void StartGame()
        {
            ResetData();

            UIManager.instance.coinText.text = DataHandler.instance.TotalCoin.ToString();

            if (!isTesting)
            {
                ClearContainer(levelContainer);
                CreateLevel();
            }
            else
            {
                Level level = FindObjectOfType<Level>();

                if (level != null)
                {
                    ClearContainer(level.GetGridContainerTrans());
                    _currLevel = level;
                    AssignLevelData();
                }
            }
        }

        public void RestartLevel()
        {
            // if (_isRestarting)
            //     return;

            StartGame();
            // _isRestarting = true;
            // RotateGridContainer();
        }

        private void RotateGridContainer()
        {
            Transform trans = _currLevel.rotationContainer;
            levelHandler.SetLevelRunningBool(false);
            Transform gridContainer = _currLevel.GetGridContainerTrans();
            if (_currLevel.gridSize.y == _currLevel.gridSize.x)
            {
                Debug.Log("Rotate If !");
                trans.DOScale(0.8f, levelHandler.timeToRotateGrid / 2).OnComplete(() =>
                {
                    trans.DOScale(1f, levelHandler.timeToRotateGrid / 2);
                });

                Quaternion localRotation = trans.localRotation;
                trans.DORotate(
                    new Vector3(localRotation.eulerAngles.x, localRotation.eulerAngles.y,
                        localRotation.eulerAngles.z + 90f), levelHandler.timeToRotateGrid).OnComplete(() =>
                {
                    _isRestarting = false;
                    levelHandler.SetLevelRunningBool(true);
                });

                for (int i = 0; i < gridContainer.childCount; i++)
                {
                    GameObject gm = gridContainer.GetChild(i).gameObject;
                    Quaternion rotation = gm.transform.localRotation;
                    gm.transform.DOLocalRotate(
                        new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z - 90f),
                        levelHandler.timeToRotateGrid);
                }
            }
            else
            {
                Debug.Log("Rotate Else !");

                trans.DOScale(0.8f, levelHandler.timeToRotateGrid).OnComplete(() =>
                {
                    trans.DOScale(1f, levelHandler.timeToRotateGrid);
                });

                Quaternion localRotation = trans.localRotation;
                trans.DORotate(
                    new Vector3(localRotation.eulerAngles.x, localRotation.eulerAngles.y,
                        localRotation.eulerAngles.z + 180f), levelHandler.timeToRotateGrid * 2).OnComplete(() =>
                {
                    _isRestarting = false;
                    levelHandler.SetLevelRunningBool(true);
                });

                for (int i = 0; i < gridContainer.childCount; i++)
                {
                    GameObject gm = gridContainer.GetChild(i).gameObject;
                    Quaternion rotation = gm.transform.localRotation;
                    gm.transform.DOLocalRotate(
                        new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z - 180f),
                        levelHandler.timeToRotateGrid * 2);
                }
            }
        }

        void ResetData()
        {
            levelHandler.inputGridsList.Clear();
            levelHandler.totalGridsList.Clear();
            levelHandler.quesTileList.Clear();
            levelHandler.buyGridList.Clear();
            levelHandler.wordCompletedGridList.Clear();
        }

        void ResetInGameData()
        {
            levelHandler.inputGridsList.Clear();
            levelHandler.wordCompletedGridList.Clear();
        }

        public void ShuffleGrid()
        {
            levelHandler.SetLevelRunningBool(false);
            ResetInGameData();
            float time = _currLevel.timeToWaitForEachGrid;
            float timeToPlaceGrids = _currLevel.timeToPlaceGrid;
            List<GridTile> list = new List<GridTile>(levelHandler.unlockedGridList);
            StartCoroutine(levelHandler.ReturnToDeck(list, time, timeToPlaceGrids));
            levelHandler.RevertQuesData();
        }

        public void Deal()
        {
            if (levelHandler.wordCompletedGridList.Count > 0)
            {
                List<GridTile> list = new List<GridTile>(levelHandler.wordCompletedGridList);
                StartCoroutine(BackToDeckAnim(list));
            }
        }

        public void Hint()
        {
            levelHandler.ShowHint();
        }

        IEnumerator BackToDeckAnim(List<GridTile> list)
        {
            foreach (GridTile gridTile in list)
            {
                yield return new WaitForSeconds(_currLevel.timeToWaitForEachGrid);
                Debug.Log("Moving To Grid Place Again !");
                gridTile.isBlastAfterWordComplete = false;
                levelHandler.UpdateTextDataForGrid(gridTile);
                gridTile.MoveTowardsGrid();
                levelHandler.wordCompletedGridList.Remove(gridTile);
            }

            StartCoroutine(ResetLevelHandlerData(_currLevel.timeToWaitForEachGrid * list.Count));
        }

        IEnumerator ResetLevelHandlerData(float time)
        {
            yield return new WaitForSeconds(time);
            levelHandler.LastQuesTile = null;
            levelHandler.SetLevelRunningBool(true);
        }

        public void NextLevel()
        {
            if (DataHandler.instance.CurrDifficultyNumber < difficultyDataInfo.difficultyInfos.Count - 1)
            {
                DataHandler.instance.CurrDifficultyNumber++;
            }
            else
            {
                DataHandler.instance.CurrDifficultyNumber = 0;
            }

            StartCoroutine(nameof(FadeScreen));
            UIManager.instance.winPanel.SetActive(false);
        }

        public void PreviousLevel()
        {
            if (DataHandler.instance.CurrDifficultyNumber > 0)
            {
                DataHandler.instance.CurrDifficultyNumber--;
            }
            else
            {
                DataHandler.instance.CurrDifficultyNumber = difficultyDataInfo.difficultyInfos.Count - 1;
            }

            StartCoroutine(nameof(FadeScreen));
        }

        void FadeScreen()
        {
            fadeCanvasGroup.DOFade(1f, timeToSwitchToNextLevel / 2f).OnComplete(() =>
            {
                StartGame();
                fadeCanvasGroup.DOFade(0f, timeToSwitchToNextLevel / 2f).OnComplete(() => { });
            });
        }
    }
}