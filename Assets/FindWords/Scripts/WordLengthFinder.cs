using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using NaughtyAttributes;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WordLengthFinder : MonoBehaviour
{
    public int wordLength = 3, minLength = 3, maxLength = 8;
    public char letter;
    public TextAsset oxfordDictTextFile, wordLengthTextFile;
    public MainDictionary mainDict;

    [Tooltip("Minimum and Maximum Folder or word Length of which you have to add all data !")]
    public Vector2Int minMaxScriptableWordLength;

    [Button]
    public void FindWordLength()
    {
        string data = oxfordDictTextFile.text.Trim();
        string[] lines = data.Split('\n');

        StringBuilder csvContent = new StringBuilder();
        Debug.Log("Data Length : " + lines.Length);
        
        for (int i = 0; i < lines.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]) && lines[i].Trim().Length == wordLength)
            {
                Debug.Log("Str : " + lines[i]);
                csvContent.AppendLine(lines[i]);
            }
            
        }

        string assetsFolderPath = Application.dataPath;

// Specify the relative path within the "Assets" folder where you want to save the CSV file
        string relativeFilePath = "FindWords/Resources/WordDictData";
        string fileName = $"WordLength_{wordLength}.csv";

// Combine the paths to get the full path of the CSV file
        string folderPath = Path.Combine(assetsFolderPath, relativeFilePath);
        string filePath = Path.Combine(folderPath, fileName);

// Check if the directory exists, and if not, create it
        if (!Directory.Exists(folderPath))
        {
            Debug.Log("Folder Creating !");
            Directory.CreateDirectory(folderPath);
        }

// Save the CSV file
        if (!File.Exists(filePath))
        {
            Debug.Log("File Creating !!");
            File.WriteAllText(filePath, csvContent.ToString());
        }
        else
        {
            Debug.LogError($"File Already Exists with name {fileName} !!");
        }
// Refresh the Unity Asset Database to make the file visible in the Editor

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
        Debug.Log("CSV file saved to: " + filePath);
    }

    [Button]
    public void RemoveLetterOfLength()
    {
        string data = oxfordDictTextFile.text.Trim();
        string[] lines = data.Split('\n');

        StringBuilder csvContent = new StringBuilder();

        for (int i = 0; i < lines.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]) && lines[i].Length >= minLength && lines[i].Length <= maxLength)
            {
                csvContent.AppendLine(lines[i]);
            }
        }

        //Asset Folder Path
        string assetsFolderPath = Application.dataPath;

        // Specify the relative path within the "Assets" folder where you want to save the CSV file
        string relativeFilePath = $"FindWords/Resources/WordDictData/NewOxfordDict.csv";

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
#if UNITY_EDITOR
        // Refresh the Unity Asset Database to make the file visible in the Editor
        AssetDatabase.Refresh();
#endif
        Debug.Log("CSV file saved to: " + filePath);
    }

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
    void AddAllFilesToScriptable()
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

                wordInfo.wordText = textFile;
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
}