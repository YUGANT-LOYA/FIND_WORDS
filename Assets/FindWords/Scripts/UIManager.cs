using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace YugantLoyaLibrary.FindWords
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;
        public float coinAnimTime = 1.5f, coinRotateAngle = 810f, maxCoinScale = 45f;
        public TextMeshProUGUI levelText, coinText;
        public Button shuffleButton, dealButton, hintButton;
        public GameObject winPanel;
        public GameObject menuGameObj, coinHolderGm;
        public Ease coinMovementEase;
        private float _coinTextUpdateTime;
        public Color disableHintColor;

        private void Awake()
        {
            CreateSingleton();
            shuffleButton.onClick.AddListener(() => { GameController.instance.ShuffleGrid(); });
            dealButton.onClick.AddListener(() => { GameController.instance.Deal(); });
            hintButton.onClick.AddListener(() => { GameController.instance.Hint(); });
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

        public void HintStatus(bool isActive)
        {
            if (!isActive)
            {
                hintButton.GetComponent<Image>().color = disableHintColor;
                hintButton.enabled = false;
            }
            else
            {
                hintButton.enabled = true;
                hintButton.GetComponent<Image>().color = Color.white;
            }
        }

        public void CoinCollectionAnimation(int coinToAdd)
        {
            StartCoroutine(nameof(PlayCoinAnim), coinToAdd);
        }

        private IEnumerator PlayCoinAnim(int coinToBeAdded)
        {
            _coinTextUpdateTime = coinAnimTime / 2;
            float xVal = Random.Range(-0.5f, 0.5f);
            float yVal = Random.Range(-0.5f, 0.5f);
            bool isCoinTextUpdating = false;
            int totalCoin = GameController.instance.coinPoolSize;
            Transform trans = GameController.instance.coinContainerTran;
            float coinSpawningTime = coinAnimTime / 2;
            float coinMovementTime = (coinSpawningTime) / totalCoin;

            SetCoinData(coinToBeAdded);

            StartCoroutine(UpdateAddedCoinText((coinMovementTime + _coinTextUpdateTime), coinToBeAdded,
                (_coinTextUpdateTime / 2)));

            for (int i = 0; i < GameController.instance.coinPoolSize; i++)
            {
                GameObject coin = DataHandler.instance.GetCoin();
                coin.transform.localScale = Vector3.one * maxCoinScale;
                coin.transform.rotation = Quaternion.identity;
                coin.transform.position = new Vector3(xVal, yVal, -0.5f);
                coin.SetActive(true);

                yield return new WaitForSeconds(coinMovementTime);

                coin.transform.DORotate(new Vector3(coinRotateAngle, coinRotateAngle,
                    coinRotateAngle), _coinTextUpdateTime, RotateMode.FastBeyond360).SetEase(coinMovementEase);

                coin.transform.DOMove(new Vector2(1.75f, 5f), _coinTextUpdateTime)
                    .SetEase(coinMovementEase).OnComplete(
                        () =>
                        {
                            DataHandler.instance.ResetCoin(coin);
                            if (isCoinTextUpdating) return;
                            isCoinTextUpdating = true;
                        });

                xVal = Random.Range(-0.5f, 0.5f);
                yVal = Random.Range(-0.5f, 0.5f);
            }
        }

        void CallWinPanel()
        {
            winPanel.SetActive(true);
        }

        private IEnumerator UpdateAddedCoinText(float waitTime, int coinToBeAdded, float coinMoveTime,
            Action callback = null)
        {
            yield return new WaitForSeconds(waitTime);
            Debug.Log("Coin Adding : " + coinToBeAdded);

            int startVal = int.Parse(coinText.text);
            float time = coinMoveTime / (float)coinToBeAdded;

            for (int i = 1; i <= coinToBeAdded; i++)
            {
                coinText.text = $"{startVal + i}";
                yield return new WaitForSeconds(time);
            }
        }

        public IEnumerator UpdateReducedCoinText(float waitTime, int coinToSubtract, float coinMoveTime,
            Action callback = null)
        {
            yield return new WaitForSeconds(waitTime);


            int startVal = int.Parse(coinText.text);
            float time = coinMoveTime / (float)coinToSubtract;

            for (int i = 1; i <= coinToSubtract; i++)
            {
                coinText.text = $"{startVal - i}";
                yield return new WaitForSeconds(time);
            }
        }

        public static void SetCoinData(int coinToBeAdded, int sign = 1)
        {
            int totalCoins = DataHandler.TotalCoin + coinToBeAdded * sign;
            DataHandler.TotalCoin = totalCoins;
            Debug.Log("Total Coin Left : " + DataHandler.TotalCoin);
        }
    }
}