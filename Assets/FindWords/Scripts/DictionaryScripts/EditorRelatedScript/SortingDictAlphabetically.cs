using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class SortingDictAlphabetically : MonoBehaviour
{
   public TextAsset sortingTextFile;
   public bool sortAscending = true;

   [Button]
   public void SortFile()
   {
      if (sortAscending)
      {
         SortFileInAscending();
      }
      else
      {
         SortFileInDescending();
      }
   }

   void SortFileInAscending()
   {
      
   }
   
   void SortFileInDescending()
   {
      
   }
   
}
