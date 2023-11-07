using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YugantLibrary.Camera;
using Random = UnityEngine.Random;
using NaughtyAttributes;
using UnityEngine.Serialization;

namespace YugantLoyaLibrary.FindWords
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        private Camera _cam;
        public GameObject gameBg;
        public Animator catIdleAnimator;
        public GameObject touchPanelGm, smokeTransitionGm;
        private CameraShake _camShakeScript;
        public ToastMessage toastMessageScript;
        public float coinAnimTime = 1.5f, coinRotateAngle = 810f, maxCoinScale = 45f;
        public TextMeshProUGUI iqLevelText;
        public TextMeshProUGUI coinText, coinAnimText;
        public CanvasGroup coinAnimCanvasGroup;
        private Vector3 _coinAnimTextDefaultPos;
        public Button shuffleButton, dealButton, hintButton;
        private Vector3 _defaultHintButtonSize, _defaultShuffleButtonSize, _defaultDealButtonSize;
        public Sprite pressedDealButton, normalDealButton;
        public Ease coinMovementEase;
        private float _coinTextUpdateTime;
        public Color disableButtonColor;
        [SerializeField] float wrongEffectTime = 0.2f;
        [SerializeField] Image wrongEffectImg;
        public Color defaultWrongEffectColor, redWrongEffectColor;
        private Color _defaultCoinAnimTextColor, _coinTextTargetColor;
        private bool _isFading;
        float _startCoinTextFadeTime;

        private void Awake()
        {
            CreateSingleton();

            _cam = Camera.main;
            if (_cam != null) _camShakeScript = _cam.GetComponent<CameraShake>();

            _coinAnimTextDefaultPos = coinAnimText.transform.position;
            _defaultCoinAnimTextColor =
                new Color(coinAnimText.color.r, coinAnimText.color.g, coinAnimText.color.b, 255f);
            _coinTextTargetColor = new Color(_defaultCoinAnimTextColor.r, _defaultCoinAnimTextColor.g,
                _defaultCoinAnimTextColor.b, 0f);
            _defaultHintButtonSize = hintButton.transform.localScale;
            _defaultShuffleButtonSize = shuffleButton.transform.localScale;
            _defaultDealButtonSize = dealButton.transform.localScale;

            shuffleButton.onClick.AddListener(() => { GameController.instance.ShuffleGrid(); });
            dealButton.onClick.AddListener(() => { GameController.instance.Deal(); });
            hintButton.onClick.AddListener(() => { GameController.instance.Hint(); });
        }

        private void CreateSingleton()
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
        

        public void CanTouch(bool isActive)
        {
            Debug.Log("Touch is " + isActive);
            touchPanelGm.SetActive(!isActive);
            SetAllButtonStatus(isActive);
        }

        public void EnableHint()
        {
            hintButton.enabled = true;
        }

        public bool HintStatus(bool isActive)
        {
            Debug.Log("Hint Button Status : " + hintButton.enabled);

            if (isActive == false ||
                DataHandler.CurrTotalQuesSize > LevelHandler.instance.gridAvailableOnScreenList.Count)
            {
                Debug.Log("Hint Disable Called !");
                hintButton.GetComponent<Image>().color = disableButtonColor;
                //hintButton.enabled = false;
            }
            else
            {
                Debug.Log("Hint Enable Called !");
                hintButton.enabled = true;
                hintButton.GetComponent<Image>().color = Color.white;
            }

            Debug.Log("Hint Button Status : " + hintButton.enabled);
            CheckOtherButtonStatus();
            LevelHandler.instance.isHintAvailInButton = hintButton.enabled;

            return hintButton.enabled;
        }

        public void CheckAllButtonStatus()
        {
            Debug.Log("Checking All Button Status !");

            bool isHintAvail = LevelHandler.instance.CheckHintStatus(out string finalStr);
            HintStatus(isHintAvail);

            if (isHintAvail && DataHandler.TotalCoin >= GameController.instance.hintUsingCoin)
            {
                hintButton.enabled = true;
                hintButton.GetComponent<Image>().color = Color.white;
            }
        }

        private void CheckOtherButtonStatus()
        {
            if (DataHandler.TotalCoin < GameController.instance.shuffleUsingCoin)
            {
                shuffleButton.GetComponent<Image>().color = disableButtonColor;
                shuffleButton.enabled = false;
            }
            else
            {
                shuffleButton.enabled = true;
                shuffleButton.GetComponent<Image>().color = Color.white;
            }

            if (DataHandler.TotalCoin < GameController.instance.dealUsingCoin)
            {
                dealButton.GetComponent<Image>().color = disableButtonColor;
                dealButton.enabled = false;
            }
            else
            {
                dealButton.enabled = true;
                dealButton.GetComponent<Image>().color = Color.white;
            }
        }

        private void SetAllButtonStatus(bool isActive)
        {
            Image hintImage = hintButton.GetComponent<Image>();
            Image dealImage = dealButton.GetComponent<Image>();
            Image shuffleImage = shuffleButton.GetComponent<Image>();

            if (DataHandler.TotalCoin < GameController.instance.shuffleUsingCoin)
            {
                shuffleButton.enabled = false;
                shuffleImage.color = disableButtonColor;
            }
            else
            {
                shuffleButton.enabled = isActive;

                shuffleImage.color = isActive ? Color.white : disableButtonColor;
            }

            if (DataHandler.TotalCoin < GameController.instance.dealUsingCoin)
            {
                dealButton.enabled = false;
                dealImage.color = disableButtonColor;
            }
            else
            {
                dealButton.enabled = isActive;
                dealImage.color = isActive ? Color.white : disableButtonColor;
            }

            if (DataHandler.TotalCoin < GameController.instance.hintUsingCoin)
            {
                hintButton.enabled = true;
                hintImage.color = disableButtonColor;
            }
            else if (!LevelHandler.instance.isHintAvailInButton)
            {
                hintButton.enabled = true;
                hintImage.color = disableButtonColor;
            }
            else
            {
                hintButton.enabled = isActive;
                hintImage.color = isActive ? Color.white : disableButtonColor;
            }
        }


        public void WrongEffect()
        {
            float redTime = 2 * wrongEffectTime / 3;
            float whiteTime = wrongEffectTime / 3;
            wrongEffectImg.DOColor(redWrongEffectColor, redTime / 2).OnComplete(() =>
            {
                wrongEffectImg.DOColor(defaultWrongEffectColor, whiteTime / 2).OnComplete(() =>
                {
                    wrongEffectImg.DOColor(redWrongEffectColor, redTime / 2).OnComplete(() =>
                    {
                        wrongEffectImg.DOColor(defaultWrongEffectColor, whiteTime / 2)
                            .OnComplete(() => { wrongEffectImg.color = defaultWrongEffectColor; });
                    });
                });
            });
        }

        public void CoinCollectionAnimation(int coinToAdd)
        {
            StartCoroutine(nameof(PlayCoinAnim), coinToAdd);
            PlayCoinAnimText(coinToAdd);
        }

        void PlayCoinAnimText(int coinToAdd)
        {
            coinAnimCanvasGroup.alpha = 1f;
            coinAnimText.transform.position = _coinAnimTextDefaultPos;
            coinAnimText.color = _defaultCoinAnimTextColor;
            coinAnimText.text = $"+{coinToAdd}";
            coinAnimText.transform.DOMoveY(_coinAnimTextDefaultPos.y + 500f, (3 * coinAnimTime / 2))
                .SetEase(Ease.Linear);

            coinAnimCanvasGroup.DOFade(0f, (3 * coinAnimTime / 2));
        }

        [Button]
        void PlayCoinDoTween()
        {
            if (Application.isPlaying)
            {
                StartCoroutine(nameof(PlayCoinAnim), 20);
                PlayCoinAnimText(20);
            }
            else
            {
                Debug.Log("Button will Not Work in This Mode !");
            }
        }


        private IEnumerator PlayCoinAnim(int coinToBeAdded)
        {
            _coinTextUpdateTime = coinAnimTime / 2;
            float xVal = Random.Range(-0.5f, 0.5f);
            float yVal = Random.Range(-0.5f, 0.5f);
            bool isCoinTextUpdating = false;
            int totalCoin = GameController.instance.coinPoolSize;
            Vector3 coinContainerTransPos = GameController.instance.coinContainerTran.position;
            float coinSpawningTime = coinAnimTime / 2;
            float coinMovementTime = (coinSpawningTime) / totalCoin;

            SetCoinData(coinToBeAdded);

            StartCoroutine(UpdateAddedCoinText((coinMovementTime + _coinTextUpdateTime), coinToBeAdded,
                (_coinTextUpdateTime / 2)));


            RectTransform coinRecTrans = coinText.transform.parent.GetComponent<RectTransform>();
            Vector2 coinTargetOffset = new Vector2(coinRecTrans.rect.x, coinRecTrans.rect.y);
            Vector3 coinTargetPos = new Vector3(Screen.width + (3 * (coinTargetOffset.x / 2)),
                Screen.height + coinTargetOffset.y, -2f);
            coinTargetPos = Camera.main.ScreenToWorldPoint(coinTargetPos);
            Debug.Log("coin Target Pos : " + coinTargetPos);
            for (int i = 0; i < GameController.instance.coinPoolSize; i++)
            {
                GameObject coin = DataHandler.instance.GetCoin();
                coin.transform.localScale = Vector3.one * maxCoinScale;
                coin.transform.rotation = Quaternion.identity;
                coin.transform.position = new Vector3(coinContainerTransPos.x + xVal,
                    coinContainerTransPos.y + yVal, -1f);

                coin.SetActive(true);

                yield return new WaitForSeconds(coinMovementTime);

                coin.transform.DORotate(new Vector3(coinRotateAngle, coinRotateAngle,
                    coinRotateAngle), _coinTextUpdateTime, RotateMode.FastBeyond360).SetEase(coinMovementEase);

                coin.transform.DOMove(coinTargetPos,
                        _coinTextUpdateTime)
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

        void SetUICoinText()
        {
            coinText.text = $"{DataHandler.TotalCoin}";
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

            SetUICoinText();
        }

        public IEnumerator UpdateReducedCoinText(float waitTime, int coinToSubtract, float coinMoveTime,
            Action callback = null)
        {
            Debug.Log("Reduced Coin Text Called !");
            yield return new WaitForSeconds(waitTime);

            int startVal = int.Parse(coinText.text);
            float time = coinMoveTime / (float)coinToSubtract;

            for (int i = 1; i <= coinToSubtract; i++)
            {
                coinText.text = $"{startVal - i}";
                yield return new WaitForSeconds(time);
            }

            SetUICoinText();
        }

        public static void SetCoinData(int coinToBeAdded, int sign = 1)
        {
            int totalCoins = DataHandler.TotalCoin + coinToBeAdded * sign;
            DataHandler.TotalCoin = totalCoins;
            Debug.Log("Total Coin Left : " + DataHandler.TotalCoin);
        }

        public void ShakeCam()
        {
            _camShakeScript.ShakeCamera(_cam.transform.position);
        }

        public void DealButtonEffect()
        {
            StartCoroutine(DealButtonClicked());
        }

        IEnumerator DealButtonClicked()
        {
            dealButton.gameObject.GetComponent<Image>().sprite = pressedDealButton;
            dealButton.transform.localScale = _defaultDealButtonSize;
            Vector3 finalScale = dealButton.transform.localScale - Vector3.one * 0.1f;
            dealButton.gameObject.transform.DOScale(finalScale, 0.25f).OnComplete(() =>
            {
                dealButton.gameObject.transform.DOScale(_defaultDealButtonSize, 0.25f);
            });
            yield return new WaitForSeconds(0.25f);
            dealButton.gameObject.GetComponent<Image>().sprite = normalDealButton;
        }

        public void HintButtonClicked()
        {
            hintButton.transform.localScale = _defaultHintButtonSize;
            Vector3 finalScale = hintButton.transform.localScale - Vector3.one * 0.1f;
            hintButton.gameObject.transform.DOScale(finalScale, 0.25f).OnComplete(() =>
            {
                hintButton.gameObject.transform.DOScale(_defaultHintButtonSize, 0.25f);
            });
        }

        public void ShuffleButtonClicked()
        {
            shuffleButton.transform.localScale = _defaultShuffleButtonSize;
            Vector3 finalScale = shuffleButton.transform.localScale - Vector3.one * 0.1f;
            shuffleButton.gameObject.transform.DOScale(finalScale, 0.25f).OnComplete(() =>
            {
                shuffleButton.gameObject.transform.DOScale(_defaultShuffleButtonSize, 0.25f);
            });
        }

        public void SmokeTransition()
        {
            StartCoroutine(nameof(SmokeAnim));
        }

        IEnumerator SmokeAnim()
        {
            DataHandler.BgIndex++;
            smokeTransitionGm.SetActive(true);
            LevelHandler.instance.SetLevelRunningBool(false);

            yield return new WaitForSeconds(1.25f / 2);

            DataHandler.instance.SetBg();

            yield return new WaitForSeconds(1.25f / 2);

            smokeTransitionGm.SetActive(false);
            LevelHandler.instance.SetLevelRunningBool(true);
        }
    }
}