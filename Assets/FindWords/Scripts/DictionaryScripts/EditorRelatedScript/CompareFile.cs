using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine.Serialization;

public class CompareFile : MonoBehaviour
{
    public TextAsset file1, file2;
    public int count = 0;
    public bool shouldCreateNewFile = true;

    [Tooltip("If You ned to check whether the data is exact same, each line is in proper order or not")]
    public bool sameAsLineToLine;

    public string comparedFileName = "ComparedFile_1";

    [Tooltip("Location should be inside the Resources Folder")]
    public string folderLocation = "FindWords/ComparedFileFolder";

    [SerializeField] private List<string> file1List, file2List, mainDataList;


    private void Start()
    {
        CompareFiles();
    }


    [Button]
    private void CompareFiles()
    {
        if (file1 == null || file2 == null)
        {
            Debug.LogError("File Missing !");
        }

        FillLists();
        CompareData();
        
        StringBuilder mainData = SaveDifferentData();

        Save(mainData.ToString());
    }

    private StringBuilder SaveDifferentData()
    {
        StringBuilder mainData = new StringBuilder();
        
        foreach (string str in mainDataList)
        {
            mainData.AppendLine(str);
        }
        
        Debug.Log("Main String : " + mainData.ToString());
        return mainData;
    }

    private void Save(string mainData)
    {
        string filepath = "";

        string directoryPath = Application.dataPath + $"/{folderLocation}";

        Debug.Log("Directory Path : " + directoryPath);

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        if (shouldCreateNewFile)
        {
            filepath = Application.dataPath + $"/{folderLocation}/{comparedFileName}.txt";
            
        }
        else
        {
            filepath = Application.dataPath + $"/{folderLocation}/{comparedFileName}_Duplicate.txt";
        }

        Debug.Log("File Path : " + filepath);



        if (shouldCreateNewFile || !File.Exists(filepath))
        {
            //File.WriteAllText(filepath, string.Empty);
            File.WriteAllText(filepath, mainData);
        }
        else
        {
            Debug.LogError("File Already Exists !");
        }

#if UNITY_EDITOR
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
#endif
    }

    private void CompareData()
    {
        List<string> tempFile1List = new List<string>(file1List);
        List<string> tempFile2List = new List<string>(file2List);

        for (var index = tempFile1List.Count - 1; index >= 0; index--)
        {
            string word = tempFile1List[index];

            if (tempFile2List.Contains(word))
            {
                tempFile2List.Remove(word);
                tempFile1List.Remove(word);
            }
        }


        mainDataList = new List<string>(tempFile1List);

        foreach (string word in tempFile2List)
        {
            mainDataList.Add(word);
        }

        if (tempFile2List.Count <= 0 && tempFile1List.Count <= 0)
        {
            Debug.Log("Exact Same Files");
            mainDataList.Clear();
        }
    }

    void FillLists()
    {
        file1List = new List<string>();
        file2List = new List<string>();

        string[] data1 = file1.text.Split('\n');
        string[] data2 = file2.text.Split('\n');

        foreach (string line in data1)
        {
            string word = line.Trim();

            if (!string.IsNullOrWhiteSpace(word) || !string.IsNullOrEmpty(word))
            {
                file1List.Add(word);
            }
        }

        foreach (string line in data2)
        {
            string word = line.Trim();

            if (!string.IsNullOrWhiteSpace(word) || !string.IsNullOrEmpty(word))
            {
                file2List.Add(word);
            }
        }
    }

    [Button]
    void Clear()
    {
        count = 0;
        file2List.Clear();
        file1List.Clear();
        mainDataList.Clear();
    }
}