using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnglishDictInfo", menuName = "English Dictionary")]
public class DictScriptableInfo : ScriptableObject
{
    public Trie dictionaryTrie = new Trie();
}