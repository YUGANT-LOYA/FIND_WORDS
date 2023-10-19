using System.Collections.Generic;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    public class DataHandler : MonoBehaviour
    {
        public static DataHandler instance;
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

        public static int IqLevel
        {
            get => PlayerPrefs.GetInt(StringHelper.IQ_LEVEL, GameController.instance.defaultIq);
            set => PlayerPrefs.SetInt(StringHelper.IQ_LEVEL, value);
        }

        //This is for traversing the Pick Data Info Word List
        public static int PickDataIndex
        {
            get => PlayerPrefs.GetInt(StringHelper.PICK_DATA_INDEX, 0);
            set => PlayerPrefs.SetInt(StringHelper.PICK_DATA_INDEX, value);
        }

        //This is for traversing the Pick Data Struct List.
        public static int PickDataStructIndex
        {
            get => PlayerPrefs.GetInt(StringHelper.PICK_DATA_SRTUCT_INDEX, 0);
            set => PlayerPrefs.SetInt(StringHelper.PICK_DATA_SRTUCT_INDEX, value);
        }

        public static int HelperWordIndex
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