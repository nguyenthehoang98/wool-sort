using System;
using System.Collections.Generic;
using System.Reflection;
using _KIT;
using UnityEditor;
using UnityEngine;

namespace _Kit.Object.Editor
{
    public class ObjectToolEditorWindow : EditorWindow
    {
        private static bool[] foldouts = new bool[200];
        
        [MenuItem("Kit/Object/View pooling object")]
        private static void Open()
        {
            ObjectToolEditorWindow wnd = GetWindow<ObjectToolEditorWindow>();
            wnd.titleContent = new GUIContent("Pooling Object");
        }

        private void OnGUI()
        {
            DrawResource();
            DrawPool();
        }
        
        private void DrawPool()
        {
            Type typeOfResourceControl = typeof(KitPool);
            FieldInfo field = typeOfResourceControl.GetField("pool", BindingFlags.Static | BindingFlags.NonPublic);
            if (field == null)
            {
                EditorGUILayout.LabelField("Not found field 'pool' of " + typeOfResourceControl);
                return;
            }

            Dictionary<string, Stack<GameObject>> pool = (Dictionary<string, Stack<GameObject>>)field.GetValue(null);
            if (pool == null)
            {
                EditorGUILayout.LabelField("Error parse field 'pool' of " + typeOfResourceControl);
                return;
            }

            EditorGUILayout.LabelField("Count of item in pool " + pool.Count);
            int index = 0;
            foreach (var pair in pool)
            {
                EditorGUILayout.BeginHorizontal();

                foldouts[index] = EditorGUILayout.Foldout(foldouts[index], pair.Key.ToString());

                GUI.enabled = false;
                if (foldouts[index])
                {
                    EditorGUILayout.BeginVertical();
                    foreach (var item in pair.Value)
                    {
                        EditorGUILayout.ObjectField(item, typeof(UnityEngine.Object), true);                        
                    }
                    EditorGUILayout.EndVertical();
                }

                GUI.enabled = true;

                index++;
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawResource()
        {
            Type typeOfResourceControl = typeof(KitResource);
            FieldInfo field = typeOfResourceControl.GetField("pool", BindingFlags.Static | BindingFlags.NonPublic);
            if (field == null)
            {
                EditorGUILayout.LabelField("Not found field 'resource' of " + typeOfResourceControl);
                return;
            }

            Dictionary<string, UnityEngine.Object> pool = (Dictionary<string, UnityEngine.Object>)field.GetValue(null);
            if (pool == null)
            {
                EditorGUILayout.LabelField("Error parse field 'resource' of " + typeOfResourceControl);
                return;
            }

            EditorGUILayout.LabelField("Count of item in resource " + pool.Count);
            foreach (var pair in pool)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(pair.Key, GUILayout.Width(150));

                GUI.enabled = false;
                EditorGUILayout.ObjectField(pair.Value, typeof(UnityEngine.Object), true);
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
            }
        }
    }
}