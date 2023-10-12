using System.Collections.Generic;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    public class DataHandler : MonoBehaviour
    {
        public static DataHandler instance;

        LevelHandler _levelHandler;
        private DifficultyDataInfo _diffDataInfo;
        [Header("Prefab Holders")] public GameObject coinPrefab;
        public GameObject levelPrefab, gridPrefab, quesPrefab;

        [Header("Data Info")] [Tooltip("When player first time start playing, Initial coins player have")]
        public int initialCoins = 300;

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

        public int CurrGridSize
        {
            get => PlayerPrefs.GetInt((StringHelper.CURR_GRIDSIZE), GameController.instance.startingGridSize);
            set => PlayerPrefs.SetInt(StringHelper.CURR_GRIDSIZE, value);
        }

        public int CurrQuesSize
        {
            get => PlayerPrefs.GetInt((StringHelper.CURR_QUESSIZE), GameController.instance.startingQuesSize);
            set => PlayerPrefs.SetInt(StringHelper.CURR_QUESSIZE, value);
        }

        public int TotalCoin
        {
            get => PlayerPrefs.GetInt(StringHelper.COIN_AVAIL, initialCoins);
            set => PlayerPrefs.SetInt(StringHelper.COIN_AVAIL, value);
        }
    }
}