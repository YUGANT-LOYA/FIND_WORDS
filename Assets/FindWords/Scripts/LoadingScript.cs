using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YugantLoyaLibrary.FindWords;

public class LoadingScript : MonoBehaviour
{
    public GameObject loadingGm, loadingCanvas;
    public Slider loadingBar;
    private float currFillAmountVal;


    private void Awake()
    {
        loadingGm.SetActive(true);
        loadingCanvas.SetActive(true);
        loadingBar.DOValue(1, 3f).OnComplete(() =>
        {
            Debug.Log("Loading Bar Destroyed !");
            GameController.instance.GetCurrentLevel().GridPlacement();
            UIManager.instance.CheckAllButtonStatus();
            Destroy(gameObject);
        });
    }
}