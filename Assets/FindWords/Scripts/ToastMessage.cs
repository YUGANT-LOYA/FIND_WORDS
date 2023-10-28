using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ToastMessage : MonoBehaviour
{
    [SerializeField] GameObject hintMessageGm, newWordFoundGm;
    [SerializeField] GameObject toastMessageCanvasGm;
    [SerializeField] string noHintMessage;
    public float toastMsgTime = 0.5f;

    public void ShowHintToast()
    {
        StopCoroutine(nameof(ToastAnimation));
        //toastMessage.SetActive(false);
        hintMessageGm.SetActive(true);
        StartCoroutine(ToastAnimation(hintMessageGm));
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