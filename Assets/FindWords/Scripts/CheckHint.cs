using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace YugantLoyaLibrary.FindWords
{
    public class CheckHint : MonoBehaviour
    {
        [Range(3, 6)] public int formingWordLength = 5;
        public string availableLettersToMakeWord;
        public MainDictionary mainDict;

        [Button]
        public void FindWord()
        {
            if (formingWordLength > availableLettersToMakeWord.Length)
            {
                Debug.LogError("Available Letters are Less for making Word !!");
                return;
            }

            string finalStr = AnyWordExists();

            Debug.Log("Final Str : " + finalStr);
        }

        string AnyWordExists()
        {
            List<char> lettersUsedList = new List<char>();

            foreach (MainDictionary.MainDictionaryInfo mainDictionaryInfo in mainDict.dictInfoList)
            {
                if (mainDictionaryInfo.wordLength != formingWordLength) continue;


                foreach (char c in availableLettersToMakeWord)
                {
                    if (lettersUsedList.Contains(c)) continue;

                    lettersUsedList.Add(c);
                    int val = c - 97;
                    Debug.Log("C : " + c + "      C Val : " + val);
                    List<string> wordList = mainDictionaryInfo.wordsInfo[val].wordList;

                    foreach (string str in wordList)
                    {
                        if (IsWholeInside(str, availableLettersToMakeWord))
                        {
                            Debug.Log("Yoo ! Word Can be Formed !!");
                            return str;
                        }
                        else
                        {
                            //Debug.Log("Nope ! Word Can not be Formed !!");
                        }
                    }
                }
            }

            Debug.Log("None of Word Matched ");
            return "";
        }

        static bool IsWholeInside(string string1, string string2)
        {
            string1 = string1.ToLower().Trim();
            string2 = string2.ToLower().Trim();

            for (var i = 1; i < string1.Length; i++)
            {
                var c = string1[i];
                Debug.Log("C : " + c);
                int index = string2.IndexOf(c);
                Debug.Log("Index : " + index);
                if (index == -1)
                {
                    Debug.Log("Letter  " + c + " Missing !");
                    return false;
                }

                // Remove the character from string2 to ensure it is not repeated
                string2 = string2.Remove(index, 1);
            }

            return true;
        }
    }
}