using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnglishDictWords))]
public class EnglishDictEditor : Editor
{
    private SerializedProperty _englishWordTextFile;

    private void OnEnable()
    {
        //English Dictionary File (Text File)
        _englishWordTextFile = serializedObject.FindProperty("wordTextFile");
    }

    public override void OnInspectorGUI()
    {
        EnglishDictWords englishDict = (EnglishDictWords)target;
        
        englishDict.inputString = EditorGUILayout.TextField("Input String : ",englishDict.inputString);
        EditorGUILayout.PropertyField(_englishWordTextFile);
        serializedObject.Update();


        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Update English Dict"))
        {
            Debug.Log("Updating English Dictionary File !");
            englishDict.UpdateEnglishDict();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5f);
        EditorGUILayout.BeginHorizontal();
        
        if (!string.IsNullOrWhiteSpace(englishDict.inputString))
        {
            if (GUILayout.Button("Search Word of Input Text in Dict"))
            {
                Debug.Log("Searching Word in English Dictionary !");
            }
        }

        EditorGUILayout.EndHorizontal(); 
        
        serializedObject.ApplyModifiedProperties();
    }
}