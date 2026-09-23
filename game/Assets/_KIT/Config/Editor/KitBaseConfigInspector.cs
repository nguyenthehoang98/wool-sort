using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using _KIT;
using ExcelExtension;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace _KIT.Editor
{
    [CustomEditor(typeof(KitBaseConfig), true)]
    public class KitBaseConfigInspector : UnityEditor.Editor
    {
        private const string PRE_PATH = "https://opensheet.elk.sh/";
        private static readonly string ROOT_FORMAT_PATH = "https://docs.google.com/spreadsheets/d/{0}/edit";
        private KitBaseConfig config;
        private ExcelAssetAttribute attribute;
        private int tab;

        private void OnEnable()
        {
            config = target as KitBaseConfig;
            attribute = config.GetType().GetCustomAttribute<ExcelAssetAttribute>();
            if(attribute == null)
                throw new NullReferenceException("Not exist ExcelAssetAttribute in class: " + target.GetType());
        }

        public override void OnInspectorGUI()
        {
            int widthButtonMini = 80;
            int widthLabel = 80;
            
            EditorGUILayout.BeginHorizontal();
            UnityEngine.GUI.enabled = tab != 0;
            if (GUILayout.Button("Excel")) tab = 0;
            UnityEngine.GUI.enabled = tab != 1;
            if (GUILayout.Button("Google sheet")) tab = 1;
            UnityEngine.GUI.enabled = true;
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginVertical(UnityEngine.GUI.skin.box);
            switch (tab)
            {
                case 0:
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    // todo:
                    EditorGUILayout.LabelField("Excel path", GUILayout.Width(widthLabel));
                    UnityEngine.GUI.enabled = false;
                    EditorGUILayout.TextField("", attribute.ExcelPath);
                    UnityEngine.GUI.enabled = true;
                    
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.EndHorizontal();
                    break;
                case 1:
                    EditorGUILayout.BeginHorizontal();
                    
                    EditorGUILayout.BeginVertical();
                    // todo:
                    if (GUILayout.Button("Import", GUILayout.Width(widthButtonMini))) Download();
                    if (GUILayout.Button("Open", GUILayout.Width(widthButtonMini)))
                        Application.OpenURL(string.Format(ROOT_FORMAT_PATH, config.fileUrl));
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.BeginVertical();
                    // todo:
                    EditorGUILayout.LabelField("URL", "", GUILayout.Width(widthLabel));
                    UnityEngine.GUI.enabled = config.enable;
                    config.fileUrl = EditorGUILayout.TextField("", config.fileUrl);
                    UnityEngine.GUI.enabled = true;
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.BeginVertical();
                    // todo:
                    EditorGUILayout.LabelField("Enable", "", GUILayout.Width(widthLabel));
                    config.enable = EditorGUILayout.Toggle("", config.enable);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.EndHorizontal();
                    break;
            }
            EditorGUILayout.EndVertical();
            
            config.OnGuiEnable();
            UnityEngine.GUI.enabled = false;
            base.OnInspectorGUI();
            UnityEngine.GUI.enabled = true;
        }

        private async void Download()
        {
            throw new NotImplementedException($"Downloading {config.GetType().Name}...");
            /*Debug.Log($"Downloading {config.GetType().Name}...");
            FieldInfo[] assetFields = config.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool found = false;
            foreach (FieldInfo info in assetFields)
            {
                bool flag = !info.FieldType.IsGenericType || info.FieldType.GetGenericTypeDefinition() != typeof(List<>);
                if (!flag)
                {
                    found = true;
                    UnityWebRequestAsyncOperation operation = UnityWebRequest.Get(PRE_PATH + config.fileUrl + "/" + info.Name).SendWebRequest();
                    while (!operation.isDone)
                    {
                        await Task.Delay(1000);
                    }

                    var request = operation.webRequest;
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        object value = JsonUtils.DeserializeObject(request.downloadHandler.text, info.FieldType);
                        info.SetValue(config, value);
                        Debug.Log("Download success");
                        EditorUtility.SetDirty(target);
                        AssetDatabase.SaveAssets();
                    }
                    else
                    {
                        Debug.LogError("Error download " + info.Name + ", detail:: " + request.error + "\n" + request.result);
                    }
                }
            }
            if (!found)
            {
                Debug.LogError("Không có kiểu phù hợp để tìm Spread-Sheet, Yêu cầu object phải sử dụng Atribute [SerializeField] và là có kiểu là List<T>");
            }*/
        }
    }
}