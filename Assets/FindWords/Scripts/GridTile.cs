using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    public class GridTile : MonoBehaviour
    {
        private Level _level;
        public Ease movingEase = Ease.Linear, blastEase = Ease.InCirc;
        public float reachTime = 1f, blastTime = 1f;
        public int moveRotationTimes = 1, blasRotationTime = 1;
        [SerializeField] private TextMeshPro gridText;
        public Material gridMaterial;
        [HideInInspector] public Vector3 defaultGridSize, defaultGridPos;
        public Vector3 blastPos;
        public Color defaultColor = Color.white, selectedColor = Color.cyan;
        public bool isGridActive = true, isSelected, isMoving;
        [SerializeField] private Vector2Int id;
        private LevelHandler _levelHandler;
        public QuesTile placedOnQuesTile;

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


        public void DefaultGridSize(Vector3 pos)
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

            Debug.Log($"Grid {gameObject.name} Clicked !");
            isSelected = !isSelected;
            Debug.Log("Is Selected : " + isSelected);

            if (isSelected)
            {
                gridMaterial.color = selectedColor;
                isSelected = true;
                _levelHandler.onNewLetterAddEvent?.Invoke(this);
            }
            else
            {
                gridMaterial.color = defaultColor;
                isSelected = false;
                _levelHandler.onRemoveLetterEvent?.Invoke(this);
            }
        }

        public void Move(Vector3 pos, bool isMovingToQues, QuesTile quesTile)
        {
            isMoving = true;
            SetQuesTileStatus(quesTile, isMovingToQues, reachTime / 2);
            var rotation = transform.rotation;
            transform.DORotate(
                    new Vector3(moveRotationTimes * 360f, moveRotationTimes * 360f, rotation.eulerAngles.z),
                    reachTime, RotateMode.FastBeyond360)
                .SetEase(movingEase);

            transform.DOMove(pos, reachTime).SetEase(movingEase).OnComplete(() =>
            {
                if (_levelHandler.LastQuesTile == quesTile)
                {
                    _levelHandler.CheckAnswer();
                }
                else
                {
                    _levelHandler.SetLevelRunningBool(true);
                }

                isMoving = false;
            });

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
        }

        private void SetQuesTileStatus(QuesTile quesTile, bool isMovingToQues, float time)
        {
            StartCoroutine(DisableQuesTile(quesTile, isMovingToQues, time));
        }

        IEnumerator DisableQuesTile(QuesTile quesTile, bool isMovingToQues, float time)
        {
            yield return new WaitForSeconds(time);
            quesTile.SetMeshRendererActiveStatus(!isMovingToQues);
        }

        public void Blast()
        {
            isMoving = true;
            blastPos = _level.GetRandomPointOutOfScreen();
            SetQuesTileStatus(placedOnQuesTile, false, blastTime / 2);
            transform.DOMove(blastPos, blastTime / 2).SetEase(blastEase);
            transform.DOScale(defaultGridSize, blastTime / 2).SetEase(movingEase);
            transform.DORotate(new Vector3(blasRotationTime * -360f, 0f, -360f * blasRotationTime), blastTime / 2,
                RotateMode.FastBeyond360).SetEase(blastEase).OnComplete(() =>
            {
                Vector2 randomPoint = _level.GetRandomPointOutOfScreen();
                transform.position = randomPoint;
                transform.rotation = Quaternion.Euler(Vector3.zero);
                GridTextData = _levelHandler.GenerateRandom_ASCII_Code();

                transform.DORotate(new Vector3(360f * blasRotationTime, 0f, 360f * blasRotationTime), blastTime / 2,
                    RotateMode.FastBeyond360).SetEase(blastEase);

                transform.DOMove(defaultGridPos, blastTime / 2).SetEase(blastEase).OnComplete(() =>
                {
                    Debug.Log("Cube Returning From Out of the Screen !");
                    ResetData();
                    if (_levelHandler.LastQuesTile == placedOnQuesTile)
                    {
                        _levelHandler.SetLevelRunningBool(true);
                    }
                });
            });
        }

        void ResetData()
        {
            isSelected = false;
            isMoving = false;

            Debug.Log("Is Moving : " + isMoving);
        }

        public void SetGridBg(bool isActive)
        {
            Color gridColor = gridMaterial.color;
            gridMaterial.color = isActive
                ? new Color(gridColor.r, gridColor.g, gridColor.b, 255f)
                : new Color(gridColor.r, gridColor.g, gridColor.b, 0f);
        }

        private void OnDestroy()
        {
            DOTween.Kill(this, false);
        }
    }
}