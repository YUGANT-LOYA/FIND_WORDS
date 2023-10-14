using System.Collections.Generic;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    public class DataHandler : MonoBehaviour
    {
        public static DataHandler instance;

        LevelHandler _levelHandler;
        private DifficultyDataInfo _diffDataInfo;
        private DefinedLevelScriptable.DefinedLevelInfo _definedLevelInfo;
        [Header("Prefab Holders")] public GameObject coinPrefab;
        public GameObject levelPrefab, gridPrefab, quesPrefab;

        [Header("Data Info")] [Tooltip("When player first time start playing, Initial coins player have")]
        public int initialCoins = 300;
        private static int _initCoins;
        
        private void Awake()
        {
            CreateSingleton();
            Init();
        }

        void CreateSingleton()
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

        void Init()
        {
            int poolSize = GameController.instance.coinPoolSize;
            _initCoins = initialCoins;
            for (int i = 0; i < poolSize; i++)
            {
                GameObject coin = Instantiate(coinPrefab, GameController.instance.coinContainerTran);
                coin.transform.localScale = Vector3.zero;
                coin.gameObject.SetActive(false);
            }

            _levelHandler = GameController.instance.GetLevelHandler();
            _diffDataInfo = GameController.instance.GetDifficultyDataInfo();
        }

        public GameObject GetCoin()
        {
            Transform tran = GameController.instance.coinContainerTran;

            foreach (Transform coinTran in tran)
            {
                if (!coinTran.gameObject.activeInHierarchy)
                {
                    return coinTran.gameObject;
                }
            }

            return null;
        }

        public void ResetCoin(GameObject coin)
        {
            coin.gameObject.SetActive(false);
            coin.transform.SetParent(GameController.instance.coinContainerTran);
            coin.transform.localScale = Vector3.one * (UIManager.instance.maxCoinScale / 2f);
        }

        public static int CurrDefinedLevel
        {
            get => PlayerPrefs.GetInt(StringHelper.CURR_DEFINED_LEVEL, 0);
            set => PlayerPrefs.SetInt(StringHelper.CURR_DEFINED_LEVEL, value);
        }

        public static int HelperWordIndex
        {
            get => PlayerPrefs.GetInt(StringHelper.HELPER_WORD_INDEX, 0);
            set => PlayerPrefs.SetInt(StringHelper.HELPER_WORD_INDEX, value);
        }
        
        public int CurrDifficultyNumber
        {
            get
            {
                if (PlayerPrefs.GetInt(StringHelper.DIFF_NUM) >=
                    _diffDataInfo.difficultyInfos.Count)
                {
                    PlayerPrefs.SetInt(StringHelper.DIFF_NUM,
                        _diffDataInfo.difficultyInfos.Count - 1);
                }

                return PlayerPrefs.GetInt(StringHelper.DIFF_NUM, 0);
            }
            set => PlayerPrefs.SetInt(StringHelper.DIFF_NUM, value);
        }

        public static int CurrGridSize
        {
            get => PlayerPrefs.GetInt((StringHelper.CURR_GRIDSIZE), GameController.instance.startingGridSize);
            set => PlayerPrefs.SetInt(StringHelper.CURR_GRIDSIZE, value);
        }

        public static int IsMaxGridOpened
        {
            get => PlayerPrefs.GetInt((StringHelper.MAX_GRID_OPENED), 0);
            set => PlayerPrefs.SetInt(StringHelper.MAX_GRID_OPENED, value);
        }
        
        public static int CurrQuesSize
        {
            get => PlayerPrefs.GetInt((StringHelper.CURR_QUESSIZE), GameController.instance.startingQuesSize);
            set => PlayerPrefs.SetInt(StringHelper.CURR_QUESSIZE, value);
        }

        public static int TotalCoin
        {
            get => PlayerPrefs.GetInt(StringHelper.COIN_AVAIL, _initCoins);
            set => PlayerPrefs.SetInt(StringHelper.COIN_AVAIL, value);
        }
    }
}