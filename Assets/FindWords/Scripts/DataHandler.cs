using System;
using System.Collections.Generic;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    public class DataHandler : MonoBehaviour
    {
        public static DataHandler instance;
        public List<Material> gameBgMaterialList;
        public List<int> iqToUnlockNewQuesTileList = new List<int>();
        [Header("Prefab Holders")] public GameObject coinPrefab;
        public GameObject levelPrefab, gridPrefab, quesPrefab;

        [Header("Data Info")] [Tooltip("When player first time start playing, Initial coins player have")]
        public int initialCoins = 300;

        private static int _initCoins;

        private void Awake()
        {
            CreateSingleton();
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
                Destroy(gameObject);
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
        }

        public void SetBg()
        {
            if (BgIndex >= gameBgMaterialList.Count)
            {
                BgIndex = 0;
            }

            UIManager.Instance.gameBg.GetComponent<MeshRenderer>().material = gameBgMaterialList[BgIndex];
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
            coin.transform.localScale = Vector3.one * (UIManager.Instance.maxCoinScale / 2f);
        }

        public static int FirstTimeGameOpen
        {
            get => PlayerPrefs.GetInt(StringHelper.FIRST_TIME_OPEN, 0);
            set => PlayerPrefs.SetInt(StringHelper.FIRST_TIME_OPEN, value);
        }

        public static int LevelNum
        {
            get => PlayerPrefs.GetInt(StringHelper.LEVEL_NUM, 1);
            set => PlayerPrefs.SetInt(StringHelper.LEVEL_NUM, value);
        }
        
        public static int IsUnlockingGridActive
        {
            get => PlayerPrefs.GetInt(StringHelper.UNLOCKING_GRIDS, 0);
            set => PlayerPrefs.SetInt(StringHelper.UNLOCKING_GRIDS, value);
        }
        
        public static int HelperLevelCompleted
        {
            get => PlayerPrefs.GetInt(StringHelper.HELPER_LEVEL_COMPLETED, 0);
            set => PlayerPrefs.SetInt(StringHelper.HELPER_LEVEL_COMPLETED, value);
        }

        public static int FirstTimeGameClose
        {
            get => PlayerPrefs.GetInt(StringHelper.FIRST_TIME_GAME_CLOSE, 0);
            set => PlayerPrefs.SetInt(StringHelper.FIRST_TIME_GAME_CLOSE, value);
        }

        public static string CurrHint
        {
            get => PlayerPrefs.GetString(StringHelper.CURR_HINT, "");
            set => PlayerPrefs.SetString(StringHelper.CURR_HINT, value);
        }

        public static int NewGridCreated
        {
            get => PlayerPrefs.GetInt(StringHelper.NEW_GRID_CREATED, 0);
            set => PlayerPrefs.SetInt(StringHelper.NEW_GRID_CREATED, value);
        }

        public static int CoinGridUnlockIndex
        {
            get => PlayerPrefs.GetInt(StringHelper.COIN_GRID_UNLOCK_INDEX, 0);
            set => PlayerPrefs.SetInt(StringHelper.COIN_GRID_UNLOCK_INDEX, value);
        }

        public static int UnlockGridIndex
        {
            get => PlayerPrefs.GetInt(StringHelper.UNLOCK_GRID_INDEX, 0);
            set => PlayerPrefs.SetInt(StringHelper.UNLOCK_GRID_INDEX, value);
        }

        public static int CurrDefinedLevel
        {
            get => PlayerPrefs.GetInt(StringHelper.CURR_DEFINED_LEVEL, 0);
            set => PlayerPrefs.SetInt(StringHelper.CURR_DEFINED_LEVEL, value);
        }

        public static int BgIndex
        {
            get => PlayerPrefs.GetInt(StringHelper.BG_INDEX, 0);
            set => PlayerPrefs.SetInt(StringHelper.BG_INDEX, value);
        }

        public static int IqExpLevel
        {
            get => PlayerPrefs.GetInt(StringHelper.IQ_EXP_LEVEL, GameController.instance.defaultIq);
            set => PlayerPrefs.SetInt(StringHelper.IQ_EXP_LEVEL, value);
        }

        public static float IqBarVal
        {
            get => PlayerPrefs.GetFloat(StringHelper.IQ_BAR_VAL, 0);
            set => PlayerPrefs.SetFloat(StringHelper.IQ_BAR_VAL, value);
        }

        public static int WordCompleteNum
        {
            get => PlayerPrefs.GetInt(StringHelper.WORD_COMPLETE_NUM, GameController.instance.defaultIq);
            set => PlayerPrefs.SetInt(StringHelper.WORD_COMPLETE_NUM, value);
        }

        //This is for traversing the Pick Data Info Word List
        public static int PickDataIndex
        {
            get => PlayerPrefs.GetInt(StringHelper.PICK_DATA_INDEX, 0);
            set => PlayerPrefs.SetInt(StringHelper.PICK_DATA_INDEX, value);
        }

        public static int HelperIndex
        {
            get => PlayerPrefs.GetInt(StringHelper.HELPER_WORD_INDEX, 0);
            set => PlayerPrefs.SetInt(StringHelper.HELPER_WORD_INDEX, value);
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

        public static int CurrTotalQuesSize
        {
            get => PlayerPrefs.GetInt((StringHelper.CURR_TOTAL_QUES_SIZE), GameController.instance.startingQuesSize);
            set => PlayerPrefs.SetInt(StringHelper.CURR_TOTAL_QUES_SIZE, value);
        }

        public static int NewQuesGridUnlock
        {
            get => PlayerPrefs.GetInt((StringHelper.NEW_QUES_TILE_UNLOCK), 0);
            set => PlayerPrefs.SetInt(StringHelper.NEW_QUES_TILE_UNLOCK, value);
        }
        
        public static int NewQuesGridUnlockIndex
        {
            get => PlayerPrefs.GetInt((StringHelper.NEW_QUES_TILE_UNLOCK_INDEX), 0);
            set => PlayerPrefs.SetInt(StringHelper.NEW_QUES_TILE_UNLOCK_INDEX, value);
        }
        
        public static int TotalCoin
        {
            get => PlayerPrefs.GetInt(StringHelper.COIN_AVAIL, _initCoins);
            set => PlayerPrefs.SetInt(StringHelper.COIN_AVAIL, value);
        }

        public static int UnlockedQuesLetter
        {
            get => PlayerPrefs.GetInt((StringHelper.UNLOCK_QUES_LETTER), GameController.instance.startingQuesSize);
            set => PlayerPrefs.SetInt(StringHelper.UNLOCK_QUES_LETTER, value);
        }
    }
}