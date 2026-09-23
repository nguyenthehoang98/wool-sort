using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using _KIT.Utils;
using NUnit.Compatibility;
using UnityEditor;
using UnityEngine;

namespace _KIT.Textures.Editor
{
    public class VectorGraphEditorWindow : EditorWindow
    {
        private UnityEngine.Object source;
        protected int scale = 100;

        protected GameObject parent;
        protected bool IsValid { get; private set; }

        /*[MenuItem("Tools/Vector convert")]
        public static void Open()
        {
            var window = GetWindow<VectorGraphEditorWindow>();
            window.titleContent = new GUIContent("Vector convert");
            window.position = new Rect(Screen.width / 2f - 50, Screen.height / 2f - 75, 200, 200);
            window.maxSize = new Vector2(400, 400);
            window.Show();
        }*/
        
        protected virtual void OnGUI()
        {
            source = EditorGUILayout.ObjectField("Svg source", source, typeof(UnityEngine.Object), false);
            scale = EditorGUILayout.IntSlider("scale (%)", scale, 1, 200);

            Type vector = TypeUtils.GetDepthType("Unity.VectorGraphics.SVGParser");
            if (vector == null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Import VectorGraphics");
                if (GUILayout.Button("Import"))
                {
                    Application.OpenURL("https://docs.unity3d.com/Packages/com.unity.vectorgraphics@2.0/manual/index.html");
                }
                EditorGUILayout.EndHorizontal();
            }            

            Type creator = TypeUtils.GetDepthType("Unity.VectorGraphics.SVGParser");
            if (creator == null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Import PathCreation");
                if (GUILayout.Button("Import"))
                {
                    Application.OpenURL("https://github.com/SebLague/Path-Creator");
                }
                EditorGUILayout.EndHorizontal();
            }

            IsValid = creator != null && vector != null;
            if (creator == null || vector == null) return;
            
            if (GUILayout.Button("Build"))
            {
                if (source == null)
                {
                    Debug.LogError("Không có chứa file source *.svg");
                    return;
                }

                string filepath = AssetDatabase.GetAssetPath(source);
                if (Path.GetExtension(filepath) != ".svg")
                {
                    Debug.LogError(Path.GetExtension(filepath));
                    Debug.LogError("File có phần mở rộng không hợp lệ, yêu cầu file svg");
                    return;
                }

                string content = File.ReadAllText(Path.GetFullPath(filepath));
                Type parserType = TypeUtils.GetDepthType("Unity.VectorGraphics.SVGParser");
                object instance = Activator.CreateInstance(parserType);
                MethodInfo info = parserType.GetMethod("ImportSVG", new Type[6] { typeof(TextReader) , typeof(float), typeof(float), typeof(int), typeof(int), typeof(bool)});
                if (info != null)
                {
                    object sceneInfo = info.Invoke(instance, new object[6]
                    {
                        new StringReader(content), 0, 1, 0, 0, false
                    });
                    if (sceneInfo != null)
                    {
                        Type sceneInfoType = sceneInfo.GetType();
                        PropertyInfo sceneProperty = sceneInfoType.GetProperty("Scene");
                        if (sceneProperty != null)
                        {
                            object scene = sceneProperty.GetValue(sceneInfo);
                            Type sceneType = scene.GetType();
                            
                            PropertyInfo rootProperty = sceneType.GetProperty("Root");
                            if (rootProperty != null)
                            {
                                object root = rootProperty.GetValue(scene);
                                Type rootType = root.GetType();
                                
                                PropertyInfo childrenProperty = rootType.GetProperty("Children");
                                if (childrenProperty != null)
                                {
                                    object children = childrenProperty.GetValue(root);
                                    if (children.GetType().IsGenericType)
                                    {
                                        IList list = children as IList;
                                        List<PathData>[] paths = new List<PathData>[list.Count];
                                        for (int i = 0; i < list.Count; i++)
                                        {
                                            paths[i] = new List<PathData>();
                                            ImportNode(ref paths[i], list[i]);
                                        }

                                        Vector3 center = Vector3.zero;
                                        int count = 0;
                                        for (int i1 = 0; i1 < paths.Length; i1++)
                                        {
                                            for (int i2 = 0; i2 < paths[i1].Count; i2++)
                                            {
                                                PathData data = paths[i1][i2];
                                                data.p0 *= (scale / 100f);
                                                data.p1 *= (scale / 100f);
                                                data.p2 *= (scale / 100f);
                                                data.p0.y *= -1;
                                                data.p1.y *= -1;
                                                data.p2.y *= -1;
                                                center += data.p0;
                                                count++;
                                            }
                                        }

                                        center /= count;
                                        for (int i1 = 0; i1 < paths.Length; i1++)
                                        {
                                            for (int i2 = 0; i2 < paths[i1].Count; i2++)
                                            {
                                                PathData data = paths[i1][i2];
                                                data.p0 -= center;
                                                data.p1 -= center;
                                                data.p2 -= center;
                                            }
                                        }

                                        parent = new GameObject(Path.GetFileName(filepath));
                                        for (int i = 0; i < paths.Length; i++)
                                        {
                                            Build(paths[i]);
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.LogError($"Not found property 'Children' in class '{rootType}'");
                                }
                            }
                            else
                            {
                                Debug.LogError($"Not found property 'Root' in class '{sceneType}'");
                            }
                        }
                        else
                        {
                            Debug.LogError($"Not found property 'Scene' in class '{sceneInfoType}'");
                        }
                    }
                    else
                    {
                        Debug.LogError($"Invoke method 'ImportSVG' in '{parserType}' return null result");
                    }
                }
                else
                {
                    Debug.LogError($"Not found method 'ImportSVG' in '{parserType}'");
                }
            }
        }
        
        private void ImportNode(ref List<PathData> paths, object node)
        {
            Type nodeType = node.GetType();
            PropertyInfo shapesProperty = nodeType.GetProperty("Shapes");
            if (shapesProperty != null)
            {
                object shapes = shapesProperty.GetValue(node);
                if (shapes != null && shapes.GetType().IsGenericType)
                {
                    IList list = shapes as IList;
                    for (int i = 0; i < list.Count; i++)
                    {
                        ImportShape(ref paths, list[i]); 
                    }
                }
            }

            PropertyInfo childrenProperty = nodeType.GetProperty("Children");
            if (childrenProperty != null)
            {
                object children = childrenProperty.GetValue(node);
                if (children != null && children.GetType().IsGenericType)
                {
                    IList list = children as IList;
                    for (int i = 0; i < list.Count; i++)
                    {
                        if(list[i] != null) ImportNode(ref paths, list[i]); 
                    }
                }
            }
        }

        private void ImportShape(ref List<PathData> paths, object shape)
        {
            Type shapeType = shape.GetType();
            PropertyInfo contoursProperty = shapeType.GetProperty("Contours");
            if (contoursProperty != null)
            {
                object contours = contoursProperty.GetValue(shape);
                if (contours != null && contours.GetType().IsArray)
                {
                    Array contoursArray = contours as Array;
                    for (int i1 = 0; i1 < contoursArray.Length; i1++)
                    {
                        object contour = contoursArray.GetValue(i1);
                        PropertyInfo segmentsProperty = contour.GetType().GetProperty("Segments");
                        object segments = segmentsProperty.GetValue(contour);
                        if (segments != null && segments.GetType().IsArray)
                        {
                            Array segmentsArray = segments as Array;
                            for (int i2 = 0; i2 < segmentsArray.Length; i2++)
                            {
                                object segment = segmentsArray.GetValue(i2);
                                Vector3 p0 = (Vector2)segment.GetType().GetField("P0").GetValue(segment);
                                Vector3 p1 = (Vector2)segment.GetType().GetField("P1").GetValue(segment);
                                Vector3 p2 = (Vector2)segment.GetType().GetField("P2").GetValue(segment);
                                paths.Add(new PathData
                                {
                                    p0 = p0, p1 = p1, p2 = p2
                                });
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("Break in here");
                }
            }
        }
        
        private void Build(List<PathData> paths)
        {
            Type creatorType = TypeUtils.GetDepthType("PathCreation.PathCreator");
            GameObject path = new GameObject("Path");
            path.transform.parent = parent.transform;
            path.transform.localPosition = Vector3.zero;

            object component = path.AddComponent(creatorType);
            component.GetType()
                .GetMethod("InitializeEditorData", new Type[1] { typeof(bool) })
                .Invoke(component, new object[1] { false });

            FieldInfo editorDataField = component.GetType().GetField("editorData", BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic);
            object editorData = editorDataField.GetValue(component);
            editorData.GetType()
                .GetMethod("ResetBezierPath", new Type[2] { typeof(Vector3), typeof(bool) })
                .Invoke(editorData, new object[2] { Vector3.zero, true });

            PropertyInfo bezierPathProperty = component.GetType().GetProperty("bezierPath");
            object bezierPath = bezierPathProperty.GetValue(component);
            FieldInfo pointsField = bezierPath.GetType().GetField("points", BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic);
            IList points = pointsField.GetValue(bezierPath) as IList;
            
            bezierPath.GetType().GetProperty("IsClosed").SetValue(bezierPath, true);
            
            for (int i = 0; i < paths.Count; i++)
            {
                int length = i * 3 + 3;
                while (points.Count < length)
                {
                    points.Add(Vector3.zero);
                }

                PathData data = paths[i];
                points[i * 3] = data.p0;
                points[i * 3 + 1] = data.p1;
                points[i * 3 + 2] = data.p2;

                bezierPath.GetType().GetMethod("NotifyPathModified").Invoke(bezierPath, new object[0]);
            }
            
            bezierPath.GetType().GetProperty("IsClosed").SetValue(bezierPath, false);
        }

        private class PathData
        {
            public Vector3 p0;
            public Vector3 p1;
            public Vector3 p2;
        }
    }
}