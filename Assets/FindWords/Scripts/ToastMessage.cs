using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace YugantLoyaLibrary.FindWords
{
    public class ToastMessage : MonoBehaviour
    {
        [SerializeField] GameObject toastMsgGm, newWordFoundGm;
        [SerializeField] private TextMeshProUGUI toastMsgTxt;
        [SerializeField] string noCoinForHintMsg, noWordFoundMsg, noShuffleFoundMsg;
        public float toastMsgTime = 0.5f;

        public void ShowHintToast()
        {
            StopCoroutine(nameof(ToastAnimation));
            toastMsgTxt.text = noCoinForHintMsg;
            toastMsgGm.SetActive(true);
            StartCoroutine(ToastAnimation(toastMsgGm));
        }

        public void ShowNoWordFoundToast()
        {
            StopCoroutine(nameof(ToastAnimation));
            toastMsgTxt.text = noWordFoundMsg;
            toastMsgGm.SetActive(true);
            StartCoroutine(ToastAnimation(toastMsgGm));
        }

        public void ShowNoShuffleFoundToast()
        {
            StopCoroutine(nameof(ToastAnimation));
            toastMsgTxt.text = noShuffleFoundMsg;
            toastMsgGm.SetActive(true);
            StartCoroutine(ToastAnimation(toastMsgGm));
        }

        public void ShowNewWordFoundToast()
        {
            //toastMessage.SetActive(false);
            StopCoroutine(nameof(ToastAnimation));
            newWordFoundGm.SetActive(true);
            StartCoroutine(ToastAnimation(newWordFoundGm));
        }

        IEnumerator ToastAnimation(GameObject messageGm)
        {
            yield return new WaitForSeconds(toastMsgTime);
            messageGm.SetActive(false);
        }
    }
}