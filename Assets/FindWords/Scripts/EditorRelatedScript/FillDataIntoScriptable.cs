using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FillDataIntoScriptable : MonoBehaviour
{
    public int wordLength = 3, minLength = 3, maxLength = 8;
    public TextAsset oxfordDictTextFile, wordLengthTextFile;
    public MainDictionary mainDict;
    public PickWordDataInfo pickWordDataInfo;

    [Tooltip("Minimum and Maximum Folder or word Length of which you have to add all data !")]
    public Vector2Int minMaxScriptableWordLength;

    [Button]
    void FindWordAccordingToLetter()
    {
        string data = wordLengthTextFile.text.Trim();
        string[] lines = data.Split('\n');

        //char currLetter = letter;

        for (int i = 97; i < 123; i++)
        {
            char currletter = (char)i;
            StringBuilder csvContent = new StringBuilder();
            foreach (var str in lines)
            {
                if (!string.IsNullOrWhiteSpace(str) &&
                    str[0].ToString().ToUpper() == currletter.ToString().ToUpper())
                {
                    csvContent.AppendLine(str.Trim());
                }
            }

            string assetsFolderPath = Application.dataPath;

            // Specify the relative path within the "Assets" folder where you want to save the CSV file
            string relativeFilePath =
                $"FindWords/Resources/WordDictData/{wordLength}_WordLengthFolder/{currletter.ToString()}_Words.csv";

            // Combine the paths to get the full path of the CSV file
            string filePath = Path.Combine(assetsFolderPath, relativeFilePath);

            // Save the CSV file

            if (!string.IsNullOrEmpty(filePath) && !File.Exists(filePath))
            {
                Debug.Log("File Creating !!");
                File.WriteAllText(filePath, csvContent.ToString());
            }
            else
            {
                Debug.LogError($"File Already Exists with name WordLength_{wordLength} !!");
            }

            if (!Directory.Exists(filePath))
            {
                Debug.Log("Folder Creating !");
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            // Refresh the Unity Asset Database to make the file visible in the Editor
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            Debug.Log("CSV file saved to: " + filePath);
        }
    }

    [Button]
    void FillMainDictionaryInfo()
    {
        MainDictionary dict = mainDict;

        List<MainDictionary.MainDictionaryInfo> dictInfoList = new List<MainDictionary.MainDictionaryInfo>();

        for (int i = minMaxScriptableWordLength.x;
             i <= minMaxScriptableWordLength.y;
             i++)
        {
            MainDictionary.MainDictionaryInfo mainDictionaryInfo = new MainDictionary.MainDictionaryInfo();

            List<MainDictionary.WordLengthDetailedInfo> dictWordInfoList =
                new List<MainDictionary.WordLengthDetailedInfo>();

            for (int k = 97; k < 123; k++)
            {
                MainDictionary.WordLengthDetailedInfo wordInfo = new MainDictionary.WordLengthDetailedInfo();

                char tempChar = (char)k;
                //Debug.Log("File Creating !!");

                TextAsset textFile =
                    Resources.Load<TextAsset>($"WordDictData/{i}_WordLengthFolder/{tempChar}_Words");

                string[] lines = textFile.text.Trim().Split('\n');

                List<string> fillStringList = new List<string>();

                foreach (var str in lines)
                {
                    fillStringList.Add(str);
                }

                wordInfo.wordList = fillStringList;
                wordInfo.wordStartChar = tempChar;

                dictWordInfoList.Add(wordInfo);
            }

            mainDictionaryInfo.wordLength = i;
            mainDictionaryInfo.wordsInfo = dictWordInfoList;

            //Debug.Log("mainDictionaryInfo.wordLength : " + mainDictionaryInfo.wordLength);
            dictInfoList.Add(mainDictionaryInfo);
        }


        dict.dictInfoList = dictInfoList;
        mainDict = dict;

#if UNITY_EDITOR
        EditorUtility.SetDirty(mainDict);
        AssetDatabase.Refresh();
#endif
    }

    public static void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n - 1; i++)
        {
            int j = UnityEngine.Random.Range(i, n);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    [Button]
    void ShuffleTextFile()
    {
        string[] lines = wordLengthTextFile.text.Trim().Split('\n');
        StringBuilder csvContent = new StringBuilder();
        List<string> wordList = new List<string>();


        foreach (string str in lines)
        {
            wordList.Add(str.Trim());
        }

        ShuffleList(wordList);

        foreach (var str in wordList)
        {
            if (!string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str))
            {
                csvContent.AppendLine(str.Trim());
            }
        }

        string assetsFolderPath = Application.dataPath;

        // Specify the relative path within the "Assets" folder where you want to save the CSV file
        string relativeFilePath =
            $"FindWords/CSV/ShuffledWordFolder/{wordLength}_Shuffled_Words.csv";

        // Combine the paths to get the full path of the CSV file
        string filePath = Path.Combine(assetsFolderPath, relativeFilePath);

        // Save the CSV file

        if (!string.IsNullOrEmpty(filePath) && !File.Exists(filePath))
        {
            Debug.Log("File Creating !!");
            File.WriteAllText(filePath, csvContent.ToString());
        }
        else
        {
            Debug.LogError($"File Already Exists with name {wordLength}_Shuffled_Words !!");
        }

        if (!Directory.Exists(filePath))
        {
            Debug.Log("Folder Creating !");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        }

        // Refresh the Unity Asset Database to make the file visible in the Editor
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
        Debug.Log("CSV file saved to: " + filePath);
    }

    [Button]
    void FillPickingDataInfo()
    {
        PickWordDataInfo wordDataInfo = pickWordDataInfo;

        List<PickWordDataInfo.PickingDataInfo> pickDataList = new List<PickWordDataInfo.PickingDataInfo>();

        for (int i = minMaxScriptableWordLength.x;
             i <= minMaxScriptableWordLength.y;
             i++)
        {
            List<string> wordList = new List<string>();
            PickWordDataInfo.PickingDataInfo pickData = new PickWordDataInfo.PickingDataInfo();
            TextAsset textFile =
                Resources.Load<TextAsset>($"PredefinedWords/{i}_Letter");

            string[] lines = textFile.text.Trim().Split('\n');

            foreach (string s in lines)
            {
                wordList.Add(s);
            }

            pickData.wordList = wordList;
            pickData.quesLetterCount = i;

            pickDataList.Add(pickData);
        }

        wordDataInfo.pickDataInfoList = pickDataList;
        pickWordDataInfo = wordDataInfo;

#if UNITY_EDITOR
        EditorUtility.SetDirty(pickWordDataInfo);
        AssetDatabase.Refresh();
#endif
    }
}