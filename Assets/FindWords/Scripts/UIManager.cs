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
        public TextMeshProUGUI iqExperienceText;
        public Slider iqSlider;
        public TextMeshProUGUI coinText, coinAnimText;
        public CanvasGroup coinAnimCanvasGroup;
        private Vector3 _coinAnimTextDefaultPos;
        public Button shuffleButton, dealButton, hintButton;
        private Image _shuffleButtonImg, _dealButtonImg, _hintButtonImg;
        private Vector3 _defaultHintButtonSize, _defaultShuffleButtonSize, _defaultDealButtonSize;
        public Sprite pressedDealButton, normalDealButton;
        public Ease coinMovementEase;
        private float _coinTextUpdateTime;
        public Color disableButtonColor;
        [SerializeField] float wrongEffectTime = 0.2f;
        [SerializeField] Image wrongEffectImg;
        public Color defaultWrongEffectColor, redWrongEffectColor;
        private Color _defaultCoinAnimTextColor, _coinTextTargetColor;
        public bool boolNewQuesIntroduced;
        float _startCoinTextFadeTime, _defaultSmokeAnimSpeed = 1f;

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

            shuffleButton.onClick.AddListener(() => { GameController.Instance.ShuffleGrid(); });
            dealButton.onClick.AddListener(() => { GameController.Instance.Deal(); });
            hintButton.onClick.AddListener(() => { GameController.Instance.Hint(); });

            _shuffleButtonImg = shuffleButton.GetComponent<Image>();
            _dealButtonImg = dealButton.GetComponent<Image>();
            _hintButtonImg = hintButton.GetComponent<Image>();
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


        public void CanTouch(bool isActive, bool shouldButtonStatusBeChecked = true)
        {
            //Debug.Log("Touch is " + isActive);
            touchPanelGm.SetActive(!isActive);

            if (shouldButtonStatusBeChecked)
            {
                SetAllButtonStatus(isActive);
            }
        }

        public void EnableHint()
        {
            hintButton.enabled = true;
        }

        public bool HintStatus(bool isActive)
        {
            //Debug.Log("Hint Button Status Called !");
            //Debug.Log("Hint Button Status : " + hintButton.enabled);
            hintButton.enabled = true;

            if (isActive == false ||
                DataHandler.UnlockedQuesLetter > LevelHandler.Instance.gridAvailableOnScreenList.Count)
            {
                //Debug.Log("Hint Disable Called !");
                _hintButtonImg.color = disableButtonColor;
                LevelHandler.Instance.isHintAvailInButton = false;
            }
            else
            {
                //Debug.Log("Hint Enable Called !");

                LevelHandler.Instance.isHintAvailInButton = true;
                _hintButtonImg.color = Color.white;
            }

            //Debug.Log("Hint Button Status : " + hintButton.enabled);
            CheckOtherButtonStatus();

            return hintButton.enabled;
        }

        public void CheckAllButtonStatus()
        {
            Debug.Log("Checking All Button Status !");

            bool isHintAvail = LevelHandler.Instance.CheckHintStatus(out string finalStr);
            HintStatus(isHintAvail);

            if (isHintAvail && DataHandler.TotalCoin >= GameController.Instance.hintUsingCoin)
            {
                hintButton.enabled = true;
                _hintButtonImg.color = Color.white;
            }
        }

        private void CheckOtherButtonStatus()
        {
            //Debug.Log("Check Other Button Status Called !");
            dealButton.enabled = true;
            shuffleButton.enabled = true;
            //
            // _shuffleButtonImg.color = DataHandler.TotalCoin < GameController.Instance.shuffleUsingCoin
            //     ? disableButtonColor
            //     : Color.white;
            //
            // _dealButtonImg.color = DataHandler.TotalCoin < GameController.Instance.dealUsingCoin
            //     ? disableButtonColor
            //     : Color.white;
        }

        private void SetAllButtonStatus(bool isActive)
        {
            //Debug.Log("Set All Button Status Called !");
            if (DataHandler.TotalCoin < GameController.Instance.shuffleUsingCoin)
            {
                shuffleButton.enabled = true;
                _shuffleButtonImg.color = Color.white;
            }
            else if (!isActive)
            {
                shuffleButton.enabled = false;
                _shuffleButtonImg.color = disableButtonColor;
            }
            else
            {
                shuffleButton.enabled = true;
                _shuffleButtonImg.color = Color.white;
            }

            if (DataHandler.TotalCoin < GameController.Instance.dealUsingCoin)
            {
                dealButton.enabled = true;
                _dealButtonImg.color = Color.white;
            }
            else if (!isActive)
            {
                dealButton.enabled = false;
                _dealButtonImg.color = disableButtonColor;
            }
            else
            {
                dealButton.enabled = true;
                _dealButtonImg.color = Color.white;
            }

            if (!isActive)
            {
                hintButton.enabled = false;
                _hintButtonImg.color = disableButtonColor;
            }
            else if (DataHandler.TotalCoin < GameController.Instance.hintUsingCoin)
            {
                hintButton.enabled = true;
                _hintButtonImg.color = Color.white;
            }
            else if (!LevelHandler.Instance.isHintAvailInButton)
            {
                hintButton.enabled = true;
                _hintButtonImg.color = disableButtonColor;
            }
            else
            {
                hintButton.enabled = true;
                _hintButtonImg.color = Color.white;
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
                //Debug.Log("Button will Not Work in This Mode !");
            }
        }


        private IEnumerator PlayCoinAnim(int coinToBeAdded)
        {
            _coinTextUpdateTime = coinAnimTime / 2;
            float xVal = Random.Range(-0.5f, 0.5f);
            float yVal = Random.Range(-0.5f, 0.5f);
            bool isCoinTextUpdating = false;
            int totalCoin = GameController.Instance.coinPoolSize;
            Vector3 coinContainerTransPos = GameController.Instance.coinContainerTran.position;
            float coinSpawningTime = coinAnimTime / 2;
            float coinMovementTime = (coinSpawningTime) / totalCoin;

            SetCoinData(coinToBeAdded);

            StartCoroutine(UpdateAddedCoinText((coinMovementTime + _coinTextUpdateTime), coinToBeAdded,
                (_coinTextUpdateTime / 2)));


            RectTransform coinRecTrans = coinText.transform.parent.GetComponent<RectTransform>();
            Vector2 coinTargetOffset = new Vector2(coinRecTrans.rect.x, coinRecTrans.rect.y);
            Vector3 coinTargetPos = new Vector3(Screen.width + (3 * (coinTargetOffset.x / 2)),
                Screen.height, -2f);
            coinTargetPos = Camera.main.ScreenToWorldPoint(coinTargetPos);
            //Debug.Log("coin Target Pos : " + coinTargetPos);
            for (int i = 0; i < GameController.Instance.coinPoolSize; i++)
            {
                GameObject coin = DataHandler.Instance.GetCoin();
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
                            DataHandler.Instance.ResetCoin(coin);
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
            //Debug.Log("Coin Adding : " + coinToBeAdded);

            int startVal = int.Parse(coinText.text);
            float time = coinMoveTime / (float)coinToBeAdded;

            for (int i = 1; i <= coinToBeAdded; i++)
            {
                coinText.text = $"{startVal + i}";
                yield return new WaitForSeconds(time);
            }

            SetUICoinText();
        }

        public IEnumerator UpdateReducedCoinText(float waitTime, int coinToSubtract,
            Action callback = null)
        {
            //Debug.Log("Reduced Coin Text Called !");
            int startVal = int.Parse(coinText.text);
            int increaseBy = 1;

            if (coinToSubtract is >= 100 and < 1000)
            {
                increaseBy = 10;
            }
            else if (coinToSubtract >= 1000)
            {
                increaseBy = 100;
            }

            float time = 1f / ((float)coinToSubtract / increaseBy);
            //Debug.Log("Coin Reduce Time : " + time);

            yield return new WaitForSeconds(waitTime);

            for (int i = 1; i <= coinToSubtract; i += increaseBy)
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
            //Debug.Log("Total Coin Left : " + DataHandler.TotalCoin);
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
            _dealButtonImg.sprite = pressedDealButton;
            dealButton.transform.localScale = _defaultDealButtonSize;
            Vector3 finalScale = dealButton.transform.localScale - Vector3.one * 0.1f;
            dealButton.transform.DOScale(finalScale, 0.25f).OnComplete(() =>
            {
                dealButton.transform.DOScale(_defaultDealButtonSize, 0.25f);
            });
            yield return new WaitForSeconds(0.25f);
            _dealButtonImg.sprite = normalDealButton;
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

        public void SmokeTransition(bool shouldBackGroundChange = false, float speed = 1f)
        {
            SmokeAnimationSpeed smokeSpeedScript = smokeTransitionGm.GetComponent<SmokeAnimationSpeed>();

            smokeSpeedScript.ChangeAnimationSpeed(Math.Abs(speed - 1f) < 0.05f
                ? smokeSpeedScript.defaultAnimationSpeed
                : speed);

            smokeTransitionGm.SetActive(true);
            float totalTime = smokeSpeedScript.TotalAnimationTime() * speed;
            StartCoroutine(SmokeAnim(shouldBackGroundChange, totalTime));
        }

        IEnumerator SmokeAnim(bool shouldBackGroundChange, float totalTime)
        {
            Debug.Log("Smoke Anim Called !");

            if (shouldBackGroundChange)
            {
                DataHandler.BgIndex++;
            }

            Debug.Log("Smoke Anim Time : " + totalTime);
            //LevelHandler.Instance.SetLevelRunningBool(false);

            yield return new WaitForSeconds(totalTime / 2);

            if (shouldBackGroundChange)
            {
                DataHandler.Instance.SetBg();
            }

            yield return new WaitForSeconds(totalTime / 2);
            smokeTransitionGm.SetActive(false);
            LevelHandler.Instance.SetLevelRunningBool();
        }
    }
}