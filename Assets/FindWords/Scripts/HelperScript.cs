using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YugantLoyaLibrary.FindWords;

public class HelperScript : MonoBehaviour
{
    public int clickDealIndex = 4;
    public List<GridTile> clickingList;
    public List<int> gridIndexList;
    public List<GameObject> bgList;

    public void PlayHelper()
    {
        for (var index = 0; index < gridIndexList.Count; index++)
        {
            int val = gridIndexList[index];
            List<GridTile> totalGridList = new List<GridTile>(LevelHandler.instance.totalGridsList);
            for (var i = 0; i < totalGridList.Count; i++)
            {
                var gridTile = totalGridList[i];
                if (val == i)
                {
                    clickingList.Add(gridTile);
                }
            }
        }

        LevelHandler.instance.SetLevelRunningBool(true);
        UIManager.instance.CanTouch(false);
        ClickTile(0.2f);
    }

    public void ClickTile(float time)
    {
        StartCoroutine(ClickTileAnim(time));
    }

    IEnumerator ClickTileAnim(float time)
    {
        yield return new WaitForSeconds(time);
        Debug.Log("Tile Clicked !");
        if (DataHandler.HelperIndex < bgList.Count)
        {
            Debug.Log("IF Helper  Index : " + DataHandler.HelperIndex);
            bgList[DataHandler.HelperIndex].SetActive(true);

            if (DataHandler.HelperIndex >= 1)
            {
                Debug.Log("Last Bg DeActivating !");
                bgList[DataHandler.HelperIndex - 1].SetActive(false);
            }

            if (DataHandler.HelperIndex < clickDealIndex)
            {
                UIManager.instance.CanTouch(false);
                clickingList[DataHandler.HelperIndex].isHelperActivate = true;
            }
            else if (clickDealIndex == DataHandler.HelperIndex)
            {
                UIManager.instance.CanTouch(true);
            }
            else
            {
                UIManager.instance.CanTouch(false);
                clickingList[DataHandler.HelperIndex - 1].isHelperActivate = true;
            }
        }
        else
        {
            //Reset Game for Original Game !
            Debug.Log("ELSE Helper  Index : " + DataHandler.HelperIndex);
            bgList[DataHandler.HelperIndex - 1].SetActive(false);
            DataHandler.HelperLevelCompleted = 1;
        }
    }
}