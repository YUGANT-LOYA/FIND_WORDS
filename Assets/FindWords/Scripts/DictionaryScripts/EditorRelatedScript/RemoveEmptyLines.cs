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
        lineList.Clear();
        
        string[] data = file.text.Split('\n');

        foreach (string line in data)
        {
            if (!string.IsNullOrEmpty(line) || !string.IsNullOrWhiteSpace(line))
            {
                lineList.Add(line);
            }
        }
        
        
    }
}