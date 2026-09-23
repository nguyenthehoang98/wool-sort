using UnityEngine;

namespace _KIT
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        // Start is called before the first frame update
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
