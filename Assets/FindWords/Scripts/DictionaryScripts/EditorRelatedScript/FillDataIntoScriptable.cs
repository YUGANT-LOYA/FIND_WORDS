using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using NaughtyAttributes;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YugantLoyaLibrary.FindWords
{
    public class FillDataIntoScriptable : MonoBehaviour
    {
        public bool reWriteFile;
        public int wordLength = 3;
        public TextAsset wordLengthTextFile;
        public MainDictionary fullDictData, filteredDictData;
        public PickWordDataInfo pickWordDataInfo;

        [Tooltip("Minimum and Maximum Folder or word Length of which you have to add all data !")]
        public Vector2Int minMaxScriptableWordLength;


        //This converts the full dict text data into letters
        [Button]
        void FullWordDataToLetters()
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
                    $"FindWords/Resources/WordFullDictData/{wordLength}_WordLengthFolder/{currletter.ToString()}_Words.txt";

                // Combine the paths to get the full path of the CSV file
                string filePath = Path.Combine(assetsFolderPath, relativeFilePath);

                // Save the CSV file

                if (reWriteFile)
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

        //This converts the full dict text data into letters
        [Button]
        void FilteredWordDataToLetters()
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
                    $"FindWords/Resources/WordFilteredDictData/{wordLength}_WordLengthFolder/{currletter.ToString()}_Words.txt";

                // Combine the paths to get the full path of the CSV file
                string filePath = Path.Combine(assetsFolderPath, relativeFilePath);

                // Save the CSV file

                if (reWriteFile)
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
        private void FillFullDictionaryInfo()
        {
            MainDictionary dict = fullDictData;

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
                        Resources.Load<TextAsset>($"WordFullDictData/{i}_WordLengthFolder/{tempChar}_Words");

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
            fullDictData = dict;

#if UNITY_EDITOR
        EditorUtility.SetDirty(fullDictData);
        AssetDatabase.Refresh();
#endif
        }

        [Button]
        private void FillFilteredDictionaryInfo()
        {
            MainDictionary dict = filteredDictData;

            List<MainDictionary.MainDictionaryInfo> dictInfoList = new List<MainDictionary.MainDictionaryInfo>();

            for (int i = minMaxScriptableWordLength.x;
                 i <= minMaxScriptableWordLength.y;
                 i++)
            {
                MainDictionary.MainDictionaryInfo filteredDictInfo = new MainDictionary.MainDictionaryInfo();

                List<MainDictionary.WordLengthDetailedInfo> dictWordInfoList =
                    new List<MainDictionary.WordLengthDetailedInfo>();

                for (int k = 97; k < 123; k++)
                {
                    MainDictionary.WordLengthDetailedInfo wordInfo = new MainDictionary.WordLengthDetailedInfo();

                    char tempChar = (char)k;
                    //Debug.Log("File Creating !!");

                    TextAsset textFile =
                        Resources.Load<TextAsset>($"WordFilteredDictData/{i}_WordLengthFolder/{tempChar}_Words");

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

                filteredDictInfo.wordLength = i;
                filteredDictInfo.wordsInfo = dictWordInfoList;

                //Debug.Log("mainDictionaryInfo.wordLength : " + mainDictionaryInfo.wordLength);
                dictInfoList.Add(filteredDictInfo);
            }


            dict.dictInfoList = dictInfoList;
            filteredDictData = dict;

#if UNITY_EDITOR
        EditorUtility.SetDirty(filteredDictData);
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
                $"FindWords/CSV/ShuffledWordFolder/{wordLength}_Shuffled_Words.txt";

            // Combine the paths to get the full path of the CSV file
            string filePath = Path.Combine(assetsFolderPath, relativeFilePath);

            // Save the CSV file

            if (reWriteFile)
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

        [Button]
        void RemoveDuplicateWords()
        {
            string[] lines = wordLengthTextFile.text.Trim().Split('\n');
            List<string> totalWordList = new List<string>();
            List<string> wordAddedToCsv = new List<string>();

            string assetsFolderPath = Application.dataPath;

            // Specify the relative path within the "Assets" folder where you want to save the CSV file
            string relativeFilePath =
                $"FindWords/Resources/PredefinedWords/{wordLength}_Duplicate_Letter.txt";

            // Combine the paths to get the full path of the CSV file
            string filePath = Path.Combine(assetsFolderPath, relativeFilePath);

            foreach (string str in lines)
            {
                totalWordList.Add(str.Trim().ToLower());
            }

            StringBuilder csvContent = new StringBuilder();

            foreach (var str in totalWordList)
            {
                if (!string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str) && !wordAddedToCsv.Contains(str))
                {
                    wordAddedToCsv.Add(str);
                    csvContent.AppendLine(str.Trim());
                }
            }

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.Write(csvContent.ToString());
            }

            Debug.Log("Duplicate Removed Successfully !!");

#if UNITY_EDITOR
        EditorUtility.SetDirty(wordLengthTextFile);
        AssetDatabase.Refresh();
#endif
        }
    }
}