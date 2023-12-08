using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class RemoveEmptyLines : MonoBehaviour
{
    public TextAsset file;
    public List<string> lineList;

    [Button]
    void FilterFileAndRemoveEmptyLines()
    {
        lineList = new List<string>();
        
        string[] data = file.text.Split('\n');

        Debug.Log(data.Length);
        
        foreach (string line in data)
        {
            if (!string.IsNullOrEmpty(line) || !string.IsNullOrWhiteSpace(line))
            {
                lineList.Add(line);
            }
        }
        
        
    }
}