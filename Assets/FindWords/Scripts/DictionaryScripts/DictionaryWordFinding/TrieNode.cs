using System.Collections.Generic;

namespace YugantLoyaLibrary.FindWords
{
    public class TrieNode
    {
        public Dictionary<char, TrieNode> Children { get; private set; }
        public bool IsEndOfWord { get; set; }

        public TrieNode()
        {
            Children = new Dictionary<char, TrieNode>();
            IsEndOfWord = false;
        }
    }

    public class Trie
    {
        private TrieNode root;

        public Trie()
        {
            root = new TrieNode();
        }

        // Get the root node of the Trie
        public TrieNode GetRootNode()
        {
            return root;
        }

        public void Insert(string word)
        {
            TrieNode current = root;
            foreach (char c in word)
            {
                if (!current.Children.ContainsKey(c))
                {
                    current.Children[c] = new TrieNode();
                }

                current = current.Children[c];
            }

            current.IsEndOfWord = true;
        }

        public bool Search(string word)
        {
            TrieNode current = root;
            foreach (char c in word)
            {
                if (!current.Children.ContainsKey(c))
                {
                    return false;
                }

                current = current.Children[c];
            }

            return current.IsEndOfWord;
        }

        public List<string> GetWords()
        {
            List<string> words = new List<string>();
            CollectWords(root, "", words);
            return words;
        }

        private void CollectWords(TrieNode node, string currentWord, List<string> words)
        {
            if (node.IsEndOfWord)
            {
                words.Add(currentWord);
            }

            foreach (var kvp in node.Children)
            {
                CollectWords(kvp.Value, currentWord + kvp.Key, words);
            }
        }

        // Method to count total children in the Trie
        public int CountTotalChildren()
        {
            return CountChildren(root);
        }

        private int CountChildren(TrieNode node)
        {
            int count = 0;

            foreach (var child in node.Children)
            {
                count += CountChildren(child.Value);
            }

            return count + (node.IsEndOfWord ? 1 : 0);
        }
    }
}