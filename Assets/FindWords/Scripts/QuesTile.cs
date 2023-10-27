using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    public class QuesTile : MonoBehaviour
    {
        [SerializeField] private TextMeshPro quesText;
        private int unlockAmount;
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
            Invoke(nameof(DeActivateObject), 0.2f);
        }

        void DeActivateObject()
        {
            gameObject.SetActive(true);
        }
    }
}