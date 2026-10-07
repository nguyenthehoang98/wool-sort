// YarnDecoration.cs

using System;
using System.Collections;
using Lean.Touch;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Game.Scripts.Gameplay
{
    public class YarnDecoration : MonoBehaviour
    {
        [Tooltip("Lực bật ra min")]
        private float minEjectionForce = 6f;
        [Tooltip("Lực bật ra max")]
        private float maxEjectionForce = 8f;
        [Tooltip("Góc ngẫu nhiên (độ) cho hướng bật ra")]
        private float spreadAngle = 5f;

        private float margin = 0.1f;
        private bool isDropping = false;

        private Transform rootModelPart;
        YarnPart parentPart;
        Rigidbody rb;

        void Awake()
        {
            parentPart = GetComponentInParent<YarnPart>();
           
            rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        private void Start()
        {
            rootModelPart = GameManager.Instance.GetTargetModel();
        }

        /// <summary>Gọi khi part dissolve xong để drop decoration.</summary>
        public void Drop()
        {
            Debug.Log($"[DecorDbg] Drop gọi: {name} parentPart={(parentPart != null ? parentPart.name : "NULL")} pos={transform.position}", this);
            if (parentPart == null)
            {
                Debug.LogError($"[DecorDbg] {name}: parentPart NULL -> decor mồ côi, không tính được hướng bắn. Kiểm tra lại hierarchy.", this);
                return;
            }
            if (rootModelPart == null) rootModelPart = GameManager.Instance.GetTargetModel();
            if (rootModelPart == null)
            {
                Debug.LogError($"[DecorDbg] {name}: rootModelPart NULL.", this);
                return;
            }
            var decoRoot = GameManager.Instance.GetDecorationRoot();
            if (decoRoot == null)
            {
                Debug.LogError($"[DecorDbg] {name}: decorationRoot NULL (chưa gán trong GameManager).", this);
                return;
            }
            transform.SetParent(decoRoot, true);

            var mc = GetComponent<MeshCollider>();
            if (mc != null) mc.convex = true;

            rb.isKinematic = false;
            rb.linearDamping = 1.36f;
            rb.angularDamping = 1.36f;

            Vector3 fromModelCenter = (transform.position - rootModelPart.position).normalized;
            Vector3 fromParentPart = (transform.position - parentPart.transform.position).normalized;

            // Nếu fromParentPart hướng ngược với fromModelCenter → decor nằm trong thân → ép bắn ra ngoài
            if (Vector3.Dot(fromModelCenter, fromParentPart) < 0f)
                fromParentPart = fromModelCenter;
            
            // Trộn 2 hướng: chủ yếu là bay khỏi model, và lệch nhẹ theo yarn cha
            Vector3 mixedDir = (fromModelCenter * 0.2f + fromParentPart * 0.8f).normalized;

            // 2. Nâng hướng bắn theo up của modelRoot (giúp bắn có quỹ đạo vòng cung)
            Vector3 liftedDir = (mixedDir + rootModelPart.up * 0.6f).normalized;  // có thể điều chỉnh *0.4f tùy mức độ cong
            
            // Random nhẹ quanh hướng đó
            Vector3 axis = Vector3.Cross(liftedDir, rootModelPart.up).normalized;
            Quaternion randomRot = Quaternion.AngleAxis(Random.Range(-spreadAngle, spreadAngle), axis);
            Vector3 finalDir = randomRot * liftedDir;

            // Force random
            float force = Random.Range(minEjectionForce, maxEjectionForce);
            rb.AddForce(finalDir * force, ForceMode.Impulse);
            
            // Tạo torque xoay loạng choạng
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
                // Nếu component này hoặc GameObject đã bị Destroy/null thì dừng coroutine
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

                yield return new WaitForSeconds(0.5f); // check mỗi 0.5s thay vì mỗi frame
            }
        }
        
#if UNITY_EDITOR
        
        // Mỗi màu thể hiện 1 bước xử lý:
        // Green: từ center của model
        // Red: từ part cha
        // Yellow: mixedDir
        // Cyan: liftedDir
        // Blue: finalDir sẽ dùng để AddForce
        private void OnDrawGizmosSelected()
        {
            if (parentPart == null || rootModelPart == null) return;

            Vector3 fromModelCenter = (transform.position - rootModelPart.position).normalized;
            Vector3 fromParentPart = (transform.position - parentPart.transform.position).normalized;
            // Nếu fromParentPart hướng ngược với fromModelCenter → decor nằm trong thân → ép bắn ra ngoài
            if (Vector3.Dot(fromModelCenter, fromParentPart) < 0f)
                fromParentPart = fromModelCenter;
            Vector3 mixedDir = (fromModelCenter * 0.2f + fromParentPart * 0.8f).normalized;
            Vector3 liftedDir = (mixedDir + rootModelPart.up * 0.6f).normalized;
            Vector3 axis = Vector3.Cross(liftedDir, rootModelPart.up).normalized;
            Quaternion randomRot = Quaternion.AngleAxis(0f, axis); // không random để debug ổn định
            Vector3 finalDir = randomRot * liftedDir;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + fromModelCenter * 0.5f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + fromParentPart * 0.5f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + mixedDir * 0.5f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + liftedDir * 0.5f);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + finalDir * 0.7f);
        }
#endif
    }
}