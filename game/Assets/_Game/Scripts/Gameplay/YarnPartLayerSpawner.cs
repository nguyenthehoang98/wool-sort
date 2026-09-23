using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Gameplay;
using DG.Tweening;
using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    // Về cơ chế spawn, khởi tạo ban đầu tất cả các lớp 1 và 2
    // nếu click lớp 1 thì sinh lớp 3, click lớp 2 thì sinh lớp 4 
    // luôn đảm bảo là trên level có 2 lớp ngoài cùng nhất của mỗi part
    public class YarnPartLayerSpawner : MonoBehaviour
    {
        public static YarnPartLayerSpawner Instance { get; private set; }

        private float initialInnerScale;
        private float growScale;
        private float growDuration;

        private LevelYarnInfoHolder levelInfo;

        // Pool dưới dạng List để chọn ngẫu nhiên rồi loại màu đã dùng
        private List<YarnColor> _randomColorPool;

        public IReadOnlyList<YarnColor> RandomColorPool => _randomColorPool;

        private void Awake()
        {
            Instance = this;
            levelInfo = GetComponent<LevelYarnInfoHolder>();

            initialInnerScale = GameManager.Instance.GameConfig.initialInnerScale;
            growScale = GameManager.Instance.GameConfig.growScale;
            growDuration = GameManager.Instance.GameConfig.growDuration;
        }

        public void Init()
        {
            // 2) Dùng allBaseYarnParts (layerIndex = 1) để spawn lần đầu (layerIndex = 2)
            var roots = levelInfo.allBaseYarnParts;
            if (roots == null) return;

            foreach (var root in roots)
            {
                // Nếu có inner (tổng >= 1) → spawn layerIndex=2
                if (root.totalInnerCount >= 1)
                {
                    root.childPart = SpawnInnerLayer(root, 2);
                }
            }
        }

        float ComputeSpawnScale(YarnPart root, int layerIndex)
            => ComputeGrowScale(root, layerIndex) * initialInnerScale;

        float ComputeGrowScale(YarnPart root, int layerIndex)
            => Mathf.Pow(growScale, layerIndex - 1);

        /// <summary>
        /// Instantiate một layer mới, sau đó Initialize với đúng màu lấy từ ChooseInnerColor.
        /// </summary>
        private YarnPart SpawnInnerLayer(YarnPart source, int layerIndex, YarnPartInfo usedInfo = null)
        {
            Transform
                commonParent = source.ancestor.transform.parent; // nếu vẫn bug null thì có thể lấy child của level
            var go = Instantiate(source.gameObject, commonParent);

            // Xóa các child không cần thiết (nếu có)
            foreach (Transform c in go.transform)
                Destroy(c.gameObject);

            var part = go.GetComponent<YarnPart>();
            part.ClearDeco(); // các inner layer ko có decor

            // 1) Tính spawnScale dựa trên flag special hoặc spawner‐wide
            float spawnScale;

            spawnScale = ComputeSpawnScale(source.ancestor, layerIndex);

            Debug.Log("spawn scale = " + spawnScale);


            // 2) Lấy màu đã định sẵn trong dữ liệu YarnPartInfo ──
            var childInfo = usedInfo; // nếu có dữ liệu thì lấy luôn (trg hợp của magic box), còn không thì lấy từ cây dữ liệu
            if (childInfo == null)
            {
                if (!GameManager.Instance.TryGetInfoByPart(source, out var parentInfo))
                {
                    Debug.LogError($"Không tìm thấy YarnPartInfo cho part {source.name}");
                    return null; // hoặc xử lý phù hợp
                }

                childInfo = parentInfo.Children.First(x =>
                    x.LayerIndex ==
                    layerIndex); // Bỏ đoạn check !GameManager.Instance.RuntimeLink.ContainsValue(x) đang gây lỗi nên không tìm được
            }
            YarnColor chosenColor = childInfo.Color;

            // transform scale ngay
            part.Initialize(source, layerIndex, spawnScale, chosenColor);
            
            // 4) Link và raise event để GameManager cập nhật runtimeLink
            GameManager.Instance.LinkPartInfo(part, childInfo);

            // ban đầu ẩn object
            part.gameObject.SetActive(false);

            return part;
        }
        
        /// <summary>
        /// Spawn trực tiếp một YarnPart theo info (bất kể layer sâu thế nào).
        /// </summary>
        public YarnPart SpawnForInfo(YarnPartInfo info)
        {
            // 1) Lấy root-info (layerIndex==1)
            var rootInfo = info;
            while (rootInfo.Parent != null)
                rootInfo = rootInfo.Parent;
            
            // debug đảm bảo tìm đúng root phải có layer index = 1
            Debug.Log("Spawn for info - root info có layer = " + rootInfo.LayerIndex);

            // 2) Tìm root-GO
            if (!GameManager.Instance.TryGetPartByInfo(rootInfo, out var rootPart))
            {
                Debug.LogError($"Không tìm thấy YarnPart cho info {rootInfo}");
                return null;
            }

            // 3) Gọi SpawnInnerLayer với source = rootGO
            return SpawnInnerLayer(rootPart, info.LayerIndex, info);
        }

        /// <summary>
        /// Khi user tap vào lớp i, spawn lớp i+2 tiếp theo (layerIndex + 1) nếu ≤ totalInnerCount + 1
        /// Chú ý xử lý trường hợp là lớp ở giữa đã bị magic box hút lên, vd mất lớp số 3 thì khi click lớp 1 phải sinh lớp 4 thay vì lớp 3
        /// </summary>
        // public void SpawnInnerOnClick(YarnPart clicked)
        // {
        //     var nextRoot = clicked.childPart;
        //     if (nextRoot != null)
        //     {
        //         // hiện lớp i + 1
        //         nextRoot.gameObject.SetActive(true);
        //         int spawnIdx = nextRoot.layerIndex + 1; // tính next qua next root vì có thể giữa clicked và nextroot có cách nhau
        //         if (spawnIdx <= clicked.totalInnerCount + 1)
        //         {
        //             nextRoot.childPart = SpawnInnerLayer(nextRoot, spawnIdx);
        //         }
        //     }
        // }

        /// <summary>
        /// Sinh lớp bên trong, dùng cho cả khi click và khi hút bằng magic box
        /// khi 1 lớp i bị tác động thì sẽ Spawn layer i+1 nếu chưa có, Active layer i+1, Warm-up layer i+2 (ẩn)
        /// </summary>
        public void SpawnInnerOnClick(YarnPart clicked)
        {
            // 0) Lấy info gắn với part
            if (!GameManager.Instance.TryGetInfoByPart(clicked, out var info))
                return;

            // 1) Tìm info của lớp i+1 còn OnBoard
            var nextInfo = info.Children
                .Where(c => c.State == PartState.OnBoard)
                .OrderBy(c => c.LayerIndex)
                .FirstOrDefault();
            if (nextInfo == null) 
                return;

            // 2) Spawn hoặc reuse layer i+1
            YarnPart nextPart;
            if (!GameManager.Instance.TryGetPartByInfo(nextInfo, out nextPart))
                nextPart = SpawnInnerLayer(clicked, nextInfo.LayerIndex);
            clicked.childPart = nextPart;

            // 3) Active layer i+1 ngay
            nextPart.gameObject.SetActive(true);

            // 4) Nếu đã warm-up layer i+2 rồi thì dừng
            if (nextPart.childPart != null)
                return;

            // 5) Tìm info layer i+2 còn OnBoard
            var next2Info = nextInfo.Children
                .Where(c => c.State == PartState.OnBoard)
                .OrderBy(c => c.LayerIndex)
                .FirstOrDefault();
            if (next2Info == null) 
                return;

            // 6) Spawn hoặc reuse layer i+2, gán làm childPart, keep hidden
            YarnPart next2Part;
            if (!GameManager.Instance.TryGetPartByInfo(next2Info, out next2Part))
                next2Part = SpawnInnerLayer(nextPart, next2Info.LayerIndex);
            nextPart.childPart = next2Part;
            next2Part.gameObject.SetActive(false);
        }


        /// <summary>
        /// Sau khi layer i “dissolve” xong, animate grow cho layer i+1.
        /// </summary>
        public void HandleLayerDissolved(YarnPart dissolved)
        {
            var inner = dissolved.childPart;
            if (inner == null) return;
            
            // nếu inner đang pop (tức là user click rồi) thì bỏ qua grow để complete pop và dissolve không lỗi
            if (inner.IsPopping) return;

            inner.gameObject.SetActive(true);
            var ancestor = inner.ancestor;
            float target = ComputeGrowScale(ancestor, inner.layerIndex);
            // inner.transform.DOKill(); // bỏ đi vì khả năng cao gây bug, kill tween của pop effect nên làm yarn part không dissolve được
            inner.transform
                .DOScale(ancestor.transform.localScale * target, growDuration)
                .SetEase(Ease.OutBack, 3);


        }
    }
}
