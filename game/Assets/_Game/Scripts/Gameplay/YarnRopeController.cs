using System.Collections.Generic;
using _Game.Scripts.Pool;
using _Game.Scripts.Core;
using DG.Tweening;
using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    /// <summary>
    /// Manager để sinh nhiều RopeBehaviour cùng lúc.
    /// Gắn lên 1 GameObject singleton trong scene.
    /// </summary>
    public class YarnRopeController : MonoBehaviour
    {
        public static YarnRopeController Instance { get; private set; }

        [Header("Prefab & Cameras")]
        public GameObject ropePrefab;       // Prefab phải có RopeBehaviour
        public GameObject ropeBoardPrefab;  // Prefab phải có RopeBoard
        public Camera orthoCam;             // Camera UI (Orthographic)
        public Camera perspectiveCam;       // Camera world (Perspective)

        [Header("Plane & Dynamics")]
        public float planeY = 3.5f;           // độ cao mặt phẳng dây
        public float sagAmount = 0.8f;      // võng dây
        public float swayAmplitude = 0.5f;  // biên độ đung đưa
        public float swayFrequency = 20f;    // tần số đung đưa
        public float retractDuration = 0.1f;// thời gian co dây

        private float planeSize = 25f; // kích thước hình vuông để vẽ

        private Plane ropePlane;
        private Plane ropeBoardPlane;
        private List<Rope> ropes = new List<Rope>();
        private List<RopeBoard> ropeBoards = new List<RopeBoard>(); // Danh sách quản lý RopeBoard

        private void Awake()
        {
            Instance = this;
            // Tạo plane ngang tại y = planeY
            ropePlane = new Plane(Vector3.up, Vector3.up * planeY);

            // Tạo plane dọc tại z = planeY (offset theo trục Z)
            ropeBoardPlane = new Plane(Vector3.forward, Vector3.forward * planeY);

            // Đăng ký prefab vào PoolManager (chỉ làm 1 lần)
            PoolManager.Instance.Register<Rope>("rope", ropePrefab);
            PoolManager.Instance.Register<RopeBoard>("ropeBoard", ropeBoardPrefab); // Đăng ký RopeBoard
        }

        /// <summary>
        /// Sinh mới 1 sợi dây nối từ slot đến part.
        /// </summary>
        public void SpawnRope(YarnPart part, Slot slot, YarnColor color = YarnColor.None)
        {
            // Lấy từ pool
            var rope = PoolManager.Instance.Get<Rope>("rope");

            // Tiếng dây khi len bay vào slot
            AudioService.Instance?.PlayRope();

            // Chọn material theo màu yarn
            YarnColor c = (color == YarnColor.None) ? part.CurrentColor : color;
            Material mat = YarnColorManager.Instance.GetSlotStringMaterial(c);

            // Khởi tạo và lưu vào danh sách
            rope.Initialize(
                part,
                slot,
                mat,
                orthoCam,
                perspectiveCam,
                ropeBoardPlane,
                sagAmount,
                swayAmplitude,
                swayFrequency,
                retractDuration
            );
            ropes.Add(rope);
        }

        /// <summary>
        /// Gọi khi Rope tự Despawn() hoặc khi muốn clear
        /// </summary>
        public void ReleaseRope(Rope rope)
        {
            PoolManager.Instance.Release("rope", rope);
            ropes.Remove(rope);
        }

        /// <summary>
        /// Sinh mới một sợi dây RopeBoard giữa hai điểm bất kỳ.
        /// Tính toán vật liệu từ YarnColor truyền vào.
        /// </summary>
        /// <param name="start">Transform điểm đầu dây</param>
        /// <param name="end">Transform điểm cuối dây</param>
        /// <param name="color">Màu dây YarnColor (nếu None sẽ lấy CurrentColor của start nếu là YarnPart)</param>
        public void SpawnRopeBoard(Transform start, Transform end, YarnColor color = YarnColor.None, float ropeTime = 0f)
        {
            // Lấy từ pool
            var ropeBoard = PoolManager.Instance.Get<RopeBoard>("ropeBoard");

            Material mat = YarnColorManager.Instance.GetSlotStringMaterial(color);

            // Khởi tạo RopeBoard với các tham số động học, truyền vào ropeBoardPlane
            ropeBoard.Initialize(
                start,
                end,
                mat,
                orthoCam,
                perspectiveCam,
                ropeBoardPlane, // Sử dụng plane dọc
                sagAmount,
                swayAmplitude,
                swayFrequency,
                retractDuration,
                ropeTime
            );
            ropeBoards.Add(ropeBoard);
        }

        /// <summary>
        /// Hủy một RopeBoard (trả về pool và xóa khỏi danh sách).
        /// </summary>
        /// <param name="ropeBoard">Đối tượng RopeBoard cần hủy</param>
        public void ReleaseRopeBoard(RopeBoard ropeBoard)
        {
            PoolManager.Instance.Release("ropeBoard", ropeBoard);
            ropeBoards.Remove(ropeBoard);
        }

        /// <summary>
        /// Khi thắng: tăng tốc dây đang bay (nhất là dây in box -> ảnh) để không phải
        /// đợi dây bay xong mới thấy WinPopup.
        /// </summary>
        public void RushRopes(float multiplier = 8f)
        {
            RopeBoard.SpeedMultiplier = Mathf.Max(1f, multiplier);
        }

        /// <summary>Trả tốc độ dây về mặc định (gọi khi load level mới).</summary>
        public void ResetRopeSpeed()
        {
            RopeBoard.SpeedMultiplier = 1f;
        }

        /// <summary>
        /// Xóa sạch mọi rope đang tồn tại.
        /// </summary>
        public void ClearAllRopes()
        {
            // Kill trước để coroutine/animation của rope không ghi đè trạng thái sau khi đã trả pool.
            foreach (var r in ropes)
                if (r != null)
                {
                    r.DOKill();
                    PoolManager.Instance.Release("rope", r);
                }
            ropes.Clear();

            foreach (var rb in ropeBoards)
                if (rb != null)
                {
                    rb.DOKill();
                    PoolManager.Instance.Release("ropeBoard", rb);
                }
            ropeBoards.Clear();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // luôn cập nhật plane mới khi chỉnh planeY trong Inspector
            ropePlane = new Plane(Vector3.up, Vector3.up * planeY);
            ropeBoardPlane = new Plane(Vector3.forward, Vector3.forward * planeY);
        }

        private void OnDrawGizmos()
        {
            // 1) Vẽ mặt phẳng y = planeY dưới dạng wire-cube mỏng (ngang)
            Gizmos.color = Color.green;
            Vector3 center = new Vector3(0, planeY, 0);
            Vector3 size = new Vector3(planeSize, 0.01f, planeSize);
            Gizmos.DrawWireCube(center, size);

            // 2) Vẽ mặt phẳng z = planeY dưới dạng wire-cube mỏng (dọc)
            Gizmos.color = Color.cyan;
            Vector3 centerZ = new Vector3(0, 0, planeY);
            Vector3 sizeZ = new Vector3(planeSize, planeSize, 0.01f);
            Gizmos.DrawWireCube(centerZ, sizeZ);
        }
#endif
    }
}
