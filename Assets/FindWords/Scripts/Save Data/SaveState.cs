using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;

[Serializable]
public class SaveState
{
    public List<char> gridDataList = new List<char>();
    public List<bool> gridOnScreenList = new List<bool>();
    public List<string> hintList = new List<string>();
}