using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _KIT;
using UnityEngine;

namespace _KIT
{
    public interface IConfig
    {
        void OnMapValue();
    }

    public static class KitConfigManager
    {
        private static Dictionary<Type, IConfig> cache;

        public static async Task Load(string[] scriptObjectsPath)
        {
            cache = new Dictionary<Type, IConfig>();
            for (int i = 0; i < scriptObjectsPath.Length; i++)
            {
                int index = i;
                ScriptableObject asset = await KitResource.LoadAsync<ScriptableObject>(scriptObjectsPath[i]);
                if (asset is IConfig config)
                {
                    config.OnMapValue();
                    cache.Add(config.GetType(), config);
                }
                else
                {
                    Debug.LogError($"Error parse to {typeof(IConfig)} from: " + scriptObjectsPath[index]);
                }
            }

            await Task.Yield();
        }

        public static T Get<T>() where T : class, IConfig
        {
            if (cache.TryGetValue(typeof(T), out IConfig config))
            {
                return config as T;
            }

            throw new TypeLoadException($"Unload config by type " + typeof(T));
        }
    }
}