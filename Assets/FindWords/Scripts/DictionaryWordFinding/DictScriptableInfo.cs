using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    [CreateAssetMenu(fileName = "EnglishDictInfo", menuName = "English Dictionary")]
    public class DictScriptableInfo : ScriptableObject
    {
        public Trie dictionaryTrie = new Trie();
        public int totalWords;

        // Get the total words count from the Trie
        public int TotalWords
        {
            get
            {
                totalWords = dictionaryTrie.CountTotalChildren();
                return totalWords;
            }
        }

        public bool ContainsData()
        {
            return dictionaryTrie != null && dictionaryTrie.GetRootNode() != null;
        }
    }
}