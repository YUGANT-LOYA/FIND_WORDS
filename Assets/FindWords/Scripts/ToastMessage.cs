using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    public class ToastMessage : MonoBehaviour
    {
        [SerializeField] GameObject toastMsgGm, newWordFoundGm;
        [SerializeField] private TextMeshProUGUI toastMsgTxt;
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

        public void ShowNewWordFoundToast()
        {
            //toastMessage.SetActive(false);
            StopCoroutine(nameof(ToastAnimation));
            isToastActive = true;
            newWordFoundGm.SetActive(true);
            StartCoroutine(ToastAnimation(newWordFoundGm));
        }

        IEnumerator ToastAnimation(GameObject messageGm)
        {
            yield return new WaitForSeconds(toastMsgTime);

            if (isToastActive)
            {
                messageGm.SetActive(false);
            }
        }
    }
}