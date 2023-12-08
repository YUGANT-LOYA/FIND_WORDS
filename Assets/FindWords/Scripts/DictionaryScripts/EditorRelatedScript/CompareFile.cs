using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class CompareFile : MonoBehaviour
{
    public int compareFileNum = 1;
    public TextAsset file1, file2;
    public int count = 0;
    public bool shouldCreateNewFile = true;

    [Tooltip("Location should be inside the Resources Folder")]
    public string fileLocation = "ComparedFileFolder";

    [SerializeField] private List<string> file1List, file2List;


    private void Start()
    {
        CompareFiles();
    }


    //[Button]
    private void CompareFiles()
    {
        if (file1 == null || file2 == null)
        {
            Debug.LogError("File Missing !");
        }

        FillLists();
        
        
        
    }


    void FillLists()
    {
        file1List = new List<string>();
        file2List = new List<string>();
        
        string[] data1 = file1.text.Split('\n');
        string[] data2 = file2.text.Split('\n');

        foreach (string word in data1)
        {
            if (!string.IsNullOrWhiteSpace(word))
            {
                file1List.Add(word);
            }
        }

        foreach (string word in data2)
        {
            if (!string.IsNullOrWhiteSpace(word))
            {
                file2List.Add(word);
            }
        }
    }
}