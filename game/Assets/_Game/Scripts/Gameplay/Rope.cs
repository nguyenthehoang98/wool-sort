using System.Collections.Generic;
using _Game.Scripts.Pool;
using Dreamteck.Splines;
using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    /// <summary>
    /// Xử lý 1 sợi dây riêng biệt. 
    /// Gắn lên prefab Rope (có SplineComputer và MeshRenderer).
    /// </summary>
    public class Rope : MonoBehaviour, IPoolable
    {
        // --- Tham số runtime ---
        private SplineComputer spline;
        // private SplineRenderer _splineRenderer;
        private SplineMesh _splineMesh;
        private YarnPart yarnPart;
        private Slot slot;
        private Plane ropePlane;
        private Camera orthoCam, perspectiveCam;
        private float sagAmount, swayAmplitude, swayFrequency, retractDuration;

        // --- Trạng thái retract ---
        private bool isRetracting = false;
        private float retractStartTime;
        private Vector3 retractStartPos;

        private float normalSize = 2f;
        private float biggerSize = 2.5f; // scale với waiting để to hơn do cam xa hơn nhìn nhỏ hơn

        // Ring Sampling - Số ring cắt trên chiều cao UV2 (càng nhiều → path càng mượt)")]
        private int ringSampleCount = 20;

        // Lưu các điểm cắt trên từng ring (UV2.y mẫu)
        private List<Vector3>[] ringPointsLocal;
        private float uv2MinY, uv2MaxY;

        private Vector3 bottomWorld;
        private Vector3 lastBottomLocal;

        private Vector3 topWorld;
        private bool isSpawning;
        private float spawnStartTime;
        private Vector3 spawnStartWorld;
        private Vector3 spawnEndWorld;
        private float spawnDuration = 0.2f;

        // Nhân hệ số sway khi retract"
        private float retractSwayMultiplier = 3f;
        // Nhân hệ số frequency khi retract (giảm cho chậm hơn)")]
        private float retractFrequencyMultiplier = 0.5f;

        private bool _firstFrame = true;
        private MeshRenderer mr;
        
        /// <summary>
        /// Thiết lập ban đầu cho RopeBehaviour.
        /// Gọi ngay sau khi Instantiate(prefab).
        /// </summary>
        public void Initialize(
            YarnPart part,
            Slot slot,
            Material ropeMaterial,
            Camera orthoCam,
            Camera perspectiveCam,
            Plane ropePlane,
            float sagAmount,
            float swayAmplitude,
            float swayFrequency,
            float retractDuration)
        {
            this.yarnPart = part;
            this.slot = slot;
            this.orthoCam = orthoCam;
            this.perspectiveCam = perspectiveCam;
            this.ropePlane = ropePlane;
            this.sagAmount = sagAmount;
            this.swayAmplitude = swayAmplitude;
            this.swayFrequency = swayFrequency;
            this.retractDuration = Mathf.Max(0.01f, retractDuration);

            // Lấy component spline và khởi tạo
            spline = GetComponent<SplineComputer>();
            if (spline == null)
            {
                Debug.LogError("RopeBehaviour thiếu SplineComputer trên prefab!", this);
                return;
            }

            // _splineRenderer = GetComponent<SplineRenderer>();
            // _splineRenderer.size = slot.Area == Slot.AreaType.Waiting ? biggerSize : normalSize;
            _splineMesh = GetComponent<SplineMesh>();
            var channel = _splineMesh.GetChannel(0); // hoặc theo tên: GetChannel("Main")
            var scaleFactor = slot.Area == Slot.AreaType.Waiting ? biggerSize : normalSize;
            var scaleVector = new Vector3(scaleFactor, scaleFactor);
            channel.minScale = scaleVector;
            channel.maxScale = scaleVector;

            spline.space = SplineComputer.Space.World;
            spline.type = Spline.Type.CatmullRom;
            spline.sampleMode = SplineComputer.SampleMode.Uniform;
            spline.sampleRate = 30;

            // Khởi tạo bằng các điểm có khoảng cách để Dreamteck không tính LookRotation(0).
            Vector3 initialStart = yarnPart != null ? yarnPart.transform.position : transform.position;
            Vector3 initialEnd = slot != null ? slot.GetAnchorWorldPosition() : initialStart + Vector3.forward;
            if ((initialEnd - initialStart).sqrMagnitude < 0.0001f)
                initialEnd += Vector3.forward * 0.01f;

            var points = new SplinePoint[5];
            for (int i = 0; i < points.Length; i++)
                points[i] = new SplinePoint(Vector3.Lerp(initialStart, initialEnd, i / (points.Length - 1f)), Vector3.back);
            spline.SetPoints(points);
            spline.RebuildImmediate();

            // Gán material và properties
            mr = GetComponent<MeshRenderer>();
            if (mr != null) mr.material = ropeMaterial;
            SetRopeMaterialProperty(mr);

            isRetracting = false;
            isSpawning = true;
            spawnStartTime = Time.time;

            // Precompute ring points dựa trên UV2
            PrecomputeRingPoints();

            // Khởi tạo lastBottomLocal = một điểm bất kỳ trên ring đầu
            if (ringPointsLocal != null && ringPointsLocal.Length > 0 && ringPointsLocal[0].Count > 0)
                lastBottomLocal = ringPointsLocal[0][0];
            else
                lastBottomLocal = Vector3.zero;

            // spawn start world = bottomWorld
            spawnStartWorld = yarnPart.transform.TransformPoint(lastBottomLocal);
            spawnEndWorld = ComputeTopWorld();
            
            if (mr != null) mr.enabled = false;
            _firstFrame = true;
        }

        /// <summary>
        /// Precompute các điểm cắt trên mesh tại các giá trị UV2.y mẫu.
        /// Chỉ chạy 1 lần trong Initialize.
        /// </summary>
        private void PrecomputeRingPoints()
        {
            var mf = yarnPart.GetComponent<MeshFilter>();
            var mesh = mf != null ? mf.sharedMesh : null;
            var uvs = mesh != null ? mesh.uv2 : null;
            if (mesh == null || uvs == null || uvs.Length != mesh.vertexCount || uvs.Length == 0)
            {
                ringPointsLocal = null;
                return;
            }

            // Tìm giá trị min/max UV2.y (mesh có thể không bắt đầu 0/1)
            uv2MinY = uv2MaxY = uvs[0].y;
            for (int i = 1; i < uvs.Length; i++)
            {
                uv2MinY = Mathf.Min(uv2MinY, uvs[i].y);
                uv2MaxY = Mathf.Max(uv2MaxY, uvs[i].y);
            }

            ringPointsLocal = new List<Vector3>[ringSampleCount];
            var verts = mesh.vertices;
            var tris = mesh.triangles;

            for (int r = 0; r < ringSampleCount; r++)
            {
                float t = r / (float)(ringSampleCount - 1);
                float targetY = Mathf.Lerp(uv2MaxY, uv2MinY, t);
                var list = new List<Vector3>();

                for (int k = 0; k < tris.Length; k += 3)
                {
                    SampleEdge(tris[k], tris[k + 1], uvs, verts, targetY, list);
                    SampleEdge(tris[k + 1], tris[k + 2], uvs, verts, targetY, list);
                    SampleEdge(tris[k + 2], tris[k], uvs, verts, targetY, list);
                }

                ringPointsLocal[r] = list;
            }
        }

        /// <summary>
        /// Nội suy vị trí trên cạnh nếu targetY nằm giữa.
        /// </summary>
        private void SampleEdge(int a, int b, Vector2[] uvs, Vector3[] verts, float y, List<Vector3> outPts)
        {
            float y0 = uvs[a].y, y1 = uvs[b].y;
            if (y >= Mathf.Min(y0, y1) && y <= Mathf.Max(y0, y1) && !Mathf.Approximately(y0, y1))
            {
                float tt = (y - y0) / (y1 - y0);
                outPts.Add(Vector3.Lerp(verts[a], verts[b], tt));
            }
        }

        private void Update()
        {
            if (spline == null || yarnPart == null) return;

            // Tính điểm đầu (slot) trong không gian world
            if (isSpawning)
            {
                float st = Mathf.Clamp01((Time.time - spawnStartTime) / spawnDuration);
                topWorld = Vector3.Lerp(spawnStartWorld, spawnEndWorld, st);
                if (st >= 1f) isSpawning = false;
            }
            else topWorld = ComputeTopWorld();

            // raw = progress dissolve [0…1]; p = 1−raw
            float raw = Mathf.Clamp01(yarnPart.DissolveProgress);
            float p = 1f - raw;

            if (raw > 0f)
            {
                // 1) Yarn vẫn còn (dissolveProgress > 0) → bottomWorld bám lên mặt mesh
                //    dựa trên các ring đã precompute
                float f = (1f - raw) * (ringSampleCount - 1);
                int i0 = Mathf.FloorToInt(f);
                int i1 = Mathf.Min(i0 + 1, ringSampleCount - 1);
                float frac = f - i0;

                if (ringPointsLocal != null && ringPointsLocal[i0].Count > 0 && ringPointsLocal[i1].Count > 0)
                {
                    // Chọn điểm trên ring sao cho dây bám liên tục trên mesh.
                    var list0 = ringPointsLocal[i0];
                    var list1 = ringPointsLocal[i1];
                    Vector3 pick0 = PickFarthest(list0, lastBottomLocal);
                    Vector3 pick1 = PickFarthest(list1, pick0);
                    Vector3 bottomLocal = Vector3.Lerp(pick0, pick1, frac);
                    bottomWorld = yarnPart.transform.TransformPoint(bottomLocal);
                    lastBottomLocal = bottomLocal;
                }
                else
                {
                    // Mesh thiếu UV2 hợp lệ: vẫn vẽ dây từ tâm part.
                    bottomWorld = yarnPart.transform.position;
                }
            }
            else
            {
                // Khi đã dissolve hết, bật retract
                if (!isRetracting)
                {
                    isRetracting = true;
                    retractStartTime = Time.time;
                    retractStartPos = bottomWorld;
                }

                // Tính tỉ lệ retract [0…1]
                float rt = Mathf.Clamp01((Time.time - retractStartTime) / retractDuration);
                bottomWorld = Vector3.Lerp(retractStartPos, topWorld, rt);

                if (rt >= 1f)
                {
                    // Tắt trước khi trả object về pool; không rebuild một spline suy biến.
                    if (mr != null) mr.enabled = false;
                    Despawn();
                    return;
                }
            }

            // Cập nhật từng point trên spline: đầu, cuối, và sag+sway ở giữa
            int count = spline.pointCount;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / (count - 1);
                Vector3 pos;
                if (i == 0) pos = topWorld;
                else if (i == count - 1) pos = bottomWorld;
                else
                {
                    float rt = isRetracting ? Mathf.Clamp01((Time.time - retractStartTime) / retractDuration) : 1f;
                    float currentSway = swayAmplitude * (isRetracting ? retractSwayMultiplier : 1f);
                    float currentFreq = swayFrequency * (isRetracting ? retractFrequencyMultiplier : 1f);
                    pos = ComputeSagSwayPoint(t, topWorld, bottomWorld, raw, sagAmount, currentSway, currentFreq);
                }

                spline.SetPointPosition(i, pos);
            }
            spline.RebuildImmediate();
            
            // lần Update đầu tiên xong xuôi thì bật lại renderer
            if (_firstFrame)
            {
                if (mr != null) mr.enabled = true;
                _firstFrame = false;
            }
        }

        private Vector3 PickFarthest(List<Vector3> list, Vector3 reference)
        {
            Vector3 best = reference;
            float maxDist = -1f;
            foreach (var p in list)
            {
                float d = Vector3.SqrMagnitude(p - reference);
                if (d > maxDist)
                {
                    maxDist = d;
                    best = p;
                }
            }
            return best;
        }

        /// <summary>
        /// Tính vị trí điểm đầu (gốc slot) trên world từ screen raycast.
        /// </summary>
        private Vector3 ComputeTopWorld()
        {
            Vector3 anchor = slot != null ? slot.GetAnchorWorldPosition() : transform.position;
            if (orthoCam == null || perspectiveCam == null || slot == null)
                return anchor;

            Vector3 screen = orthoCam.WorldToScreenPoint(anchor);
            Ray ray = perspectiveCam.ScreenPointToRay(screen);
            return ropePlane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : anchor;
        }

        /// <summary>
        /// Tính sag (võng) + sway (đung đưa) tại t giữa 0…1.
        /// </summary>
        private Vector3 ComputeSagSwayPoint(float t, Vector3 top, Vector3 bottom, float raw,
            float sagAmt, float swayAmt, float freq)
        {
            Vector3 dir = (bottom - top).normalized;
            Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized;
            Vector3 basePos = Vector3.Lerp(top, bottom, t) + Vector3.down * sagAmt;
            float swayF = isRetracting ? (1f - Mathf.Clamp01((Time.time - retractStartTime) / retractDuration)) : (1f - raw);
            float sway = Mathf.Sin(Time.time * freq + t * Mathf.PI) * swayAmt * swayF;
            return basePos + perp * sway;
        }

        static readonly int ThreadAnim = Shader.PropertyToID("_ThreadAnim");
        /// <summary>
        /// Thiết lập thêm các property block cho shader dây.
        /// </summary>
        private void SetRopeMaterialProperty(MeshRenderer mr)
        {
            if (mr == null) return;
            var mpb = new MaterialPropertyBlock();
            mr.GetPropertyBlock(mpb);
            mpb.SetFloat(ThreadAnim, 1f);
            mr.SetPropertyBlock(mpb);
        }

        /// <summary>
        /// Xóa rope ngay lập tức (nếu cần gọi thủ công).
        /// </summary>
        public void ClearRope()
        {
            Despawn();
        }


        //  vẽ: slotAnchor (biểu thị ray bắt đầu), tia ray từ camera perspective đến plane, điểm giao (hit) để xem chính xác topWorld.
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (orthoCam == null || perspectiveCam == null) return;

            // 1) màu cho slotAnchor
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(slot.GetAnchorWorldPosition(), 0.05f);

            // 2) tính ray từ ortho → perspective
            Vector3 screen = orthoCam.WorldToScreenPoint(slot.GetAnchorWorldPosition());
            Ray ray = perspectiveCam.ScreenPointToRay(screen);

            // 3) tìm giao với plane y = planeY
            Plane plane = new Plane(Vector3.up, Vector3.up * YarnRopeController.Instance.planeY);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hit = ray.GetPoint(enter);
                // vẽ line và điểm hit
                Gizmos.color = Color.red;
                Gizmos.DrawLine(ray.origin, hit);
                Gizmos.DrawSphere(hit, 0.05f);
            }
        }
#endif
        public void OnSpawned()
        {
            transform.SetParent(PoolManager.Instance.PoolRoot, worldPositionStays: false);
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Trả về pool thay vì Destroy
        /// </summary>
        public void Despawn()
        {
            YarnRopeController.Instance.ReleaseRope(this);
        }
    }
}
