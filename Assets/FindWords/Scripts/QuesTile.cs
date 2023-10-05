using TMPro;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    public class QuesTile : MonoBehaviour
    {
        [SerializeField] TextMeshPro quesText;
        public bool isAssigned;
        private MeshRenderer _meshRenderer;

        public string QuesTextData
        {
            get => quesText.text;
            private set => quesText.text = value;
        }

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        public void SetMeshRendererActiveStatus(bool setActive)
        {
            gameObject.SetActive(setActive);
            _meshRenderer.enabled = setActive;
            quesText.enabled = setActive;
        }

        public void AddData(string str)
        {
            isAssigned = true;
            QuesTextData = str;
        }

        public void RevertData()
        {
            isAssigned = false;
            QuesTextData = "";
        }
    }
}