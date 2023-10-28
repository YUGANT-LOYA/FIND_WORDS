using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    [CustomEditor(typeof(EnglishDictWords))]
    public class EnglishDictEditor : Editor
    {
        private SerializedProperty _fullDictWordTextFile,
            _filteredDictWordTextFile,
            _fullDictScriptable,
            _filteredDictScriptable;

        private void OnEnable()
        {
            //English Dictionary File (Text File)
            _fullDictWordTextFile = serializedObject.FindProperty("fullWordTextFile");
            _filteredDictWordTextFile = serializedObject.FindProperty("filteredWordTextFile");
            _fullDictScriptable = serializedObject.FindProperty("fullDictData");
            _filteredDictScriptable = serializedObject.FindProperty("filteredDictData");
        }

        public override void OnInspectorGUI()
        {
            EnglishDictWords englishDict = (EnglishDictWords)target;

            englishDict.wordInFullDict =
                EditorGUILayout.TextField("Find Word In Full Dict : ", englishDict.wordInFullDict);
            englishDict.wordInFilteredDict =
                EditorGUILayout.TextField("Find Word In Filtered Dict : ", englishDict.wordInFilteredDict);
            englishDict.compareWord =
                EditorGUILayout.TextField("Compare Word : ", englishDict.compareWord);

            serializedObject.Update();

            EditorGUILayout.PropertyField(_fullDictWordTextFile);
            EditorGUILayout.PropertyField(_filteredDictWordTextFile);
            EditorGUILayout.PropertyField(_fullDictScriptable);
            EditorGUILayout.PropertyField(_filteredDictScriptable);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Update Full English Dict"))
            {
                Debug.Log("Updating Full English Dictionary File !");
                englishDict.UpdateFullEnglishDict();
            }


            if (GUILayout.Button("Update Filtered English Dict"))
            {
                Debug.Log("Updating Filtered English Dictionary File !");
                englishDict.UpdateFilteredEnglishDict();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Compare Word in Both Dict"))
            {
                Debug.Log("Compare Word in Both Dict !");
                englishDict.CompareWordsInEditor();
            }


            if (!string.IsNullOrWhiteSpace(englishDict.wordInFullDict))
            {
                if (GUILayout.Button("Search Word of Input Text in Dict"))
                {
                    Debug.Log("Searching Word in English Dictionary !");
                    englishDict.SearchInFullDict(englishDict.wordInFullDict);
                }
            }

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}