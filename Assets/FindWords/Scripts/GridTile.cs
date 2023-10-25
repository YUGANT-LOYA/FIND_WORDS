using System.Collections;
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
        public Material lockMaterial;
        [SerializeField] private GameObject currlockGm, otherLockGm;
        [SerializeField] private TextMeshPro gridText;
        public Material gridMaterial;
        [HideInInspector] public Vector3 defaultGridSize, defaultGridPos;
        public Vector3 blastPos;
        public bool isGridActive = true, isSelected, isBlastAfterWordComplete;
        [SerializeField] private Vector2Int id;
        private LevelHandler _levelHandler;
        public QuesTile placedOnQuesTile;

        [Header("Movement Info")] public bool isMoving;
        public int moveRotationTimes = 1;
        public Ease movingEase = Ease.Linear;
        public float reachTime = 1f;

        [Header("Blasting Info")] public Ease blastEase = Ease.InCirc;
        public int blasRotationTime = 1;
        public float blastEffectAfterTime = 0.5f, blastTime = 1f;
        public Ease blastReturnEase = Ease.OutCirc;

        [Header("Lock Info")] public TextMeshPro amountToUnlockText;
        public bool isFullLocked, isCurrLock;
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

        public void DefaultGridData(Vector3 pos)
        {
            defaultGridSize = transform.localScale;
            defaultGridPos = pos;
        }

        public void ObjectStatus(bool isActive)
        {
            cube.GetComponent<MeshRenderer>().enabled = isActive;
            GetComponent<BoxCollider>().enabled = isActive;
            gridText.gameObject.SetActive(isActive);
        }

        public void AssignInfo(Level currLevel)
        {
            _levelHandler = GameController.instance.GetLevelHandler();
            _level = currLevel;
        }

        public void OnMouseDown()
        {
            if (!_levelHandler.GetLevelRunningBool() || isMoving)
                return;

            if (isCurrLock && DataHandler.TotalCoin >= _levelHandler.coinToUnlockNextGrid)
            {
                isFullLocked = false;
                isGridActive = true;
                isCurrLock = false;
                DataHandler.UnlockGridIndex++;
                _levelHandler.unlockedGridList.Add(this);
                _levelHandler.totalBuyingGridList.Remove(this);
                _levelHandler.gridAvailableOnScreenList.Add(this);
                UIManager.SetCoinData(_levelHandler.coinToUnlockNextGrid, -1);
                StartCoroutine(UIManager.instance.UpdateReducedCoinText(0f, _levelHandler.coinToUnlockNextGrid, 0.5f));
                NewGridUnlockAnimation(_levelHandler.coinToUnlockNextGrid);
                return;
            }

            if (isFullLocked || isCurrLock)
            {
                ShakeAnim();
                return;
            }

            if (!isGridActive)
                return;

            if (_levelHandler.LastQuesTile == null)
            {
                SoundManager.instance.PlaySound(SoundManager.SoundType.ClickSound);
                Debug.Log($"Grid {gameObject.name} Clicked !");
                isSelected = !isSelected;
                Debug.Log("Is Selected : " + isSelected);

                if (isSelected)
                {
                    _levelHandler.onNewLetterAddEvent?.Invoke(this);
                    isSelected = true;
                }
                else
                {
                    _levelHandler.onRemoveLetterEvent?.Invoke(this);
                    isSelected = false;
                }
            }
        }

        void ShakeAnim()
        {
            DOTween.Kill(transform, false);
            transform.position = defaultGridPos;
            transform.DOShakePosition(lockShakeTime, shakeStrength, vibrationStrength, shakeRandomness)
                .SetEase(lockEase).OnComplete(() => { transform.position = defaultGridPos; });
        }

        private void NewGridUnlockAnimation(int coinToSubtract)
        {
            isMoving = true;

            FillData();

            float time = UIManager.instance.coinAnimTime;
            //StartCoroutine(UIManager.instance.UpdateReducedCoinText(0f, coinToSubtract, time));
            transform.DORotate(
                    new Vector3(unlockRotationTime * 360f, transform.rotation.eulerAngles.y,
                        transform.rotation.eulerAngles.z),
                    unlockTime, RotateMode.FastBeyond360)
                .SetEase(movingEase).OnComplete(() =>
                {
                    DataHandler.CoinGridUnlockIndex++;
                    _levelHandler.UnlockNextGridForCoins();
                    DeactivateLockStatus();
                    cube.GetComponent<Renderer>().material = new Material(gridMaterial);
                    gridText.gameObject.SetActive(true);
                    isMoving = false;
                    _levelHandler.CheckAllGridBuyed();
                });
        }


        //Filling Letter According to the word in WordLeftList.
        void FillData()
        {
            char letterToFill = _levelHandler.GetUnAvailableLetterInRandomWord();

            if (letterToFill != ' ')
            {
                GridTextData = letterToFill.ToString();
            }
        }

        public void MoveAfter(Vector3 pos, bool isMovingToQues, QuesTile quesTile, float time = 0f)
        {
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
                placedOnQuesTile.RevertData();
                isSelected = false;
                transform.DOScale(defaultGridSize, reachTime);
            }

            var rotation = transform.rotation;
            transform.DORotate(
                    new Vector3(moveRotationTimes * 360f, moveRotationTimes * 360f, rotation.eulerAngles.z),
                    reachTime, RotateMode.FastBeyond360)
                .SetEase(movingEase);

            SetQuesTileStatus(quesTile, isMovingToQues, reachTime / 2);

            transform.DOMove(pos, reachTime).SetEase(movingEase).OnComplete(() =>
            {
                if (_levelHandler.LastQuesTile == quesTile)
                {
                    _levelHandler.CheckAnswer();
                }

                isMoving = false;
            });
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
            transform.DORotate(new Vector3(blasRotationTime * -360f, 0f, -360f * blasRotationTime), blastTime / 2,
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
            transform.DORotate(new Vector3(360f * blasRotationTime, 0f, 360f * blasRotationTime), blastTime / 2,
                RotateMode.FastBeyond360).SetEase(blastReturnEase);

            transform.DOMove(defaultGridPos, blastTime / 2).SetEase(blastReturnEase).OnComplete(() => { });

            StartCoroutine(ResetData(blastTime / 2));
        }


        public void DeckAnimation(float timeToPlaceGrids, Vector2 pos, string gridData, bool shouldReturn = true)
        {
            if (!_levelHandler.gridAvailableOnScreenList.Contains(this))
            {
                _levelHandler.gridAvailableOnScreenList.Add(this);
            }

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

                transform.DOMove(defaultGridPos, timeToPlaceGrids / 2).SetEase(movingEase).OnComplete(() => { });
            });

            StartCoroutine(ResetData(timeToPlaceGrids));
        }


        IEnumerator ResetData(float time)
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