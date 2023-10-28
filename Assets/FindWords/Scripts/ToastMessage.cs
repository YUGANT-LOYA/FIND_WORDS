using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToastMessage : MonoBehaviour
{
    [SerializeField] GameObject toastMessage;
    [SerializeField] GameObject toastMessageCanvasGm;
    [SerializeField] string noHintMessage;
    public float toastMsgTime = 0.5f;

    private void Start()
    {
    }

    public void ShowHintToast()
    {
        //toastMessage.SetActive(false);
        toastMessageCanvasGm.SetActive(true);

        StartCoroutine(ToastAnimation());
    }

    IEnumerator ToastAnimation()
    {
        yield return new WaitForSeconds(toastMsgTime);
        toastMessageCanvasGm.SetActive(false);
    }
}