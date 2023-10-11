using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    public class GridTile : MonoBehaviour
    {
        private Level _level;
        public Material lockMaterial;
        [SerializeField] private GameObject lockGm;
        [SerializeField] private TextMeshPro gridText;
        public Material gridMaterial;
        [HideInInspector] public Vector3 defaultGridSize, defaultGridPos;
        public Vector3 blastPos;
        public Color defaultColor = Color.white, selectedColor = Color.cyan;
        public bool isGridActive = true, isSelected;
        [SerializeField] private Vector2Int id;
        private LevelHandler _levelHandler;
        public QuesTile placedOnQuesTile;

        [Header("Movement Info")] public bool isMoving;
        public int moveRotationTimes = 1;
        public Ease movingEase = Ease.Linear;
        public float reachTime = 1f;

        [Header("Blasting Info")] public Ease blastEase = Ease.InCirc;
        public int blasRotationTime = 1;
        public float blastTime = 1f;
        public Ease blastReturnEase = Ease.OutCirc;

        [Header("Lock Info")] public TextMeshPro amountToUnlockText;
        public bool isLocked;
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
            get => gridText.text;
            set => gridText.text = value;
        }

        public TextMeshPro GetText()
        {
            return gridText;
        }

        public void SetLockStatus(bool isActive)
        {
            lockGm.SetActive(isActive);
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

        public void AssignInfo(Level currLevel)
        {
            _levelHandler = GameController.instance.GetLevelHandler();
            _level = currLevel;
        }

        public void OnMouseDown()
        {
            if (!_levelHandler.GetLevelRunningBool() || isMoving)
                return;

            if (DataHandler.instance.TotalCoin >= 100 && isLocked)
            {
                NewGridUnlockAnimation(100);
                return;
            }
            else if (isLocked)
            {
                ShakeAnim();
            }

            if (!isGridActive)
                return;

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
            //DOTween.Kill(transform, false);
            float time = UIManager.instance.coinAnimTime;
            StartCoroutine(UIManager.instance.UpdateReducedCoinText(0f, coinToSubtract, time));
            transform.DORotate(
                    new Vector3(unlockRotationTime * 360f, transform.rotation.eulerAngles.y,
                        transform.rotation.eulerAngles.z),
                    unlockTime, RotateMode.FastBeyond360)
                .SetEase(movingEase).OnComplete(() =>
                {
                    _levelHandler.buyGridList.Remove(this);
                    isGridActive = true;
                    isLocked = false;
                    SetLockStatus(false);
                    _levelHandler.unlockedGridList.Add(this);
                    GetComponent<Renderer>().material = new Material(gridMaterial);
                    gridText.gameObject.SetActive(true);
                    isMoving = false;
                    _levelHandler.CheckAllGridBuyed();
                });
        }

        public void Move(Vector3 pos, bool isMovingToQues, QuesTile quesTile)
        {
            isMoving = true;

            if (isMovingToQues)
            {
                placedOnQuesTile = quesTile;
                placedOnQuesTile.AddData(GridTextData);
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
            StartCoroutine(DisableQuesTile(quesTile, isMovingToQues, time));
        }

        IEnumerator DisableQuesTile(QuesTile quesTile, bool isMovingToQues, float time)
        {
            yield return new WaitForSeconds(time);
            quesTile.gameObject.SetActive(!isMovingToQues);
        }

        public void Blast()
        {
            blastPos = _level.GetRandomPointOutOfScreen();
            transform.DOMove(blastPos, blastTime / 2).SetEase(blastEase);
            transform.DOScale(defaultGridSize, blastTime / 2).SetEase(movingEase);
            transform.DORotate(new Vector3(blasRotationTime * -360f, 0f, -360f * blasRotationTime), blastTime / 2,
                RotateMode.FastBeyond360).SetEase(blastEase).OnComplete(() =>
            {
                _levelHandler.SetLevelRunningBool(true);
                transform.rotation = Quaternion.Euler(Vector3.zero);
                GridTextData = _levelHandler.GenerateRandom_ASCII_Code();

                CheckGridLeft();
            });
        }

        private void CheckGridLeft()
        {
            if (_levelHandler.wordCompletedGridList.Count == _levelHandler.unlockedGridList.Count)
            {
                foreach (GridTile gridTile in _levelHandler.unlockedGridList)
                {
                    gridTile.transform.position = _level.BottomOfScreenPoint();
                }

                _levelHandler.wordCompletedGridList.Clear();
                GameController.instance.ShuffleGrid();
            }
        }

        public void MoveTowardsGrid()
        {
            transform.position = _level.BottomOfScreenPoint();
            transform.DORotate(new Vector3(360f * blasRotationTime, 0f, 360f * blasRotationTime), blastTime / 2,
                RotateMode.FastBeyond360).SetEase(blastReturnEase);

            transform.DOMove(defaultGridPos, blastTime / 2).SetEase(blastReturnEase).OnComplete(() =>
            {
                //Debug.Log("Cube Returning From Out of the Screen !");
                // ResetData();
                // if (_levelHandler.LastQuesTile == placedOnQuesTile)
                // {
                //     _levelHandler.LastQuesTile = null;
                //     _levelHandler.SetLevelRunningBool(true);
                // }
            });

            StartCoroutine(ResetData(blastTime / 2));
        }


        public void DeckAnimation(float timeToPlaceGrids, Vector2 pos, bool shouldReturn = true)
        {
            blastPos = pos;
            transform.DOMove(blastPos, timeToPlaceGrids / 2).SetEase(movingEase);
            transform.DOScale(defaultGridSize, timeToPlaceGrids / 2).SetEase(movingEase);
            transform.DORotate(new Vector3(moveRotationTimes * -360f, 0f, -360f * moveRotationTimes),
                timeToPlaceGrids / 2,
                RotateMode.FastBeyond360).SetEase(movingEase).OnComplete(() =>
            {
                if (!shouldReturn)
                    return;

                transform.rotation = Quaternion.Euler(Vector3.zero);
                GridTextData = _levelHandler.GenerateRandom_ASCII_Code();

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
            _levelHandler.LastQuesTile = null;
            _levelHandler.SetLevelRunningBool(true);
            //Debug.Log("Is Moving : " + isMoving);
        }

        private void OnDestroy()
        {
            DOTween.Kill(this, false);
        }
    }
}