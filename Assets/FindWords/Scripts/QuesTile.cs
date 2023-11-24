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
        [SerializeField] private TextMeshPro quesText;
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
            isUnlocked = false;
            lockGm.SetActive(true);
        }

        private void DeActivateLock()
        {
            lockGm.SetActive(false);
            isUnlocked = true;
            DataHandler.UnlockedQuesLetter = DataHandler.CurrTotalQuesSize;
        }

        public void ActivateObject()
        {
            gameObject.SetActive(true);
        }

        public void OnMouseDown()
        {
            Debug.Log("Ques Tile Clicked !");

            if (isUnlocked) return;

            if (DataHandler.TotalCoin < LevelHandler.Instance.coinToUnlockNextGrid)
            {
                SoundManager.instance.PlaySound(SoundManager.SoundType.LockGridClicked);
                //ShakeAnim();
            }
            else
            {
                LevelHandler.Instance.SetLevelRunningBool(false);
                DeActivateLock();
                StartCoroutine(UIManager.Instance.UpdateReducedCoinText(0f, 1000, 0.5f));
                UIManager.SetCoinData(1000, -1);
                LevelHandler.Instance.UnlockGridForUpgrade();
            }
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