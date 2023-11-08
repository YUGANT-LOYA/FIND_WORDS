using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PluginsManager : MonoBehaviour
{
    public static PluginsManager instance;
    
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            DestroyImmediate(gameObject);
        }
    }
}