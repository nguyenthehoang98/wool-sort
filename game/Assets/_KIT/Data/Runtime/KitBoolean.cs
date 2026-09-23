using UnityEngine;

namespace _KIT
{
    public class KitBoolean
    {
        private bool value;
        private readonly string key;

        public KitBoolean(string key, bool defaultValue)
        {
            this.key = key;
            value = defaultValue;
        }

        public void Set(bool value)
        {
            this.value = value;
            PlayerPrefs.SetInt(key, this.value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public bool Get()
        {
            return PlayerPrefs.GetInt(key, value ? 1 : 0) == 1;
        }
    }
}