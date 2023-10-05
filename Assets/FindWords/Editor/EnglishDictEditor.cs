using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    [CustomEditor(typeof(EnglishDictWords))]
    public class EnglishDictEditor : Editor
    {
        private SerializedProperty _englishWordTextFile, _dictScriptable;

        private void OnEnable()
        {
            //English Dictionary File (Text File)
            _englishWordTextFile = serializedObject.FindProperty("wordTextFile");
            _dictScriptable = serializedObject.FindProperty("dictionaryData");
        }

        public override void OnInspectorGUI()
        {
            EnglishDictWords englishDict = (EnglishDictWords)target;

            englishDict.inputString = EditorGUILayout.TextField("Input String : ", englishDict.inputString);

            serializedObject.Update();

            EditorGUILayout.PropertyField(_englishWordTextFile);
            EditorGUILayout.PropertyField(_dictScriptable);

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
                    englishDict.Search(englishDict.inputString);
                }
            }

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}