using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    public class ToastMessage : MonoBehaviour
    {
        [SerializeField] GameObject toastMsgGm, newWordFoundGm;
        public GameObject newQuesTileUnlockedGm;
        [SerializeField] private TextMeshProUGUI toastMsgTxt,newWordFoundTxt, newQuesTileUnlockTxt;
        [SerializeField] string notEnoughCoinsMsg, noWordFoundMsg, noDealFoundMsg;
        public float toastMsgTime = 0.5f;
        public bool isToastActive;


        public void ShowNotEnoughCoinsToast()
        {
            StopCoroutine(nameof(ToastAnimation));
            isToastActive = true;
            toastMsgTxt.text = notEnoughCoinsMsg;
            toastMsgGm.SetActive(true);
            StartCoroutine(ToastAnimation(toastMsgGm));
        }

        public void ShowNoWordFoundToast()
        {
            StopCoroutine(nameof(ToastAnimation));
            isToastActive = true;
            toastMsgTxt.text = noWordFoundMsg;
            toastMsgGm.SetActive(true);
            StartCoroutine(ToastAnimation(toastMsgGm));
        }

        public void ShowNoDealFoundToast()
        {
            StopCoroutine(nameof(ToastAnimation));
            isToastActive = true;
            toastMsgTxt.text = noDealFoundMsg;
            toastMsgGm.SetActive(true);
            StartCoroutine(ToastAnimation(toastMsgGm));
        }

        public void ShowXLetterWordCanFormToast()
        {
            StopCoroutine(nameof(ToastAnimation));
            isToastActive = true;
            UIManager.Instance.boolNewQuesIntroduced = true;
            newQuesTileUnlockTxt.text = $"Form a Word using {DataHandler.UnlockedQuesLetter} Letters !!";
            newQuesTileUnlockedGm.SetActive(true);
            //StartCoroutine(ToastAnimation(newQuesTileUnlockedGm,5f));
        }

        public void ShowNewWordFoundToast()
        {
            //toastMessage.SetActive(false);
            StopCoroutine(nameof(ToastAnimation));
            isToastActive = true;
            newWordFoundTxt.text = $"New Word Found {LevelHandler.Instance.coinPerWord} + {LevelHandler.Instance.extraCoinForNewWord} = {LevelHandler.Instance.coinPerWord + LevelHandler.Instance.extraCoinForNewWord} ";
            newWordFoundGm.SetActive(true);
            StartCoroutine(ToastAnimation(newWordFoundGm));
        }

        public void MakeToastMessageDisappear(GameObject messageGm, float timeToWait = -1f)
        {
            StartCoroutine(ToastAnimation(messageGm, timeToWait));
        }

        IEnumerator ToastAnimation(GameObject messageGm, float timeToWait = -1f)
        {
            if (Math.Abs(timeToWait - (-1f)) < 0.1f)
            {
                timeToWait = toastMsgTime;
            }

            yield return new WaitForSeconds(timeToWait);

            if (isToastActive)
            {
                messageGm.SetActive(false);
            }
        }
    }
}