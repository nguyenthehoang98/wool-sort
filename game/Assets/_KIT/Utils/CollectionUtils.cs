using System;
using System.Collections.Generic;
using System.Linq;

namespace _KIT
{
    public static class CollectionUtils
    {
        public static void Shuffle<T>(ref List<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        
        public static void Shuffle<TKey, TValue>(ref Dictionary<TKey, TValue> input)
        {
            Dictionary<TKey, TValue> dic = input.OrderBy(x => Guid.NewGuid()).ToDictionary(x => x.Key, x => x.Value);
            input = dic;
        }

        public static void InsertAt<T>(ref T[] array, int index, T newItem)
        {
            if (index < 0 || index > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }

            T[] newArray = new T[array.Length + 1];
            for (int i = 0; i < index; i++)
            {
                newArray[i] = array[i];
            }

            newArray[index] = newItem;
            for (int i = index; i < array.Length; i++)
            {
                newArray[i + 1] = array[i];
            }

            array = newArray;
        }
    }
}