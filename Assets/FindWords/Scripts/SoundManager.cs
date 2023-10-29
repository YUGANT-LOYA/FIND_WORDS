using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public AudioSource audioSource;

    [Serializable]
    public struct SoundStruct
    {
        public SoundType soundType;
        public AudioClip audioClip;
    }

    public List<SoundStruct> soundInfoList = new List<SoundStruct>();

    public enum SoundType
    {
        None,
        ClickSound,
        WrongSound,
        CorrectSound
    }


    private void Awake()
    {
        CreateSingleton();
    }

    private void CreateSingleton()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }
    }


    public void PlaySound(SoundType soundType)
    {
        foreach (SoundStruct soundStruct in soundInfoList)
        {
            if (soundStruct.soundType == soundType)
            {
                audioSource.clip = soundStruct.audioClip;
                audioSource.Play();
            }
        }
    }
}