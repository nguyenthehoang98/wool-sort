using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace _KIT
{
    public static class KitResource
    {
        private static readonly Dictionary<string, UnityEngine.Object> pool = new Dictionary<string, UnityEngine.Object>();

        public static T LoadInstance<T>(string path) where T : UnityEngine.Object
        {
            T obj = null;
            if (pool.TryGetValue(path, out UnityEngine.Object o))
            {
                if (o == null)
                {
                    Debug.LogError($"Error load instance Object type {typeof(T)} at: Resources/{path}");
                    return null;
                }
                if (typeof(T) == o.GetType())
                {
                    return o as T;
                }
                else if (typeof(T).IsSubclassOf(typeof(Component)))
                {
                    if (o is GameObject gameObject)
                    {
                        obj = gameObject.GetComponent<T>();
                        if (obj == null)
                        {
                            throw new NullReferenceException(
                                $"Not found component type {typeof(T)} at: Resources/{path}");
                        }

                        return obj;
                    }
                    else
                    {
                        throw new NullReferenceException(
                            $"Object not define is GameObject type {o.GetType()} at: Resources/{path}");
                    }
                }
            }

            obj = Resources.Load<T>(path);
            
            if (obj == null)
            {
                Debug.LogError($"Error load instance Object type {typeof(T)} at: Resources/{path}");
            }

            pool[path] = obj;

            return obj;
        }

        public static async Task<T> LoadAsync<T>(string path) where T : UnityEngine.Object
        {
            T obj = null;
            if (pool.TryGetValue(path, out UnityEngine.Object o))
            {
                if (typeof(T) == o.GetType())
                {
                    return o as T;
                }
                else if (typeof(T).IsSubclassOf(typeof(Component)))
                {
                    if (o is GameObject gameObject)
                    {
                        obj = gameObject.GetComponent<T>();
                        if (obj == null)
                        {
                            throw new NullReferenceException(
                                $"Not found component type {typeof(T)} at: Resources/{path}");
                        }

                        return obj;
                    }
                    else
                    {
                        throw new NullReferenceException(
                            $"Object not define is GameObject type {o.GetType()} at: Resources/{path}");
                    }
                }
            }

            ResourceRequest request = Resources.LoadAsync<T>(path);
            while (!request.isDone)
            {
                await Task.Yield();
            }

            if (request.asset == null)
                throw new NullReferenceException($"Error load async Object type {typeof(T)} at: Resources/{path}");

            if (typeof(T).IsSubclassOf(typeof(Component)))
            {
                GameObject gameObject = request.asset as GameObject;
                if (gameObject != null)
                {
                    obj = gameObject.GetComponent<T>();
                    if (obj == null)
                    {
                        throw new NullReferenceException($"Not found component type {typeof(T)} at: Resources/{path}");
                    }
                }
                else
                {
                    throw new NullReferenceException($"Object not define is GameObject at: Resources/{path}");
                }
            }
            else
            {
                obj = request.asset as T;
                if (obj == null)
                    throw new NullReferenceException($"Error parse Object to {typeof(T)} at: Resources/{path}");
            }

            pool[path] = obj;

            return obj;
        }

        public static void Unload(string path)
        {
            if (pool.TryGetValue(path, out var prefab))
            {
                pool.Remove(path);
            }
        }
    }
}