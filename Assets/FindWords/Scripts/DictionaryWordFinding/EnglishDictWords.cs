using UnityEngine;
using UnityEditor;

namespace YugantLoyaLibrary.FindWords
{
    public class EnglishDictWords : MonoBehaviour
    {
        [SerializeField] private TextAsset wordTextFile;
        public string inputString;
        public DictScriptableInfo dictionaryData;

#if UNITY_EDITOR
        public void UpdateEnglishDict()
        {
            if (wordTextFile != null && dictionaryData != null)
            {
                //dictionaryData.dictionaryTrie = new Trie();
                string[] words = wordTextFile.text.Split('\n');
                foreach (string word in words)
                {
                    string cleanedWord = word.Trim().ToLower();
                    if (!string.IsNullOrEmpty(cleanedWord))
                    {
                        dictionaryData.dictionaryTrie.Insert(cleanedWord);
                    }
                }

                Debug.Log("Total Words in DictionaryData: " + dictionaryData.TotalWords);
            }
            else
            {
                Debug.LogError("Text file or DictionaryData not assigned.");
            }

            EditorUtility.SetDirty(dictionaryData);
            AssetDatabase.Refresh();
        }
#endif

        public bool Search(string word)
        {
#if UNITY_EDITOR
            UpdateEnglishDict();
#endif
            
            bool isAvailable = dictionaryData.dictionaryTrie.Search(word.ToLower());

            if (isAvailable)
            {
                Debug.Log($"Yoo !! {word} Word Found");
                return true;
            }
            else
            {
                Debug.Log($"Oops !! {word} Word Not Found");
                return false;
            }
        }
    }
}