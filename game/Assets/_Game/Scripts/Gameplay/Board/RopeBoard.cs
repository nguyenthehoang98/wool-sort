using _Game.Scripts.Pool;
using Dreamteck.Splines;
using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    /// <summary>
    /// Xử lý một sợi dây giữa hai điểm (Transform) bất kỳ.
    /// Gắn lên prefab RopeBoard (cần có SplineComputer, SplineMesh, MeshRenderer).
    /// </summary>
    public class RopeBoard : MonoBehaviour, IPoolable
    {
        // --- Tham số runtime, khởi tạo ở hàm Initialize ---
        private Transform botPoint;        // Điểm bắt đầu dây (Transform)
        private Transform topPoint;        // Điểm kết thúc dây (Transform)
        private Material ropeMaterial;     // Vật liệu của dây
        private float sagAmount;           // Độ võng của dây
        private float swayAmplitude;       // Biên độ đung đưa
        private float swayFrequency;       // Tần số đung đưa
        private float retractDuration;     // Thời gian thu dây
        private Plane ropePlane;
        private Camera orthoCam, perspectiveCam;

        // --- Thành phần spline và mesh ---
        private SplineComputer spline;
        private SplineMesh splineMesh;
        private MeshRenderer meshRenderer;

        // --- Trạng thái dây bay ---
        private float _ropeFlyDuration;
        private float _ropeFlyElapsed;
        private Vector3 _ropeFlyStartPos;

        // --- Trạng thái khởi tạo renderer ---
        private bool _firstFrame = true;


        /// <summary>
        /// Khởi tạo RopeBoard với các tham số truyền vào.
        /// Gọi ngay sau khi Instantiate(prefab).
        /// </summary>
        /// <param name="botPoint">Transform điểm bắt đầu dây</param>
        /// <param name="topPoint">Transform điểm kết thúc dây</param>
        /// <param name="material">Vật liệu dây</param>
        /// <param name="orthoCam">Camera ortho</param>
        /// <param name="perspectiveCam">Camera perspective</param>
        /// <param name="ropePlane">Plane của dây</param>
        /// <param name="sag">Độ võng</param>
        /// <param name="swayAmp">Biên độ đung đưa</param>
        /// <param name="swayFreq">Tần số đung đưa</param>
        /// <param name="retractTime">Thời gian thu dây</param>
        /// <param name="ropeFlyTime">Thời gian dây bay</param>
        public void Initialize(
            Transform botPoint,
            Transform topPoint,
            Material material,
            Camera orthoCam,
            Camera perspectiveCam,
            Plane ropePlane,
            float sag,
            float swayAmp,
            float swayFreq,
            float retractTime,
            float ropeFlyTime)
        {
            // Lưu các tham số truyền vào
            this.botPoint = botPoint;
            this.topPoint = topPoint;
            ropeMaterial = material;
            this.orthoCam = orthoCam;
            this.perspectiveCam = perspectiveCam;
            this.ropePlane = ropePlane;
            sagAmount = sag;
            swayAmplitude = swayAmp;
            swayFrequency = swayFreq;
            retractDuration = Mathf.Max(0.01f, retractTime);
            // Dùng đúng thời lượng animation để tốc độ tự thay đổi theo quãng đường.
            _ropeFlyDuration = ropeFlyTime > 0f ? ropeFlyTime : retractDuration;
            _ropeFlyElapsed = 0f;

            // Lấy component spline và mesh
            spline = GetComponent<SplineComputer>();
            splineMesh = GetComponent<SplineMesh>();
            meshRenderer = GetComponent<MeshRenderer>();

            if (spline == null)
            {
                Debug.LogError("RopeBoard thiếu SplineComputer!", this);
                return;
            }

            // Thiết lập scale cho mesh (cố định 2f, có thể chỉnh lại nếu cần)
            if (splineMesh != null)
            {
                var channel = splineMesh.GetChannel(0);
                var scaleVector = new Vector3(2f, 2f);
                channel.minScale = scaleVector;
                channel.maxScale = scaleVector;
            }

            // Thiết lập spline: kiểu CatmullRom, sample đều, không gian world
            spline.space = SplineComputer.Space.World;
            spline.type = Spline.Type.CatmullRom;
            spline.sampleMode = SplineComputer.SampleMode.Uniform;
            spline.sampleRate = 30;

            // Khởi tạo bằng start/end thật để Dreamteck không LookRotation trên vector zero.
            _ropeFlyStartPos = ComputePos(botPoint, true);
            var initialTop = ComputePos(topPoint, false);
            if ((initialTop - _ropeFlyStartPos).sqrMagnitude < 0.0001f)
                initialTop += Vector3.forward * 0.01f;

            var points = new SplinePoint[5];
            for (int i = 0; i < points.Length; i++)
            {
                var position = Vector3.Lerp(_ropeFlyStartPos, initialTop, i / (points.Length - 1f));
                points[i] = new SplinePoint(position, Vector3.back);
            }
            spline.SetPoints(points);
            spline.RebuildImmediate();

            // Gán vật liệu cho renderer và thiết lập property shader
            if (meshRenderer != null && ropeMaterial != null)
            {
                meshRenderer.material = ropeMaterial;
                SetRopeMaterialProperty(meshRenderer);
            }

            _firstFrame = true;
            _nullLogged = false;
            if (meshRenderer != null) meshRenderer.enabled = false;

        }

        private bool _nullLogged;

        void Update()
        {
            // Nếu chưa khởi tạo spline hoặc điểm bắt đầu/kết thúc thì bỏ qua
            if (spline == null || botPoint == null || topPoint == null)
            {
                if (!_nullLogged)
                {
                    _nullLogged = true;
                    Debug.LogError($"[RopeBoard] {name}: spline/botPoint/topPoint null nên dây không hiện. Kiểm tra anchor truyền vào SpawnRopeBoard.", this);
                }
                return;
            }

            Vector3 topWorld = ComputePos(topPoint, false);
            _ropeFlyElapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_ropeFlyElapsed / _ropeFlyDuration);

            // Dây co dần trong đúng thời lượng animation, nên quãng đường dài/ngắn
            // đều kết thúc đồng bộ với animation thay vì phụ thuộc tốc độ cố định.
            Vector3 botWorld = Vector3.Lerp(_ropeFlyStartPos, topWorld, progress);

            // Khi bay xong, tắt trước khi trả object về pool để tránh rebuild spline suy biến.
            if (progress >= 1f)
            {
                if (meshRenderer != null) meshRenderer.enabled = false;
                Despawn();
                return;
            }

            // Cập nhật từng điểm trên spline: bắt đầu, kết thúc, và các điểm giữa có sag + sway
            int count = spline.pointCount;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / (count - 1);
                Vector3 pos;
                if (i == 0) pos = botWorld; // Điểm bắt đầu
                else if (i == count - 1) pos = topWorld; // Điểm kết thúc
                else
                {
                    float currentSway = swayAmplitude;
                    float currentFreq = swayFrequency;
                    pos = ComputeSagSwayPoint(t, botWorld, topWorld, sagAmount, currentSway, currentFreq);
                }
                spline.SetPointPosition(i, pos);
            }
            spline.RebuildImmediate();

            // Bật renderer sau frame đầu tiên để tránh hiện tượng nháy
            if (_firstFrame && meshRenderer != null)
            {
                meshRenderer.enabled = true;
                _firstFrame = false;
            }
        }

        /// <summary>
        /// Tính vị trí điểm giữa dây với hiệu ứng võng và đung đưa.
        /// </summary>
        /// <param name="t">Tỉ lệ vị trí trên dây (0…1)</param>
        /// <param name="bot">Vị trí bắt đầu</param>
        /// <param name="top">Vị trí kết thúc</param>
        /// <param name="sagAmt">Độ võng</param>
        /// <param name="swayAmt">Biên độ đung đưa</param>
        /// <param name="freq">Tần số đung đưa</param>
        /// <returns>Vị trí điểm đã tính toán</returns>
        private Vector3 ComputeSagSwayPoint(float t, Vector3 bot, Vector3 top, float sagAmt, float swayAmt, float freq)
        {
            // Tính hướng dây và vector vuông góc để đung đưa
            Vector3 dir = (top - bot).normalized;
            Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized;
            // Vị trí cơ bản (có võng)
            Vector3 basePos = Vector3.Lerp(bot, top, t) + Vector3.down * sagAmt;
            // Hiệu ứng đung đưa theo thời gian và vị trí trên dây
            float sway = Mathf.Sin(Time.time * freq + t * Mathf.PI) * swayAmt;
            return basePos + perp * sway;
        }

        // ID property shader cho hiệu ứng dây
        static readonly int ThreadAnim = Shader.PropertyToID("_ThreadAnim");

        /// <summary>
        /// Thiết lập property block cho shader dây (nếu cần hiệu ứng động).
        /// </summary>
        /// <param name="mr">MeshRenderer của dây</param>
        private void SetRopeMaterialProperty(MeshRenderer mr)
        {
            if (mr == null) return;
            var mpb = new MaterialPropertyBlock();
            mr.GetPropertyBlock(mpb);
            mpb.SetFloat(ThreadAnim, 1f);
            mr.SetPropertyBlock(mpb);
        }

        private Vector3 ComputePos(Transform trans, bool isStartPos)
        {
            if (trans == null) return transform.position;

            Vector3 pos = trans.position;
            if (orthoCam != null && perspectiveCam != null)
            {
                Vector3 screen = orthoCam.WorldToScreenPoint(trans.position);
                Ray ray = perspectiveCam.ScreenPointToRay(screen);
                if (ropePlane.Raycast(ray, out float enter))
                    pos = ray.GetPoint(enter);

                if (isStartPos)
                    pos = Vector3.Lerp(perspectiveCam.transform.position, pos, 0.75f);
            }

            return pos;
        }

        /// <summary>
        /// Được gọi khi object được lấy ra từ pool.
        /// </summary>
        public void OnSpawned()
        {
            // Đặt lại parent về PoolRoot để quản lý
            transform.SetParent(PoolManager.Instance.PoolRoot, worldPositionStays: false);
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Được gọi khi object bị trả về pool.
        /// </summary>
        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Trả về pool thay vì Destroy.
        /// </summary>
        public void Despawn()
        {
            YarnRopeController.Instance.ReleaseRopeBoard(this);
        }
    }
}
