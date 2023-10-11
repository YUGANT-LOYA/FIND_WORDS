using TMPro;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    public class QuesTile : MonoBehaviour
    {
        [SerializeField] TextMeshPro quesText;
        public bool isAssigned;
        private LevelHandler _levelHandler;

        public string QuesTextData
        {
            get => quesText.text;
            private set => quesText.text = value;
        }

        public void SetLevelHandler(LevelHandler handler)
        {
            _levelHandler = handler;
        }

        public LevelHandler GetLevelHandler()
        {
            return _levelHandler;
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