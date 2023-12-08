using System.IO;
using System.Text;
using UnityEngine;
using NaughtyAttributes;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YugantLoyaLibrary.FindWords
{
    public class WordLengthFinder : MonoBehaviour
    {
        public bool rewriteFile;
        public int wordLength = 3, minLength = 3, maxLength = 8;
        public TextAsset oxfordDictTextFile, filteredDictTextFile;

        [Button]
        public void FindWordLengthForFullDictData()
        {
            string data = oxfordDictTextFile.text.Trim();
            string[] lines = data.Split('\n');

            StringBuilder content = new StringBuilder();
            Debug.Log("Data Length : " + lines.Length);

            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]) && lines[i].Trim().Length == wordLength)
                {
                    Debug.Log("Str : " + lines[i]);
                    content.AppendLine(lines[i]);
                }
            }

            string assetsFolderPath = Application.dataPath;

// Specify the relative path within the "Assets" folder where you want to save the CSV file
            string relativeFilePath = "FindWords/Resources/WordLengthFindFolder/FullLengthWordData";
            string fileName = $"WordLength_{wordLength}.txt";

// Combine the paths to get the full path of the CSV file
            string folderPath = Path.Combine(assetsFolderPath, relativeFilePath);
            string filePath = Path.Combine(folderPath, fileName);

// Check if the directory exists, and if not, create it
            if (!Directory.Exists(folderPath))
            {
                Debug.Log("Folder Creating !");
                Directory.CreateDirectory(folderPath);
            }

// Save the Txt file
            if (rewriteFile || !File.Exists(filePath))
            {
                Debug.Log("File Creating !!");
                File.WriteAllText(filePath, content.ToString());
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
        public void FindWordLengthForFilteredDictData()
        {
            string data = filteredDictTextFile.text.Trim();
            string[] lines = data.Split('\n');

            StringBuilder content = new StringBuilder();
            Debug.Log("Data Length : " + lines.Length);

            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]) && lines[i].Trim().Length == wordLength)
                {
                    Debug.Log("Str : " + lines[i]);
                    content.AppendLine(lines[i]);
                }
            }

            string assetsFolderPath = Application.dataPath;

// Specify the relative path within the "Assets" folder where you want to save the CSV file
            string relativeFilePath = "FindWords/Resources/WordLengthFindFolder/FilteredLengthWordData";
            string fileName = $"WordLength_{wordLength}.txt";

// Combine the paths to get the full path of the CSV file
            string folderPath = Path.Combine(assetsFolderPath, relativeFilePath);
            string filePath = Path.Combine(folderPath, fileName);

// Check if the directory exists, and if not, create it
            if (!Directory.Exists(folderPath))
            {
                Debug.Log("Folder Creating !");
                Directory.CreateDirectory(folderPath);
            }

// Save the Txt file
            if (!File.Exists(filePath) || rewriteFile)
            {
                Debug.Log("File Creating !!");
                File.WriteAllText(filePath, content.ToString());
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

            StringBuilder content = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]) && lines[i].Length >= minLength &&
                    lines[i].Length <= maxLength)
                {
                    content.AppendLine(lines[i]);
                }
            }

            //Asset Folder Path
            string assetsFolderPath = Application.dataPath;

            // Specify the relative path within the "Assets" folder where you want to save the CSV file
            string relativeFilePath = $"FindWords/Resources/WordDictData/NewOxfordDict.txt";

            // Combine the paths to get the full path of the CSV file
            string filePath = Path.Combine(assetsFolderPath, relativeFilePath);

            // Save the Txt file

            if (!string.IsNullOrEmpty(filePath) && !File.Exists(filePath))
            {
                Debug.Log("File Creating !!");
                File.WriteAllText(filePath, content.ToString());
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
            Debug.Log("Txt file saved to: " + filePath);
        }
    }
}