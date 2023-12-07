using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace YugantLoyaLibrary.FindWords
{
    public class GridTile : MonoBehaviour
    {
        private Level _level;
        public GameObject cube;
        [SerializeField] private GameObject currlockGm, otherLockGm;
        [SerializeField] private TextMeshPro gridText;
        [HideInInspector] public Vector3 defaultGridSize;
        [HideInInspector] public Vector3 defaultLocalGridPos, defaultGlobalGridPos;
        [HideInInspector] public Quaternion defaultQuaternionRotation;
        [SerializeField] private BoxCollider boxCollider;
        [SerializeField] private MeshRenderer cubeMeshRenderer;
        public Vector3 blastPos;

        public bool isHelperActivate = false,
            isGridActive = true,
            isSelected,
            isBlastAfterWordComplete,
            isFullLocked,
            isCurrLock;

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

        public void SetLockStatus()
        {
            if (isFullLocked)
            {
                FullLockStatus();
            }
            else if (isCurrLock)
            {
                CurrLockStatus();
            }
        }

        public void SetLockStatus(float time)
        {
            if (isFullLocked)
            {
                Invoke(nameof(FullLockStatus), time);
            }
            else if (isCurrLock)
            {
                Invoke(nameof(CurrLockStatus), time);
            }
        }

        void FullLockStatus()
        {
            //Debug.Log("Full Lock Status Called !");
            currlockGm.SetActive(false);
            otherLockGm.SetActive(true);
        }

        void CurrLockStatus()
        {
            //Debug.Log("Curr Lock Status Called !");
            currlockGm.SetActive(true);
            otherLockGm.SetActive(false);
        }

        public void DeactivateLockStatus()
        {
            Debug.Log("DeActivated All Locks !");
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
            cubeMeshRenderer.enabled = isActive;
            boxCollider.enabled = isActive;
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
                    UnlockGrid();
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

        private void UnlockGrid(bool calledByPlayer = true)
        {
            isFullLocked = false;
            isGridActive = true;
            isCurrLock = false;

            if (LevelHandler.Instance.totalBuyingGridList.Count == 1)
            {
                LevelHandler.Instance.lastLockedGrid = this;
            }

            DataHandler.UnlockGridIndex++;
            LevelHandler.Instance.UnlockingGrid(this);
            SoundManager.instance.PlaySound(SoundManager.SoundType.NewGridUnlock);

            if (calledByPlayer)
            {
                UIManager.SetCoinData(LevelHandler.Instance.coinToUnlockNextGrid, -1);
                StartCoroutine(
                    UIManager.Instance.UpdateReducedCoinText(0f, LevelHandler.Instance.coinToUnlockNextGrid));
            }

            NewGridUnlockAnimation(calledByPlayer);
        }

        void ShakeAnim()
        {
            DOTween.Kill(transform, false);
            transform.localPosition = defaultLocalGridPos;
            transform.DOShakePosition(lockShakeTime, shakeStrength, vibrationStrength, shakeRandomness)
                .SetEase(lockEase).OnComplete(() => { transform.localPosition = defaultLocalGridPos; });
        }

        private void NewGridUnlockAnimation(bool calledByPlayer = true)
        {
            isMoving = true;
            transform.rotation = defaultQuaternionRotation;

            if (calledByPlayer)
            {
                FillData();
            }
            else
            {
                int randomIndex = Random.Range(0, 2);
                GridTextData = randomIndex == 0
                    ? GameController.RandomVowel().ToString()
                    : GameController.RandomConsonent().ToString();
            }

            transform.DORotate(
                    new Vector3(unlockRotationTime * 360f, transform.rotation.eulerAngles.y,
                        transform.rotation.eulerAngles.z),
                    unlockTime, RotateMode.FastBeyond360)
                .SetEase(movingEase).OnComplete(() =>
                {
                    if (calledByPlayer)
                    {
                        DataHandler.CoinGridUnlockIndex++;
                        LevelHandler.Instance.UnlockNextGridForCoins();
                    }

                    DeactivateLockStatus();
                    gridText.gameObject.SetActive(true);

                    LevelHandler.Instance.noHintExist = false;
                    bool allGridBought = false;

                    if (LevelHandler.Instance.totalBuyingGridList.Count <= 0 &&
                        LevelHandler.Instance.lastLockedGrid == this)
                    {
                        allGridBought = LevelHandler.Instance.CheckAllGridBought();
                    }

                    if (!allGridBought)
                    {
                        LevelHandler.Instance.CheckWordExistOrNot(out bool hintButtonStatus, out string hintStr);
                    }

                    isMoving = false;

                    LionStudiosManager.LevelComplete(DataHandler.LevelNum, GameController.LevelAttempts, 0);
                    GAScript.LevelEnd(true, DataHandler.LevelNum.ToString());
                    GameController.LevelAttempts = 0;
                    DataHandler.LevelNum++;

                    LionStudiosManager.LevelStart(DataHandler.LevelNum, GameController.LevelAttempts, 0);
                    GAScript.LevelStart(DataHandler.LevelNum.ToString());
                });
        }

        public void RotateToOpen()
        {
            transform.DORotate(
                    new Vector3(unlockRotationTime * 360f, transform.rotation.eulerAngles.y,
                        transform.rotation.eulerAngles.z),
                    unlockTime, RotateMode.FastBeyond360)
                .SetEase(movingEase).OnComplete(() => { isMoving = false; });
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
                GameController.Instance.helper.gridHelperHand.SetActive(false);
            }

            StartCoroutine(Move(pos, isMovingToQues, quesTile, time));
        }

        IEnumerator Move(Vector3 pos, bool isMovingToQues, QuesTile quesTile, float time = 0f)
        {
            isMoving = true;
            string quesStr = LevelHandler.Instance.quesHintStr;
            yield return new WaitForSeconds(time);

            if (isMovingToQues)
            {
                placedOnQuesTile = quesTile;
                transform.DOScale(quesTile.transform.localScale, reachTime);
            }
            else
            {
                if (!string.IsNullOrEmpty(quesStr) && quesStr.Length == DataHandler.UnlockedQuesLetter)
                {
                    int id = placedOnQuesTile.id;
                    placedOnQuesTile.QuesTextData = quesStr[id].ToString();
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
                    GameController.Instance.helper.ClickTile(0.2f);
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

        public void MoveTowardsGrid(Action callback = null)
        {
            transform.position = _level.BottomOfScreenPoint();
            transform.DORotate(new Vector3(360f * blastRotationTime, 0f, 360f * blastRotationTime), blastTime / 2,
                RotateMode.FastBeyond360).SetEase(blastReturnEase);

            transform.DOMove(defaultGlobalGridPos, blastTime / 2).SetEase(blastReturnEase).OnComplete(() =>
            {
                callback?.Invoke();
            });

            StartCoroutine(ResetData(blastTime / 2));
        }


        public void DeckAnimation(float timeToPlaceGrids, Vector2 pos, string gridData, bool shouldReturn = true)
        {
            LevelHandler.AddGridToList(LevelHandler.Instance.gridAvailableOnScreenList, this);
            SoundManager.instance.PlaySound(SoundManager.SoundType.CardDeck);
            blastPos = pos;
            transform.DOMove(blastPos, timeToPlaceGrids / 2).SetEase(movingEase);
            transform.DOScale(defaultGridSize, timeToPlaceGrids / 2).SetEase(movingEase).OnComplete(() =>
            {
                transform.localScale = defaultGridSize;
            });
            transform.DORotate(new Vector3(moveRotationTimes * -360f, 0f, -360f * moveRotationTimes),
                timeToPlaceGrids / 2,
                RotateMode.FastBeyond360).SetEase(movingEase).OnComplete(() =>
            {
                if (!shouldReturn)
                    return;

                GridTextData = gridData;
                transform.rotation = Quaternion.Euler(Vector3.zero);
                // ObjectStatus(true);
                isBlastAfterWordComplete = false;
                transform.DORotate(new Vector3(360f * moveRotationTimes, 0f, 360f * moveRotationTimes),
                    timeToPlaceGrids / 2,
                    RotateMode.FastBeyond360).SetEase(movingEase);

                transform.DOLocalMove(defaultLocalGridPos, timeToPlaceGrids / 2).SetEase(movingEase)
                    .OnComplete(() =>
                    {
                        transform.localPosition = defaultLocalGridPos;
                    });
            });

            StartCoroutine(ResetData(timeToPlaceGrids));
        }


        private IEnumerator ResetData(float time = 0f)
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