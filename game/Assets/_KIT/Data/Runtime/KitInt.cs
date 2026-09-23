using UnityEngine;

namespace _KIT
{
    public class KitInt
    {
        private int value;
        private readonly string key;

        public KitInt(string key, int defaultValue)
        {
            this.key = key;
            value = defaultValue;
        }

        public void Set(int value)
        {
            this.value = value;
            PlayerPrefs.SetInt(key, this.value);
            PlayerPrefs.Save();
        }

        public int Get()
        {
            return PlayerPrefs.GetInt(key, value);
        }
    }
}