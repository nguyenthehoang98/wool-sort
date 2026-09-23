using UnityEngine;
using UnityEngine.Pool;

namespace _Game.Scripts.Pool
{
    public class PrefabPool<T> where T : MonoBehaviour, IPoolable
    {
        private ObjectPool<T> pool;
        private T prefab;

        public PrefabPool(T prefab, int capacity = 10, int maxSize = 50)
        {
            this.prefab = prefab;
            pool = new ObjectPool<T>(
                () => GameObject.Instantiate(prefab),
                obj => obj.OnSpawned(),
                obj => obj.OnDespawned(),
                GameObject.Destroy,
                false, capacity, maxSize
            );
        }

        public T Get() => pool.Get();
        public void Release(T obj) => pool.Release(obj);
    }
}