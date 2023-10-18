using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    public class QuesTile : MonoBehaviour
    {
        [SerializeField] TextMeshPro quesText, coinToUnlockText;
        [SerializeField] private GameObject lockedParent;
        public int unlockRotationTime = 1;
        public float unlockTime = 0.75f;
        public Ease rotatingEase = Ease.OutBack;
        private int unlockAmount;
        public bool isAssigned;
        private LevelHandler _levelHandler;
        public bool isUnlocked = true;

        public string QuesTextData
        {
            get => quesText.text;
            set => quesText.text = value;
        }

        public void SetLevelHandler(LevelHandler handler)
        {
            _levelHandler = handler;
        }

        public LevelHandler GetLevelHandler()
        {
            return _levelHandler;
        }

        public void AddData(string str)
        {
            QuesTextData = str;
        }

        public void RevertData()
        {
            isAssigned = false;
            QuesTextData = "";
            Invoke(nameof(DeActivateObject), 0.2f);
        }

        void DeActivateObject()
        {
            gameObject.SetActive(true);
        }

        public void IsLocked(bool isActive)
        {
            quesText.gameObject.SetActive(!isActive);
            lockedParent.gameObject.SetActive(isActive);
        }

        public void SetUnlockCoinAmount(int amount)
        {
            coinToUnlockText.text = amount.ToString();
            unlockAmount = amount;
        }

        public void OnMouseDown()
        {
            if (!isUnlocked && DataHandler.TotalCoin > unlockAmount)
            {
                isUnlocked = true;
                IsLocked(false);
                NewGridUnlockAnimation(unlockAmount);
            }
        }

        private void NewGridUnlockAnimation(int coinToSubtract)
        {
            float time = UIManager.instance.coinAnimTime;
            UIManager.SetCoinData(coinToSubtract, -1);
            StartCoroutine(UIManager.instance.UpdateReducedCoinText(0f, coinToSubtract, time));
            var rotation = transform.rotation;
            transform.DORotate(
                    new Vector3(unlockRotationTime * 360f, rotation.eulerAngles.y,
                        rotation.eulerAngles.z),
                    unlockTime, RotateMode.FastBeyond360)
                .SetEase(rotatingEase).OnComplete(() =>
                {
                    DataHandler.UnlockedQuesLetter = DataHandler.CurrTotalQuesSize;
                });
        }
    }
}