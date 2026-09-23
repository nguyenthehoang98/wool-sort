using System;
using System.Collections.Generic;
using _Game.Scripts.Utils;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace _KIT
{
    public class KitPool : MonoBehaviour
    {
        // track path của level vừa load
        private static string _lastLevelPath;
        
        public bool releaseOnLoadScene = false;
        
        private static KitPool instance;

        public static KitPool Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject("KitPool").AddComponent<KitPool>();
                    DontDestroyOnLoad(instance.gameObject);
                }

                return instance;
            }
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (releaseOnLoadScene)
            {
                foreach (var pair in unityPool)
                    pair.Value.Dispose();
               
                List<GameObject> gameObjects = new List<GameObject>();
                for (int i = 0; i < transform.childCount; i++)
                {
                    gameObjects.Add(transform.GetChild(i).gameObject);
                }

                for (int i = 0; i < gameObjects.Count; i++)
                {
                    Object.Destroy(gameObjects[i]);
                }

                unityPool.Clear();
            }
        }

        class Pool : IDisposable
        {
            private readonly GameObject instance;
            private readonly IObjectPool<GameObject> pool;
            
            public Pool(GameObject instance, int numberOfPreload)
            {
                this.instance = instance;
#if UNITY_EDITOR
                pool = new ObjectPool<GameObject>(OnCreate, OnReuse, OnRelease, OnDestroy);
#else
                pool = new ObjectPool<GameObject>(OnCreate, OnReuse, OnRelease, OnDestroy, false);
#endif
                for (int i = 0; i < numberOfPreload; i++)
                {
                    GameObject o = OnCreate();
                    OnRelease(o);
                }
            }

            public void Release(GameObject target) => pool.Release(target);

            public void SpawnInPool()
            {
                GameObject o = OnCreate();
                pool.Release(o);
            }
            
            public GameObject Reuse() => pool.Get();

            private void OnDestroy(GameObject obj) => UnityEngine.Object.Destroy(obj);

            private void OnRelease(GameObject obj)
            {
                KitPool monobehaviour = KitPool.Instance;
                obj.transform.parent = monobehaviour.transform;
                obj.SetActive(false);
            }

            private void OnReuse(GameObject obj) => obj.SetActive(true);

            private GameObject OnCreate()
            {
                GameObject obj = UnityEngine.Object.Instantiate(instance);
                obj.name = instance.name;            
                return obj;
            }

            public void Dispose()
            {
                pool.Clear();
            }
        }

        private static readonly Dictionary<string, Pool> unityPool = new Dictionary<string, Pool>();

        public static void RegisterPool(GameObject ins, int numberOfPreload)
        {
            if (ins == null)
            {
#if UNITY_EDITOR
                Debug.LogError("register pool ins is null");
#endif
                return;
            }
            
            string code = ins.name;
            if (!unityPool.ContainsKey(code))
            {
                unityPool.Add(code, new Pool(ins, numberOfPreload));
            }
        }

        public static void SpawnToPool(GameObject ins)
        {
            string code = ins.name;
            if (unityPool.TryGetValue(code, out Pool pool))
            {
                pool.SpawnInPool();
            }
        }
        
        public static void UnRegisterPool(GameObject ins)
        {
            string code = ins.name;
            if (unityPool.TryGetValue(code, out Pool pool))
            {
                pool.Dispose();
                unityPool.Remove(code);
            }
        }

        public new static T Instantiate<T>(T ins) where T : Component
        {
            GameObject o = Instantiate(ins.gameObject);
            return o.GetComponent<T>();
        }

        public new static T Instantiate<T>(T ins, Transform parent) where T : Component
        {
            GameObject o = Instantiate(ins.gameObject);
            o.transform.SetParent(parent);
            return o.GetComponent<T>();
        }

        public static T Instantiate<T>(T ins, bool active) where T : Component
        {
            GameObject o = Instantiate(ins.gameObject, active);
            return o.GetComponent<T>();
        }
        
        public static GameObject Instantiate(GameObject ins, bool active)
        {
            string code = ins.name;
            if (unityPool.TryGetValue(code, out Pool pool))
            {
                GameObject o = pool.Reuse();
                if(active) o.SetActive(true);
                return o;
            }

            GameObject obj = UnityEngine.Object.Instantiate(ins);
            obj.name = ins.name;
            return obj;
        }

        public static GameObject Instantiate(GameObject ins)
        {
            string code = ins.name;
            if (unityPool.TryGetValue(code, out Pool pool))
            {
                GameObject o = pool.Reuse();
                o.SetActive(true);
                return o;
            }

            GameObject obj = UnityEngine.Object.Instantiate(ins);
            obj.name = ins.name;
            return obj;
        }

        public static GameObject Instantiate(string path)
        {
            // chỉ áp dụng cho levels
            if (path.StartsWith("Levels/Level"))
            {
                LogUtils.Log("Kit Pool Load Level path " + path, Color.magenta);
                
                // nếu đổi level (win → next) thì unload cũ
                if (!string.IsNullOrEmpty(_lastLevelPath) && _lastLevelPath != path)
                {
                    LogUtils.Log("Unload level cũ path = " + _lastLevelPath, Color.magenta);
                    KitResource.Unload(_lastLevelPath);
                }

                // cập nhật
                _lastLevelPath = path;
            }
            
            GameObject obj = KitResource.LoadInstance<GameObject>(path);
            if (obj == null)
                return null;
            return Instantiate(obj);
        }

        public static T Instantiate<T>(GameObject ins) where T : Component
        {
            GameObject obj = Instantiate(ins);
            if (obj == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"Error instantiate {typeof(T)} from Object: " + ins);
#endif
                return null;
            }

            T component = obj.GetComponent<T>();
#if UNITY_EDITOR
            if (component == null) Debug.LogError($"Error parse to {typeof(T)} from Object: {obj.name}");
#endif
            return obj.GetComponent<T>();
        }

        public static T Instantiate<T>(string path) where T : Component
        {
            GameObject obj = KitResource.LoadInstance<GameObject>(path);
            if (obj == null)
                return null;
            return Instantiate<T>(obj);
        }

        public static void Destroy(GameObject ins)
        {
            if (ins == null) return;
            string code = ins.name;
            if (unityPool.TryGetValue(code, out Pool pool))
            {
                pool.Release(ins);
            }
            else
            {
                UnityEngine.Object.Destroy(ins);
            }
        }
    }
}