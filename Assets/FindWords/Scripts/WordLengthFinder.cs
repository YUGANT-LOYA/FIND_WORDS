using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using NaughtyAttributes;

public class WordLengthFinder : MonoBehaviour
{
    public int wordLength = 3;
    public char letter;
    public TextAsset oxfordDictTextFile, wordLengthTextFile;

    [Button]
    public void FindWordLength()
    {
        string data = oxfordDictTextFile.text.Trim();
        string[] lines = data.Split('\n');

        StringBuilder csvContent = new StringBuilder();

        for (int i = 0; i < lines.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]) && lines[i].Length == wordLength)
            {
                csvContent.AppendLine(lines[i]);
            }
        }

        //Asset Folder Path
        string assetsFolderPath = Application.dataPath;

        // Specify the relative path within the "Assets" folder where you want to save the CSV file
        string relativeFilePath = $"FindWords/CSV/WordLengthDictionary/WordLength_{wordLength}.csv";

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

        // for (int i = 65; i <= 91; i++)
        // {
        //     char letter = (char)i;
        StringBuilder csvContent = new StringBuilder();
        for (int j = 0; j < lines.Length; j++)
        {
            if (!string.IsNullOrWhiteSpace(lines[j]) && lines[j][0].ToString().ToUpper() == letter.ToString().ToUpper())
            {
                csvContent.AppendLine(lines[j]);
            }
        }

        string assetsFolderPath = Application.dataPath;

        // Specify the relative path within the "Assets" folder where you want to save the CSV file
        string relativeFilePath =
            $"FindWords/CSV/WordLengthDictionary/{wordLength}_WordLengthFolder/{letter.ToString().ToUpper()}_Words.csv";

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
    //}
}