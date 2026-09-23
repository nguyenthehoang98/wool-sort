using System;
using UnityEngine;

namespace _KIT
{
    public class KitEnum<T> where T : Enum
    {
        private T value;
        private readonly string key;

        public KitEnum(string key, T defaultValue)
        {
            this.key = key;
            value = defaultValue;
        }

        public void Set(T value)
        {
            this.value = value;
            PlayerPrefs.SetString(key, this.value.ToString());
            PlayerPrefs.Save();
        }

        public T Get()
        {
            string data = PlayerPrefs.GetString(key, value.ToString());
            return (T)Enum.Parse(typeof(T), data);
        }
    }
}