// PoolManager.cs

using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts.Pool
{
    [DefaultExecutionOrder(-99)] // đảm bảo chạy trước các manager khác, và sau game manager
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [SerializeField] private Transform poolRootContainer;
        public Transform PoolRoot => poolRootContainer;

        // Dictionary giữ mọi PrefabPool<T> dưới dạng object
        private Dictionary<string, object> poolDict = new Dictionary<string, object>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
        }

        /// <summary>
        /// Thiết lập pool cho prefab loại T với key chỉ định.
        /// Gọi 1 lần (thường trong Awake của controller).
        /// </summary>
        public void Register<T>(string key, GameObject prefab, int defaultCapacity = 10, int maxSize = 50)
        where T : MonoBehaviour, IPoolable
        {
            if (poolDict.ContainsKey(key))
            {
                Debug.LogWarning($"PoolManager: key '{key}' đã tồn tại, ghi đè.");
                poolDict.Remove(key);
            }

            var comp = prefab.GetComponent<T>();
            if (comp == null)
            {
                Debug.LogError($"PoolManager.Register: prefab không có component {typeof(T).Name}");
                return;
            }

            var pool = new PrefabPool<T>(comp, defaultCapacity, maxSize);
            poolDict[key] = pool;
        }

        /// <summary>
        /// Lấy 1 instance từ pool với key.
        /// </summary>
        public T Get<T>(string key) where T : MonoBehaviour, IPoolable
        {
            if (!poolDict.TryGetValue(key, out var obj))
            {
                Debug.LogError($"PoolManager.Get: không tìm thấy pool cho key '{key}'");
                return null;
            }
            return ((PrefabPool<T>)obj).Get();
        }

        /// <summary>
        /// Trả 1 instance về pool.
        /// </summary>
        public void Release<T>(string key, T instance) where T : MonoBehaviour, IPoolable
        {
            if (!poolDict.TryGetValue(key, out var obj))
            {
                Debug.LogError($"PoolManager.Release: không tìm thấy pool cho key '{key}'");
                return;
            }
            ((PrefabPool<T>)obj).Release(instance);
        }
    }
}
