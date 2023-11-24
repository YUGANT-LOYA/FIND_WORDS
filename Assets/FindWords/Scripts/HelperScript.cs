using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    public class HelperScript : MonoBehaviour
    {
        public int clickDealIndex = 4;
        public GameObject canvasHelperHand, gridHelperHand;
        public List<GridTile> clickingList;
        public List<int> gridIndexList;
        public List<GameObject> bgList;
        public List<Vector3> helperHandPosList;
        [SerializeField] private Canvas canvas;

        public void PlayHelper()
        {
            for (var index = 0; index < gridIndexList.Count; index++)
            {
                int val = gridIndexList[index];
                List<GridTile> totalGridList = new List<GridTile>(LevelHandler.Instance.totalGridsList);
                for (var i = 0; i < totalGridList.Count; i++)
                {
                    var gridTile = totalGridList[i];
                    if (val == i)
                    {
                        clickingList.Add(gridTile);
                    }
                }
            }

            LevelHandler.Instance.SetLevelRunningBool(true);
            UIManager.Instance.CanTouch(false);
            ClickTile(0.2f);
        }

        public void ClickTile(float time)
        {
            StartCoroutine(ClickTileAnim(time));
        }

        IEnumerator ClickTileAnim(float time)
        {
            yield return new WaitForSeconds(time);
            //Debug.Log("Tile Clicked !");
            if (DataHandler.HelperIndex < bgList.Count)
            {
                //Debug.Log("IF Helper  Index : " + DataHandler.HelperIndex);
                bgList[DataHandler.HelperIndex].SetActive(true);

                if (DataHandler.HelperIndex >= 1)
                {
                    //Debug.Log("Last Bg DeActivating !");
                    bgList[DataHandler.HelperIndex - 1].SetActive(false);
                }

                if (DataHandler.HelperIndex < clickDealIndex)
                {
                    gridHelperHand.SetActive(true);
                    gridHelperHand.transform.position = helperHandPosList[DataHandler.HelperIndex];
                    UIManager.Instance.CanTouch(false);
                    clickingList[DataHandler.HelperIndex].isHelperActivate = true;
                }
                else if (clickDealIndex == DataHandler.HelperIndex)
                {
                    gridHelperHand.SetActive(false);
                    canvasHelperHand.SetActive(true);
                    UIManager.Instance.CanTouch(true);
                }
                else
                {
                    gridHelperHand.SetActive(true);
                    gridHelperHand.transform.position = helperHandPosList[DataHandler.HelperIndex];
                    UIManager.Instance.CanTouch(false);
                    clickingList[DataHandler.HelperIndex - 1].isHelperActivate = true;
                }
            }
            else
            {
                //Reset Game for Original Game !
                //Debug.Log("ELSE Helper  Index : " + DataHandler.HelperIndex);
                bgList[DataHandler.HelperIndex - 1].SetActive(false);
                DataHandler.HelperLevelCompleted = 1;
            }
        }
    }
}