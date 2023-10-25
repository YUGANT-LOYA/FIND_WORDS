using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { set; get; }
    public SaveState state;
    public string saveKey = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        Load();
        Debug.Log("Save Manager (Word Left List) Count : " + SaveManager.Instance.state.wordLeftList.Count);
        Debug.Log("Save Manager (Hint List) Count : " + SaveManager.Instance.state.hintList.Count);
        Debug.Log("Save Manager (Grid On Screen List) Count : " +
                  SaveManager.Instance.state.gridOnScreenList.Count);
        Debug.Log("Save Manager (Grid Data List) Count : " + SaveManager.Instance.state.gridDataList.Count);
    }
    
    public void Save()
    {
        PlayerPrefs.SetString(saveKey, SaveHelper.Serialize<SaveState>(state));
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            state = SaveHelper.Deserialize<SaveState>(PlayerPrefs.GetString(saveKey));
        }
        else
        {
            state = new SaveState();
        }
    }

    public void UpdateState()
    {
        Save();
        Load();
    }

    public void Reset()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            PlayerPrefs.DeleteKey(saveKey);
        }
    }
}