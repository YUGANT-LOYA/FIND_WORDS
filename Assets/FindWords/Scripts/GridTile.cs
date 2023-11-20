using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace YugantLoyaLibrary.FindWords
{
    public class GridTile : MonoBehaviour
    {
        private Level _level;
        public GameObject cube;
        [SerializeField] private GameObject currlockGm, otherLockGm;
        [SerializeField] private TextMeshPro gridText, rentTimeText;
        [HideInInspector] public Vector3 defaultGridSize;
        [HideInInspector] public Vector3 defaultLocalGridPos, defaultGlobalGridPos;
        [HideInInspector] public Quaternion defaultQuaternionRotation;
        public Vector3 blastPos;

        public bool isHelperActivate = false,
            isGridActive = true,
            isSelected,
            isBlastAfterWordComplete,
            isFullLocked,
            isCurrLock,
            isGridOnRent;

        [SerializeField] private Vector2Int id;
        public QuesTile placedOnQuesTile;

        [Header("Movement Info")] public bool isMoving;
        public int moveRotationTimes = 1;
        public Ease movingEase = Ease.Linear;
        public float reachTime = 1f;

        [Header("Blasting Info")] public Ease blastEase = Ease.InCirc;
        public int blastRotationTime = 1;
        public float blastEffectAfterTime = 0.5f, blastTime = 1f;
        public Ease blastReturnEase = Ease.OutCirc;

        [Header("Lock Info")] public TextMeshPro amountToUnlockText;
        public Ease lockEase = Ease.InOutBounce;
        public float lockShakeTime = 0.5f, shakeStrength = 0.1f;
        public int vibrationStrength = 12;
        public int shakeRandomness = 10;
        [Header("Unlock Info")] public Ease unlockEase = Ease.Linear;
        public int unlockRotationTime = 2;
        public float unlockTime = 1f;

        public Vector2Int GridID
        {
            get => id;
            set => id = value;
        }

        public string GridTextData
        {
            get => gridText.text.ToLower();
            set => gridText.text = value.ToUpper();
        }

        public TextMeshPro GetText()
        {
            return gridText;
        }

        public void SetCurrentLockStatus(bool isActive)
        {
            currlockGm.SetActive(isActive);
            otherLockGm.SetActive(!isActive);
        }

        public void DeactivateLockStatus()
        {
            ObjectStatus(true);
            currlockGm.SetActive(false);
            otherLockGm.SetActive(false);
            isCurrLock = false;
            isFullLocked = false;
            isGridActive = true;
        }

        public void SetLockTextAmount(int valToOpen)
        {
            amountToUnlockText.text = valToOpen.ToString();
        }

        public void DefaultGridData(Vector3 pos, Quaternion rot, Vector3 globalPos)
        {
            defaultGridSize = transform.localScale;
            defaultLocalGridPos = pos;
            defaultGlobalGridPos = globalPos;
            defaultQuaternionRotation = rot;
        }

        public void ObjectStatus(bool isActive)
        {
            cube.GetComponent<MeshRenderer>().enabled = isActive;
            GetComponent<BoxCollider>().enabled = isActive;
            gridText.gameObject.SetActive(isActive);
        }

        public void AssignInfo(Level currLevel)
        {
            _level = currLevel;
        }

        public void ResetWholeData()
        {
            isMoving = false;
            isSelected = false;
            isBlastAfterWordComplete = false;
        }

        public void OnMouseDown()
        {
            if (!LevelHandler.Instance.GetLevelRunningBool() || isMoving)
                return;

            if (DataHandler.HelperLevelCompleted == 0)
            {
                if (!isHelperActivate)
                    return;
            }

            Vibration.Vibrate(20);

            if (DataHandler.TotalCoin >= LevelHandler.Instance.coinToUnlockNextGrid)
            {
                if (isCurrLock)
                {
                    isFullLocked = false;
                    isGridActive = true;
                    isCurrLock = false;
                    DataHandler.UnlockGridIndex++;
                    LevelHandler.Instance.unlockedGridList.Add(this);
                    LevelHandler.Instance.totalBuyingGridList.Remove(this);
                    LevelHandler.Instance.gridAvailableOnScreenList.Add(this);
                    UIManager.SetCoinData(LevelHandler.Instance.coinToUnlockNextGrid, -1);
                    SoundManager.instance.PlaySound(SoundManager.SoundType.NewGridUnlock);
                    StartCoroutine(
                        UIManager.Instance.UpdateReducedCoinText(0f, LevelHandler.Instance.coinToUnlockNextGrid, 0.5f));
                    NewGridUnlockAnimation(LevelHandler.Instance.coinToUnlockNextGrid);
                    return;
                }
            }

            if (isFullLocked || isCurrLock)
            {
                SoundManager.instance.PlaySound(SoundManager.SoundType.LockGridClicked);
                ShakeAnim();
                return;
            }

            if (!isGridActive)
                return;


            if (LevelHandler.Instance.LastQuesTile == null)
            {
                SoundManager.instance.PlaySound(SoundManager.SoundType.Click);
                //Debug.Log($"Grid {gameObject.name} Clicked !");
                isSelected = !isSelected;
                //Debug.Log("Is Selected : " + isSelected);

                if (isSelected)
                {
                    LevelHandler.Instance.OnNewLetterAddEvent?.Invoke(this);
                    isSelected = true;
                }
                else
                {
                    LevelHandler.Instance.OnRemoveLetterEvent?.Invoke(this);
                    isSelected = false;
                }
            }
        }

        void ShakeAnim()
        {
            DOTween.Kill(transform, false);
            transform.localPosition = defaultLocalGridPos;
            transform.DOShakePosition(lockShakeTime, shakeStrength, vibrationStrength, shakeRandomness)
                .SetEase(lockEase).OnComplete(() => { transform.localPosition = defaultLocalGridPos; });
        }

        private void NewGridUnlockAnimation(int coinToSubtract)
        {
            isMoving = true;

            FillData();

            transform.DORotate(
                    new Vector3(unlockRotationTime * 360f, transform.rotation.eulerAngles.y,
                        transform.rotation.eulerAngles.z),
                    unlockTime, RotateMode.FastBeyond360)
                .SetEase(movingEase).OnComplete(() =>
                {
                    DataHandler.CoinGridUnlockIndex++;
                    LevelHandler.Instance.UnlockNextGridForCoins();
                    DeactivateLockStatus();
                    gridText.gameObject.SetActive(true);
                    isMoving = false;

                    // if (DataHandler.CoinGridUnlockIndex % GameController.instance.changeBgAfter == 0)
                    // {
                    //     UIManager.Instance.SmokeTransition();
                    // }

                    bool allGridBought = LevelHandler.Instance.CheckAllGridBought();

                    if (!allGridBought)
                    {
                        LevelHandler.Instance.CheckWordExistOrNot(out bool hintButtonStatus, out string hintStr);
                    }
                    // else
                    // {
                    //     UIManager.Instance.SmokeTransition();
                    // }
                    
                    LionStudiosManager.LevelComplete(DataHandler.LevelNum, GameController.LevelAttempts, 0);
                    GAScript.LevelEnd(true,DataHandler.LevelNum.ToString());
                    GameController.LevelAttempts = 0;
                    DataHandler.LevelNum++;
                    
                    LionStudiosManager.LevelStart(DataHandler.LevelNum, GameController.LevelAttempts, 0);
                    GAScript.LevelStart(DataHandler.LevelNum.ToString());
                });
        }


        //Filling Letter According to the word in WordLeftList.
        void FillData()
        {
            char letterToFill = LevelHandler.Instance.GetUnAvailableLetterInRandomWord();

            if (letterToFill != ' ')
            {
                GridTextData = letterToFill.ToString();
            }
        }

        public void MoveAfter(Vector3 pos, bool isMovingToQues, QuesTile quesTile, float time = 0f)
        {
            if (DataHandler.HelperLevelCompleted == 0)
            {
                GameController.instance.helper.gridHelperHand.SetActive(false);
            }

            StartCoroutine(Move(pos, isMovingToQues, quesTile, time));
        }

        IEnumerator Move(Vector3 pos, bool isMovingToQues, QuesTile quesTile, float time = 0f)
        {
            isMoving = true;

            yield return new WaitForSeconds(time);

            if (isMovingToQues)
            {
                placedOnQuesTile = quesTile;
                transform.DOScale(quesTile.transform.localScale, reachTime);
            }
            else
            {
                if (!string.IsNullOrEmpty(LevelHandler.Instance.quesHintStr) &&
                    !string.IsNullOrWhiteSpace(LevelHandler.Instance.quesHintStr))
                {
                    int id = placedOnQuesTile.id;
                    placedOnQuesTile.QuesTextData = LevelHandler.Instance.quesHintStr[id].ToString();
                    placedOnQuesTile.isAssigned = false;
                    Invoke(nameof(ActivatePlacedQuesTile), 0.2f);
                }
                else
                {
                    placedOnQuesTile.RevertData();
                }

                isSelected = false;
                transform.DOScale(defaultGridSize, reachTime);
            }

            var rotation = transform.rotation;
            transform.DORotate(
                    new Vector3(moveRotationTimes * 360f, moveRotationTimes * 360f, rotation.eulerAngles.z),
                    reachTime, RotateMode.FastBeyond360)
                .SetEase(movingEase).OnComplete(() => { transform.rotation = defaultQuaternionRotation; });

            SetQuesTileStatus(quesTile, isMovingToQues, reachTime / 2);

            transform.DOMove(pos, reachTime).SetEase(movingEase).OnComplete(() =>
            {
                if (LevelHandler.Instance.LastQuesTile == quesTile)
                {
                    LevelHandler.Instance.CheckAnswer();
                }

                isMoving = false;

                if (DataHandler.HelperLevelCompleted == 0)
                {
                    isHelperActivate = false;
                    DataHandler.HelperIndex++;
                    GameController.instance.helper.ClickTile(0.2f);
                }
            });
        }

        void ActivatePlacedQuesTile()
        {
            placedOnQuesTile.ActivateObject();
        }

        public void SetQuesTileStatus(QuesTile quesTile, bool isMovingToQues, float time)
        {
            StartCoroutine(QuesStatus(quesTile, isMovingToQues, time));
        }

        IEnumerator QuesStatus(QuesTile quesTile, bool isMovingToQues, float time)
        {
            yield return new WaitForSeconds(time);

            quesTile.gameObject.SetActive(!isMovingToQues);

            if (isMovingToQues)
            {
                placedOnQuesTile.AddData(GridTextData);
            }
        }

        public void CallBlastAfterTime()
        {
            StartCoroutine(Blast());
        }

        IEnumerator Blast()
        {
            yield return new WaitForSeconds(blastEffectAfterTime);
            blastPos = _level.GetRandomPointOutOfScreen();
            transform.DOMove(blastPos, blastTime / 2).SetEase(blastEase);
            transform.DOScale(defaultGridSize, blastTime / 2).SetEase(movingEase);
            transform.DORotate(new Vector3(blastRotationTime * -360f, 0f, -360f * blastRotationTime), blastTime / 2,
                RotateMode.FastBeyond360).SetEase(blastEase).OnComplete(() =>
            {
                isBlastAfterWordComplete = true;
                transform.rotation = Quaternion.Euler(Vector3.zero);
                isMoving = false;
            });
        }

        public void MoveTowardsGrid()
        {
            transform.position = _level.BottomOfScreenPoint();
            transform.DORotate(new Vector3(360f * blastRotationTime, 0f, 360f * blastRotationTime), blastTime / 2,
                RotateMode.FastBeyond360).SetEase(blastReturnEase);

            transform.DOMove(defaultGlobalGridPos, blastTime / 2).SetEase(blastReturnEase).OnComplete(() => { });

            StartCoroutine(ResetData(blastTime / 2));
        }


        public void DeckAnimation(float timeToPlaceGrids, Vector2 pos, string gridData, bool shouldReturn = true)
        {
            if (!LevelHandler.Instance.gridAvailableOnScreenList.Contains(this))
            {
                LevelHandler.Instance.gridAvailableOnScreenList.Add(this);
            }

            SoundManager.instance.PlaySound(SoundManager.SoundType.CardDeck);
            blastPos = pos;
            transform.DOMove(blastPos, timeToPlaceGrids / 2).SetEase(movingEase);
            transform.DOScale(defaultGridSize, timeToPlaceGrids / 2).SetEase(movingEase);
            transform.DORotate(new Vector3(moveRotationTimes * -360f, 0f, -360f * moveRotationTimes),
                timeToPlaceGrids / 2,
                RotateMode.FastBeyond360).SetEase(movingEase).OnComplete(() =>
            {
                if (!shouldReturn)
                    return;

                GridTextData = gridData;
                transform.rotation = Quaternion.Euler(Vector3.zero);
                ObjectStatus(true);
                isBlastAfterWordComplete = false;
                transform.DORotate(new Vector3(360f * moveRotationTimes, 0f, 360f * moveRotationTimes),
                    timeToPlaceGrids / 2,
                    RotateMode.FastBeyond360).SetEase(movingEase);

                transform.DOLocalMove(defaultLocalGridPos, timeToPlaceGrids / 2).SetEase(movingEase)
                    .OnComplete(() => { });
            });

            StartCoroutine(ResetData(timeToPlaceGrids));
        }


        public IEnumerator ResetData(float time = 0f)
        {
            yield return new WaitForSeconds(time);
            isSelected = false;
            isMoving = false;
        }

        private void OnDestroy()
        {
            DOTween.Kill(this, false);
        }
    }
}