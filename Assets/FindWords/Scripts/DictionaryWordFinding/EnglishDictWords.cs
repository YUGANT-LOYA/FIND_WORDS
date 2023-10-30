using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YugantLoyaLibrary.FindWords
{
    public class EnglishDictWords : MonoBehaviour
    {
        [SerializeField] private TextAsset fullWordTextFile, filteredWordTextFile;
        public string compareWord, wordInFullDict, wordInFilteredDict;
        public DictScriptableInfo fullDictData;
        public DictScriptableInfo filteredDictData;

        public void UpdateFullEnglishDict()
        {
            if (fullWordTextFile != null && fullDictData != null)
            {
                //dictionaryData.dictionaryTrie = new Trie();
                string[] words = fullWordTextFile.text.Split('\n');
                foreach (string word in words)
                {
                    string cleanedWord = word.Trim().ToLower();
                    if (!string.IsNullOrEmpty(cleanedWord))
                    {
                        fullDictData.dictionaryTrie.Insert(cleanedWord);
                    }
                }

                Debug.Log("Total Words in DictionaryData: " + fullDictData.TotalWords);
            }
            else
            {
                Debug.LogError("Text file or DictionaryData not assigned.");
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(fullDictData);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
#endif
        }


        public void UpdateFilteredEnglishDict()
        {
            if (filteredWordTextFile != null && filteredDictData != null)
            {
                //dictionaryData.dictionaryTrie = new Trie();
                string[] words = filteredWordTextFile.text.Split('\n');
                foreach (string word in words)
                {
                    string cleanedWord = word.Trim().ToLower();
                    if (!string.IsNullOrEmpty(cleanedWord))
                    {
                        filteredDictData.dictionaryTrie.Insert(cleanedWord);
                    }
                }

                Debug.Log("Total Words in DictionaryData: " + filteredDictData.TotalWords);
            }
            else
            {
                Debug.LogError("Text file or DictionaryData not assigned.");
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(filteredDictData);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
#endif
        }

        public bool SearchInFullDict(string word)
        {
            bool isAvailable = fullDictData.dictionaryTrie.Search(word.ToLower());
            Debug.Log("Is Avail : " + isAvailable);
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

        public bool SearchInFilteredFullDict(string word)
        {
            bool isAvailable = filteredDictData.dictionaryTrie.Search(word.ToLower());

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

        public void CompareWordsInEditor()
        {
            bool isWordInFullDict = SearchInFullDict(compareWord);

            if (isWordInFullDict)
            {
                bool isWordInFilteredDict = SearchInFilteredFullDict(compareWord);
                Debug.Log(!isWordInFilteredDict ? "New Word Found Bro!!" : "Word Exist in Both !!");
            }
            else
            {
                Debug.Log($"{compareWord} Word Not Found !!");
            }
        }

        public bool CompareWordsInGame(string word, out bool doExist)
        {
            bool isWordInFullDict = SearchInFullDict(word);

            if (isWordInFullDict)
            {
                doExist = true;
                bool isWordInFilteredDict = SearchInFilteredFullDict(word);

                if (!isWordInFilteredDict)
                {
                    Debug.Log("New Word Found Bro!!");
                    return true;
                }
                else
                {
                    Debug.Log("Word is Not Common!");
                }
            }
            else
            {
                Debug.Log("Word Doesn't Exist!");
                doExist = false;
            }

            return false;
        }
    }
}