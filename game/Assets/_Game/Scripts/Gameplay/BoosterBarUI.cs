// BoosterBarUI.cs
// Dựng lại hàng nút booster tương đương BoosterBtn.prefab gốc bằng code
// (không copy prefab để tránh lỗi). Số đo lấy từ prefab gốc:
// root 200x200, Frame 194x100, ImgIcon 102x105 @ y=41.5,
// Notice 56x56 @ (88.6,-24.6), IconPlus 55x56 @ (89,-23).
// Sprite kéo từ Assets/_Game/BoosterUI_Picked/ vào Inspector;
// để trống vẫn chạy bằng màu trơn.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Gameplay
{
    public sealed class BoosterBarUI : MonoBehaviour
    {
        [Header("Sprites — kéo từ BoosterUI_Picked")]
        public Sprite frameNormal;
        public Sprite frameLock;
        public Sprite iconLock;
        public Sprite iconPlus;
        public Sprite iconNoti;

        [Header("Icon thường theo loại")]
        public Sprite iconAddSlot;
        public Sprite iconClean;
        public Sprite iconMagic;

        [Header("Icon xám (lock) theo loại")]
        public Sprite iconAddSlotGray;
        public Sprite iconCleanGray;
        public Sprite iconMagicGray;

        [Header("Vị trí (chỉ dùng khi autoBuild bật)")]
        public Vector2 containerAnchoredPos = new Vector2(0f, 137f);
        public float buttonSpacing = 263f;
        public float buttonYOffset = -10f;

        [Header("Chế độ")]
        [Tooltip("Mặc định TẮT: nút booster nằm sẵn trong scene Gameplay (BoosterContainer), " +
            "component này chỉ gọi Setup/refresh. Bật chỉ khi muốn sinh nút bằng code.")]
        public bool autoBuild = false;

        private void Awake()
        {
            ResolveSprites();
            EnsureSceneButtons();
        }

        /// <summary>Gọi từ Inspector (chuột phải) hoặc code để refresh 3 nút trong scene.</summary>
        public void Refresh() => RefreshSceneButtons();

        [ContextMenu("Refresh Buttons")]
        private void CtxRefresh() => RefreshSceneButtons();

        [ContextMenu("Wire Sprites From Resources")]
        private void CtxWireSprites()
        {
            frameNormal = null; frameLock = null; iconLock = null;
            iconPlus = null; iconNoti = null;
            iconAddSlot = null; iconClean = null; iconMagic = null;
            iconAddSlotGray = null; iconCleanGray = null; iconMagicGray = null;
            ResolveSprites();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        [ContextMenu("Force Rebuild Buttons (autoBuild)")]
        private void CtxRebuild()
        {
            ResolveSprites();
            BuildIfMissing(force: true);
        }

        // Tự chữa: nếu container/thiếu nút/thiếu ref thì dựng lại bằng code
        // (kể cả khi autoBuild=false) để Play là thấy UI ngay, không cần kéo tay.
        private void EnsureSceneButtons()
        {
            if (autoBuild)
            {
                BuildIfMissing();
                return;
            }
            var parent = FindBoosterParent();
            var container = parent != null ? parent.Find("BoosterContainer") : null;
            if (container == null)
            {
                Debug.LogWarning("[BoosterBarUI] Thiếu BoosterContainer -> tự dựng bằng code.", this);
                BuildIfMissing();
                return;
            }
            var items = container.GetComponentsInChildren<BoosterItem>(true);
            if (items.Length < 3)
            {
                Debug.LogWarning("[BoosterBarUI] BoosterContainer thiếu nút -> tự dựng bù.", this);
                BuildIfMissing();
                return;
            }
            RefreshSceneButtons();
        }

        // Nút nằm sẵn trong scene: gọi Setup để cập nhật lock/count theo level hiện tại.
        private void RefreshSceneButtons()
        {
            var parent = FindBoosterParent();
            if (parent == null) return;
            var container = parent.Find("BoosterContainer");
            if (container == null)
            {
                Debug.LogWarning("[BoosterBarUI] Không thấy BoosterContainer trong scene. " +
                    "Nút booster phải được đặt sẵn trong scene Gameplay (không sinh từ code).", this);
                return;
            }
            foreach (var item in container.GetComponentsInChildren<BoosterItem>(true))
                item.Setup();
        }

        // Field nào chưa kéo sprite trong Inspector thì tự load bản copy
        // trong Resources/BoosterUI/ (bản Picked để bạn edit, bản Resources để chạy).
        private void ResolveSprites()
        {
            frameNormal = frameNormal ? frameNormal : Load("Button_Boosters");
            frameLock = frameLock ? frameLock : Load("Button_Boosters_Gray");
            iconLock = iconLock ? iconLock : Load("Icon_Lock_2");
            iconPlus = iconPlus ? iconPlus : Load("Icon_Plus");
            iconNoti = iconNoti ? iconNoti : Load("Icon_Noti");
            iconAddSlot = iconAddSlot ? iconAddSlot : Load("Icon_Slot_3");
            iconAddSlotGray = iconAddSlotGray ? iconAddSlotGray : Load("Icon_Slot_4");
            iconClean = iconClean ? iconClean : Load("Icon_Broom_2");
            iconCleanGray = iconCleanGray ? iconCleanGray : Load("Icon_Broom_3");
            iconMagic = iconMagic ? iconMagic : Load("Icon_Spool_2");
            iconMagicGray = iconMagicGray ? iconMagicGray : Load("Icon_Spool_3");

            if (frameNormal == null || iconAddSlot == null || iconClean == null || iconMagic == null)
                Debug.LogWarning("[BoosterBarUI] Chưa load được sprite (Inspector trống + Resources/BoosterUI thiếu). " +
                    "Nút vẫn hiện bằng màu fallback. Kéo sprite từ BoosterUI_Picked/ vào field để giống gốc.", this);
        }

        private static Sprite Load(string name)
        {
            return Resources.Load<Sprite>("BoosterUI/" + name);
        }

        private void BuildIfMissing(bool force = false)
        {
            var parent = FindBoosterParent();
            if (parent == null) return;

            var container = parent.Find("BoosterContainer");
            if (!force && container != null && container.childCount >= 3)
            {
                foreach (var item in container.GetComponentsInChildren<BoosterItem>(true))
                    item.Setup();
                return;
            }

            if (force && container != null)
            {
                // Xóa nút cũ dựng sai/thiếu ref rồi dựng lại sạch.
                for (int i = container.childCount - 1; i >= 0; i--)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        DestroyImmediate(container.GetChild(i).gameObject);
                    else
#endif
                        Destroy(container.GetChild(i).gameObject);
                }
            }

            if (container == null)
            {
                var go = new GameObject("BoosterContainer", typeof(RectTransform));
                container = go.transform;
                container.SetParent(parent, false);
                var rt = (RectTransform)container;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
                rt.anchoredPosition = containerAnchoredPos;
                rt.sizeDelta = new Vector2(100f, 100f);
                rt.pivot = new Vector2(0.5f, 0.5f);
            }

            CreateButton(container, BoosterType.AddSlot, new Vector2(-buttonSpacing, buttonYOffset), 180f);
            CreateButton(container, BoosterType.MagicBox, new Vector2(0f, buttonYOffset), 200f);
            CreateButton(container, BoosterType.Clean, new Vector2(buttonSpacing, buttonYOffset), 200f);
        }

        private Transform FindBoosterParent()
        {
            var canvas = GameObject.Find("Canvas_1 (Base)");
            if (canvas != null)
            {
                var safe = canvas.transform.Find("SafeArea");
                if (safe != null) return safe;
                return canvas.transform;
            }
            var anyCanvas = FindObjectOfType<Canvas>();
            return anyCanvas != null ? anyCanvas.transform : transform;
        }

        private void CreateButton(Transform container, BoosterType type, Vector2 anchored, float rootSize)
        {
            var root = new GameObject($"BoosterBtn_{(int)type + 1}", typeof(RectTransform));
            root.layer = 5;
            var rootRt = (RectTransform)root.transform;
            rootRt.SetParent(container, false);
            rootRt.anchorMin = rootRt.anchorMax = new Vector2(0.5f, 0.5f);
            rootRt.anchoredPosition = anchored;
            rootRt.sizeDelta = new Vector2(rootSize, rootSize);

            var button = root.AddComponent<Button>();
            var bg = root.AddComponent<Image>();
            bg.sprite = null;
            // Nút bấm phải nhìn thấy ngay cả khi chưa gán asset: nền đặc theo loại.
            bg.color = TypeColor(type);

            // Nhánh thường: Frame + ImgIcon
            var objNormal = BuildFrame(root.transform, "Frame",
                new Vector2(194f, 100f), Vector2.zero, frameNormal, new Color(0.45f, 0.31f, 0.18f, 1f));
            var iconNormal = BuildIcon(objNormal.transform, "ImgIcon",
                new Vector2(102f, 105f), new Vector2(0f, 41.5f),
                NormalIconFor(type), TypeColor(type));

            // Nhánh khóa: FrameLock + ImgLock + icon xám
            var objLock = BuildFrame(root.transform, "FrameLock",
                new Vector2(194f, 100f), Vector2.zero, frameLock, new Color(0.5f, 0.5f, 0.52f, 1f));
            BuildIcon(objLock.transform, "ImgLock",
                new Vector2(86f, 106f), new Vector2(0f, 42f), iconLock, Color.white);
            var lockIcon = BuildIcon(objLock.transform, "ImgIcon",
                new Vector2(102f, 105f), new Vector2(0f, 41.5f),
                GrayIconFor(type), Color.white);
            lockIcon.gameObject.SetActive(false);
            objLock.SetActive(false);

            // Badge đếm
            var objNoti = new GameObject("Notice", typeof(RectTransform));
            var notiRt = SetupRect(objNoti, root.transform, new Vector2(56f, 56f), new Vector2(88.6f, -24.6f));
            var notiImg = objNoti.AddComponent<Image>();
            notiImg.sprite = iconNoti;
            notiImg.color = iconNoti == null ? new Color(0.95f, 0.25f, 0.25f, 1f) : Color.white;
            var txtGo = new GameObject("TxtNum", typeof(RectTransform));
            SetupRect(txtGo, objNoti.transform, new Vector2(44f, 86f), Vector2.zero);
            var txt = txtGo.AddComponent<TextMeshProUGUI>();
            txt.font = TMP_Settings.defaultFontAsset;
            txt.fontSize = 40f;
            txt.fontSizeMin = 18f;
            txt.fontSizeMax = 40f;
            txt.enableAutoSizing = true;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
            txt.text = "0";
            objNoti.SetActive(false);

            // Dấu cộng (hết lượt)
            var objPlus = BuildIcon(root.transform, "IconPlus",
                new Vector2(55f, 56f), new Vector2(89f, -23f), iconPlus, Color.white);
            objPlus.gameObject.SetActive(false);

            var item = root.AddComponent<BoosterItem>();
            item.Bind(type, button, objNormal, objLock, objNoti, txt, objPlus);
            button.targetGraphic = bg;
        }

        private static GameObject BuildFrame(Transform parent, string name, Vector2 size, Vector2 pos, Sprite sprite, Color fallback)
        {
            var go = new GameObject(name, typeof(RectTransform));
            SetupRect(go, parent, size, pos);
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = sprite == null ? fallback : Color.white;
            img.preserveAspect = true;
            return go;
        }

        private static GameObject BuildIcon(Transform parent, string name, Vector2 size, Vector2 pos, Sprite sprite, Color fallback)
        {
            var go = new GameObject(name, typeof(RectTransform));
            SetupRect(go, parent, size, pos);
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = sprite == null ? fallback : Color.white;
            img.preserveAspect = true;
            return go;
        }

        private static RectTransform SetupRect(GameObject go, Transform parent, Vector2 size, Vector2 pos)
        {
            go.layer = 5;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
        }

        private Sprite NormalIconFor(BoosterType type)
        {
            switch (type)
            {
                case BoosterType.AddSlot: return iconAddSlot;
                case BoosterType.Clean: return iconClean;
                default: return iconMagic;
            }
        }

        private Sprite GrayIconFor(BoosterType type)
        {
            switch (type)
            {
                case BoosterType.AddSlot: return iconAddSlotGray;
                case BoosterType.Clean: return iconCleanGray;
                default: return iconMagicGray;
            }
        }

        // Màu fallback khi chưa có sprite: mỗi loại một màu để phân biệt nút.
        private static Color TypeColor(BoosterType type)
        {
            switch (type)
            {
                case BoosterType.AddSlot: return new Color(0.3f, 0.62f, 0.35f, 1f);
                case BoosterType.Clean: return new Color(0.9f, 0.55f, 0.2f, 1f);
                default: return new Color(0.6f, 0.4f, 0.85f, 1f);
            }
        }
    }
}
