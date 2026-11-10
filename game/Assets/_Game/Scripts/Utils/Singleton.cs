using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HyperSonic
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = FindObjectOfType<T>();
                    if(instance == null)
                    {
                        Debug.LogErrorFormat("[SINGLETON] The class {0} could not be found in the scene!", typeof(T));
                    }
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            // Nhận làm instance khi: chưa có, HOẶC instance cũ thuộc scene KHÁC
            // (vừa chuyển scene — bản cũ sắp bị huỷ) => scene mới bind lại.
            if(instance == null || instance.gameObject.scene != gameObject.scene)
            {
                instance = this as T;
                OnAwake();
            }
            else if(instance != this)
            {
                Debug.LogWarningFormat("[SINGLETON] There is more than one instance of class {0} in the scene!", typeof(T));
                Destroy(this.gameObject);
                return;
            }
        }

        protected virtual void OnAwake()
        {

        }

        protected virtual void OnDestroy()
        {
            // Chỉ gỡ khi chính mình là instance hiện tại. Bản trùng bị huỷ
            // (destroy duplicate) không được xoá nhầm instance hợp lệ — nếu không,
            // chuyển scene xong singleton bị null oan dù scene mới đã có object.
            if(ReferenceEquals(instance, this))
                instance = null;
        }

        public static bool HasInstance => instance != null;

        public void Preload() { }
    }
}


