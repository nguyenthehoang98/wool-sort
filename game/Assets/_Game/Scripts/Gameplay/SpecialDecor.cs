using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEventBus;

namespace _Game.Scripts.Gameplay
{
    public class SpecialDecor : MonoBehaviour, IListener<YarnLastLayerCleared>
    {
        private YarnPart hostPart;
        private float launchForce = 5f;

        private Transform rootModelPart;
        
        private Collider _collider;
        private Rigidbody rb;
        
        private float margin = 0.1f;
        private bool isDropping = false;

        void Awake()
        {
            // 1) Tìm piece gốc (outermost YarnPart)
            var p = GetComponentInParent<YarnPart>();
            hostPart = p;
            if (hostPart == null)
            {
                Debug.LogError($"[Decor] SpecialDecor {name}: mồ côi, không nằm dưới YarnPart nào. Kéo nó vào làm con của đúng sợi len.", this);
                return;
            }

            // 2) Detach để không bị ẩn khi các layer bị disable
            if (hostPart.transform.parent != null)
                transform.SetParent(hostPart.transform.parent, worldPositionStays: true);
            else
                Debug.LogError($"[Decor] SpecialDecor {name}: hostPart {hostPart.name} không có parent để detach.", this);

            _collider = GetComponent<Collider>();
            if (_collider == null)
                Debug.LogError($"[Decor] SpecialDecor {name}: thiếu Collider.", this);
            rb = GetComponent<Rigidbody>();
            if (rb == null)
                Debug.LogError($"[Decor] SpecialDecor {name}: thiếu Rigidbody (nên gắn sẵn).", this);
        }
        
        private void Start()
        {
            rootModelPart = GameManager.Instance.GetTargetModel();
        }

        void Drop()
        {
            Debug.Log($"[DecorDbg] SpecialDecor Drop: {name} host={(hostPart != null ? hostPart.name : "NULL")}", this);
            if (hostPart == null)
            {
                Debug.LogError($"[Decor] SpecialDecor {name}: hostPart NULL, không Drop được.", this);
                return;
            }
            if (_collider == null || rb == null)
            {
                Debug.LogError($"[Decor] SpecialDecor {name}: thiếu Collider/Rigidbody nên không rơi được.", this);
                return;
            }
            if (rootModelPart == null) rootModelPart = GameManager.Instance.GetTargetModel();
            if (rootModelPart == null)
            {
                Debug.LogError($"[Decor] SpecialDecor {name}: rootModelPart NULL.", this);
                return;
            }
            var decoRoot = GameManager.Instance.GetDecorationRoot();
            if (decoRoot == null)
            {
                Debug.LogError($"[Decor] SpecialDecor {name}: decorationRoot NULL (chưa gán trong GameManager).", this);
                return;
            }
            transform.SetParent(decoRoot, true);
            
            _collider.isTrigger = true;
            
            Vector3 fromModelCenter = (transform.position - rootModelPart.position).normalized;
            Vector3 fromParentPart = (transform.position - hostPart.transform.position).normalized;
            
            rb.isKinematic = false;
            rb.linearDamping = 1.5f;
            rb.angularDamping = 1.5f;
            rb.useGravity = true;
            
            // Trộn 2 hướng: chủ yếu là bay khỏi model, và lệch nhẹ theo yarn cha
            Vector3 mixedDir = (fromModelCenter * 0.2f + fromParentPart * 0.8f).normalized;

            // 2. Nâng hướng bắn theo up của modelRoot (giúp bắn có quỹ đạo vòng cung)
            Vector3 liftedDir = (mixedDir + rootModelPart.up * 0.6f).normalized;  // có thể điều chỉnh *0.4f tùy mức độ cong
            
            // Random nhẹ quanh hướng đó
            Vector3 axis = Vector3.Cross(liftedDir, rootModelPart.up).normalized;
            Quaternion randomRot = Quaternion.AngleAxis(Random.Range(-5, 5), axis);
            Vector3 finalDir = randomRot * liftedDir;
            
            rb.AddForce(finalDir * 5, ForceMode.Impulse);
            
            Vector3 randomTorque = Random.onUnitSphere * Random.Range(0.5f, 2f); // tuỳ chỉnh biên độ
            rb.AddTorque(randomTorque, ForceMode.Impulse);
            
            if (!isDropping)
            {
                isDropping = true;
                GameManager.Instance.StartCoroutine(CheckOffscreen());
            }
        }
        
        private IEnumerator CheckOffscreen()
        {
            while (true)
            {
                if (this == null || gameObject == null)
                    yield break;
                
                Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);

                bool outOfX = viewPos.x < -margin || viewPos.x > 1f + margin;
                bool outOfY = viewPos.y < -margin || viewPos.y > 1f + margin;

                if (outOfX || outOfY)
                {
                    Destroy(gameObject); // hoặc SetActive(false)
                    yield break; // dừng coroutine
                }

                yield return new WaitForSeconds(1f); // check mỗi 1s thay vì mỗi frame
            }
        }
        
        private void OnEnable()
        {
            GlobalBus.Subscribe(this);
        }

        private void OnDisable()
        {
            GlobalBus.UnSubscribe(this);
        }
        

        public void React(in YarnLastLayerCleared e)
        {
            // chỉ drop nếu layer cuối đó thuộc đúng piece này
            var clearedAncestor = (e.YarnPart != null && e.YarnPart.ancestor != null) ? e.YarnPart.ancestor : e.YarnPart;
            if (clearedAncestor == hostPart)
                Drop();
            else
                Debug.Log($"[DecorDbg] SpecialDecor {name}: bỏ qua YarnLastLayerCleared của {e.YarnPart.name} (ancestor {(clearedAncestor != null ? clearedAncestor.name : "NULL")} != host {(hostPart != null ? hostPart.name : "NULL")})", this);
        }
    }
}