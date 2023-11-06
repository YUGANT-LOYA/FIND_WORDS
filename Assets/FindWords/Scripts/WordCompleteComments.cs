using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;
using UnityEngine.UI;

namespace YugantLoyaLibrary.FindWord
{
    public class WordCompleteComments : MonoBehaviour
    {
        public float wordFloatingTime = 1f, wordUpMovement = 5f;
        public Transform spawningAreaTrans;

        public enum WordCommentType
        {
            GOOD,
            GREAT,
            AMAZING,
            EXCELLENT,
            FANTASTIC
        }

        [Serializable]
        public struct WordCommentStruct
        {
            public WordCommentType wordType;
            public GameObject commentObj;
        }

        public List<WordCommentStruct> wordComments;


        public void PickRandomCommentForWordComplete(int correctWordCompleteInLine)
        {
            switch (correctWordCompleteInLine)
            {
                case 3:
                    CallWordComment(WordCommentType.GOOD);

                    break;

                case 4:
                    CallWordComment(WordCommentType.GREAT);

                    break;

                case 5:
                    CallWordComment(WordCommentType.AMAZING);

                    break;

                case 6:
                    CallWordComment(WordCommentType.EXCELLENT);

                    break;

                case 7:
                    CallWordComment(WordCommentType.FANTASTIC);

                    break;

                default:

                    int total = Enum.GetNames(typeof(WordCommentType)).Length;
                    int index = Random.Range(0, total);
                    WordCommentType wordType = (WordCommentType)(index);
                    CallWordComment(wordType);

                    break;
            }
        }

        private void CallWordComment(WordCommentType commentType)
        {
            float zRot = Random.Range(-25f, 25f);
            float xPosVal = Random.Range(-300f, 300f);
            float yPosVal = Random.Range(-10f, 10f);
            GameObject currObj = null;

            foreach (WordCommentStruct commentStruct in wordComments)
            {
                if (commentType == commentStruct.wordType)
                {
                    currObj = commentStruct.commentObj;
                    break;
                }
            }

            if (currObj != null)
            {
                var pos = spawningAreaTrans.position;
                currObj.transform.position = new Vector3(pos.x + xPosVal,
                    pos.y + yPosVal, pos.z);

                currObj.SetActive(true);
                var rotation = currObj.transform.rotation;
                Quaternion quat = Quaternion.Euler(rotation.x, rotation.y, zRot);
                currObj.transform.rotation = quat;

                Image currImage = currObj.GetComponent<Image>();

                currImage.DOFade(0f, wordFloatingTime).OnComplete(() => { currObj.SetActive(false); });
                currObj.transform.DOMoveY(pos.y + wordUpMovement, wordFloatingTime);
            }
        }
    }
}