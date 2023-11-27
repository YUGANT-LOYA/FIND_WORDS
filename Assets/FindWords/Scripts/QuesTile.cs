using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    public class QuesTile : MonoBehaviour
    {
        [SerializeField] private TextMeshPro quesText, lockText;
        public int id;
        public GameObject lockGm;
        public Vector3 defaultTileScale;
        private int _unlockAmount;
        public bool isAssigned;
        public Ease rotationEase = Ease.Linear;
        public bool isUnlocked = true;
        public int unlockRotationTime = 2;
        public float unlockTime = 0.5f;

        public string QuesTextData
        {
            get => quesText.text;
            set => quesText.text = value;
        }

        public void AddData(string str)
        {
            QuesTextData = str;
        }

        public void RevertData()
        {
            isAssigned = false;
            QuesTextData = "";
            Invoke(nameof(ActivateObject), 0.2f);
        }

        public void LockQuesTile(int amount)
        {
            _unlockAmount = amount;
            isUnlocked = false;
            lockGm.SetActive(true);
            lockText.text = amount.ToString();
            LevelHandler.Instance.currQuesLockTile = this;
            RotateToOpen(0.5f);
        }

        private void DeActivateLock()
        {
            lockGm.SetActive(false);
            isUnlocked = true;
            DataHandler.UnlockedQuesLetter++;
            LevelHandler.Instance.quesTileList.Add(this);
            RotateToOpen();
        }

        public void ActivateObject()
        {
            gameObject.SetActive(true);
        }

        public void OnMouseDown()
        {
            if (!LevelHandler.Instance.GetLevelRunningBool() || isUnlocked || DataHandler.HelperLevelCompleted == 0)
                return;

            Debug.Log("Ques Tile Clicked !");
            if (DataHandler.TotalCoin < _unlockAmount)
            {
                SoundManager.instance.PlaySound(SoundManager.SoundType.LockGridClicked);
                UIManager.Instance.toastMessageScript.ShowNotEnoughCoinsToast();
                Vibration.Vibrate(20);
                //ShakeAnim();
            }
            else
            {
                //LevelHandler.Instance.SetLevelRunningBool(false);
                UIManager.Instance.CanTouch(false);
                SoundManager.instance.PlaySound(SoundManager.SoundType.NewGridUnlock);
                Vibration.Vibrate(20);
                DataHandler.NewQuesGridUnlockIndex++;
                DeActivateLock();
                Invoke(nameof(UnlockNewQuesGrid), (float)unlockRotationTime / 2);
                CheckGridsAvailableForMakingWord();
                LevelHandler.Instance.SetLevelRunningBool(false);
                LevelHandler.Instance.RevertGridBackToPosAndCleanQuesTilesData();
                StartCoroutine(UIManager.Instance.UpdateReducedCoinText(0f, _unlockAmount));
                UIManager.SetCoinData(_unlockAmount, -1);
            }
        }

        void CheckGridsAvailableForMakingWord()
        {
            //LevelHandler.Instance.SetLevelRunningBool();
            bool isGridLeft = LevelHandler.Instance.CheckGridLeft();
            Debug.Log("Grid Left Bool : " + isGridLeft);
            Debug.Log("Level Running Bool  " + LevelHandler.Instance.GetLevelRunningBool());
            if (LevelHandler.Instance.gridAvailableOnScreenList.Count < DataHandler.UnlockedQuesLetter && isGridLeft)
            {
                Debug.Log("Deal Called From Unlocking Ques Tile !");
                GameController.Instance.Deal(false);
            }
        }

        private void UnlockNewQuesGrid()
        {
            GameController.Instance.confettiParticleSystem.gameObject.SetActive(false);
            DataHandler.UnlockedQuesLetter = DataHandler.CurrTotalQuesSize;
            DataHandler.CurrTotalQuesSize++;
            UIManager.Instance.toastMessageScript.ShowXLetterWordCanFormToast();
            GameController.Instance.GetCurrentLevel().SetQuesGrid(DataHandler.CurrTotalQuesSize);
            LevelHandler.Instance.FillNewWordInWordLeftList();

            if (DataHandler.UnlockedQuesLetter == DataHandler.CurrTotalQuesSize) return;

            Transform lockedTileTrans = LevelHandler.Instance.currQuesLockTile.transform;
            lockedTileTrans.localScale = Vector3.zero;
            lockedTileTrans.DOScale(defaultTileScale, 0.5f).OnComplete(() =>
            {
                transform.localScale = defaultTileScale;
            });

            GameController.Instance.confettiParticleSystem.transform.position = lockedTileTrans.position;
            GameController.Instance.confettiParticleSystem.gameObject.SetActive(true);
        }

        void RotateToOpen(float time = -1f)
        {
            if (Math.Abs(time - (-1f)) < 0.1f)
            {
                time = unlockTime;
            }

            transform.DORotate(
                    new Vector3(unlockRotationTime * 360f, transform.rotation.eulerAngles.y,
                        transform.rotation.eulerAngles.z),
                    time, RotateMode.FastBeyond360)
                .SetEase(rotationEase).OnComplete(() => { });
        }
    }
}