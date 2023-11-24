using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    public class QuesTile : MonoBehaviour
    {
        [SerializeField] private TextMeshPro quesText, lockText;
        public int id;
        public GameObject lockGm;
        private int _unlockAmount;
        public bool isAssigned;
        public bool isUnlocked = true;

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
        }

        private void DeActivateLock()
        {
            lockGm.SetActive(false);
            isUnlocked = true;
            DataHandler.UnlockedQuesLetter++;
            LevelHandler.Instance.quesTileList.Add(this);
        }

        public void ActivateObject()
        {
            gameObject.SetActive(true);
        }

        public void OnMouseDown()
        {
            Debug.Log("Ques Tile Clicked !");

            if (isUnlocked) return;

            if (DataHandler.TotalCoin < _unlockAmount)
            {
                SoundManager.instance.PlaySound(SoundManager.SoundType.LockGridClicked);
                Vibration.Vibrate(20);
                //ShakeAnim();
            }
            else
            {
                SoundManager.instance.PlaySound(SoundManager.SoundType.NewGridUnlock);
                Vibration.Vibrate(20);
                LevelHandler.Instance.SetLevelRunningBool(false);
                DataHandler.NewQuesGridUnlockIndex++;
                DeActivateLock();
                StartCoroutine(UIManager.Instance.UpdateReducedCoinText(0f, _unlockAmount, 0.5f));
                UIManager.SetCoinData(_unlockAmount, -1);

                UnlockNewQuesGrid();
                
                LevelHandler.Instance.SetLevelRunningBool();
                //LevelHandler.Instance.UnlockGridForUpgrade();
            }
        }

        private void UnlockNewQuesGrid()
        {
            DataHandler.UnlockedQuesLetter = DataHandler.CurrTotalQuesSize;
            DataHandler.CurrTotalQuesSize++;
            GameController.instance.GetCurrentLevel().SetQuesGrid(DataHandler.CurrTotalQuesSize,true);
            LevelHandler.Instance.FillNewWordInWordLeftList();
        }


        // void ShakeAnim()
        // {
        //     DOTween.Kill(transform, false);
        //     transform.localPosition = defaultLocalTilePos;
        //     transform.DOShakePosition(lockShakeTime, shakeStrength, vibrationStrength, shakeRandomness)
        //         .SetEase(lockEase).OnComplete(() => { transform.localPosition = defaultLocalTilePos; });
        // }
    }
}