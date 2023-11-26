using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YugantLoyaLibrary.FindWords
{
    public class LoadingScript : MonoBehaviour
    {
        public GameObject loadingGm, loadingCanvas;
        public TextMeshProUGUI freeCoinText;
        public Slider loadingBar;
        private float currFillAmountVal;

        private void Awake()
        {
            loadingGm.SetActive(true);
            loadingCanvas.SetActive(true);

            if (DataHandler.FirstTimeGameOpen == 0)
            {
                freeCoinText.text = $"FREE GIFT : 100 COINS !!";
                freeCoinText.gameObject.SetActive(true);
            }

            loadingBar.DOValue(1, 3f).OnComplete(() =>
            {
                Debug.Log("Loading Bar Destroyed !");
                GameController.Instance.GetCurrentLevel().GridPlacement();

                Destroy(gameObject);
            });
        }
    }
}