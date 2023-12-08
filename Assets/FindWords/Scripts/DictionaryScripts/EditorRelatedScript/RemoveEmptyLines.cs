using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;

public class RemoveEmptyLines : MonoBehaviour
{
    public bool reWriteFile;
    public string fileLocation = "FindWords";
    public TextAsset txtFile;
    public int fileLength = 0;
    public List<string> lineList;

    [Button]
    void FilterFileAndRemoveEmptyLines()
    {
        lineList = new List<string>();
        fileLength = 0;

        string[] data = txtFile.text.Split('\n');
        StringBuilder txtFileData = new StringBuilder();

        foreach (string line in data)
        {
            string str = line.Trim();
            if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str)) continue;
            Debug.Log("Str : " + str + "   Length : " + str.Length);
            fileLength++;
            lineList.Add(str);
            txtFileData.AppendLine(str);
        }

        //For Removing Last Empty Line Spacing
        
        txtFileData.Remove(txtFileData.Length - 1, 1);
        
        string filepath = "";

        if (reWriteFile)
        {
            filepath = Application.dataPath + $"/{fileLocation}/{txtFile.name}.txt";
        }
        else
        {
            filepath = Application.dataPath + $"/{fileLocation}/{txtFile.name}_Duplicate.txt";
        }

        Debug.Log("File Path : " + filepath);

        if (reWriteFile || !File.Exists(filepath))
        {
            File.WriteAllText(filepath, string.Empty);
            File.WriteAllText(filepath, txtFileData.ToString());
            
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

    [Button]
    void Clear()
    {
        lineList = new List<string>();
        fileLength = 0;
    }
}