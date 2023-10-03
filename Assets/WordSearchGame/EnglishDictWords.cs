using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEditor;

public class EnglishDictWords : MonoBehaviour
{
    public TextAsset wordTextFile;
    public string inputString;
    public DictScriptableInfo dictionaryData;

#if UNITY_EDITOR
    public void UpdateEnglishDict()
    {
        if (wordTextFile != null && dictionaryData != null)
        {
            string[] words = wordTextFile.text.Split('\n');
            foreach (string word in words)
            {
                string cleanedWord = word.Trim().ToLower(); // Clean and convert to lowercase
                if (!string.IsNullOrEmpty(cleanedWord))
                {
                    dictionaryData.dictionaryTrie.Insert(cleanedWord);
                }
            }
        }
        else
        {
            Debug.LogError("Text file or DictionaryData not assigned.");
        }
    }
#endif
}
