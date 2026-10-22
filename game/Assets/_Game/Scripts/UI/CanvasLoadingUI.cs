namespace _Game.Scripts.UI
{
    using _Game.Scripts.Core;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Thanh tiến trình màn Entry.
    ///
    /// Bản gốc: prefab `_KIT/Template/Prefabs/CanvasLoading.prefab` (khung
    /// Frame_Progress_1, fill Frame_Progress_2, nền BGR_Loading) + `GameEntry.UpdadeProgress`
    /// chạy giả `totalTimeLoad = 10` giây rồi chuyển scene, hiện "%" theo fill.
    ///
    /// Target: giữ nguyên prefab gốc (đã đổi font BALOO → GoogleSans, bỏ I2 Localize)
    /// và chạy fill 0 → 1 trong `totalTime` giây, xong thì gọi
    /// `EntryFlow.LoadTargetScene()` để vào Home/Gameplay.
    /// </summary>
    public sealed class CanvasLoadingUI : MonoBehaviour
    {
        [Tooltip("Image kiểu Filled (Frame_Progress_2) — fillAmount chạy 0 → 1.")]
        [SerializeField] private Image fill;
        [Tooltip("Text hiện phần trăm, ví dụ '42%'.")]
        [SerializeField] private TMP_Text percentText;
        [Tooltip("Object bọc cả cụm loading (mặc định là chính object này).")]
        [SerializeField] private GameObject loadingContainer;

        [Header("Cấu hình")]
        [Tooltip("Thời gian chạy đầy thanh (giây). Bản gốc là 10.")]
        [SerializeField] private float totalTime = 3f;
        [Tooltip("Bật: tự chạy rồi tự chuyển scene. Tắt để tự gọi LoadTargetScene().")]
        [SerializeField] private bool autoLoad = true;

        private float _elapsed;

        private void Awake()
        {
            if (loadingContainer == null) loadingContainer = gameObject;
        }

        private void OnEnable()
        {
            _elapsed = 0f;
            Apply(0f);
        }

        private void Update()
        {
            if (!autoLoad) return;

            _elapsed += Time.unscaledDeltaTime;
            var percent = totalTime > 0f ? Mathf.Clamp01(_elapsed / totalTime) : 1f;
            Apply(percent);

            if (percent >= 1f)
            {
                autoLoad = false;
                Finish();
            }
        }

        /// <summary>Kết thúc loading: nhường EntryFlow quyết định scene đích.</summary>
        public void Finish()
        {
            var flow = EntryFlow.Instance;
            if (flow != null) flow.LoadTargetScene();
            else Debug.LogWarning("[CanvasLoadingUI] Không có EntryFlow — không chuyển scene được.");
        }

        public void Show() => loadingContainer?.SetActive(true);
        public void Hide() => loadingContainer?.SetActive(false);

        private void Apply(float percent)
        {
            if (fill != null) fill.fillAmount = percent;
            if (percentText != null) percentText.text = Mathf.RoundToInt(percent * 100f) + "%";
        }
    }
}
