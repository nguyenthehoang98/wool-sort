namespace _Game.Scripts.Meta
{
    using _Game.Scripts.Core;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Hàng nút chọn màn trên trang chủ (port từ LevelHome/ItemLevelHome của dự án gốc).
    ///
    /// Bấm một ô đã mở → chơi đúng màn đó (EconomyService.PlayLevel rồi load scene Gameplay).
    /// Màn chưa mở thì nút không bấm được.
    /// </summary>
    public class LevelList : MonoBehaviour
    {
        [Header("Nút màn (template để instantiate)")]
        [SerializeField] private LevelButton template;

        [Header("Chỗ chứa các nút")]
        [SerializeField] private RectTransform content;

        [Header("Sprite ô màn (pick từ dự án gốc)")]
        [SerializeField] private Sprite normalSprite;   // Button_Lv_1 - xanh
        [SerializeField] private Sprite currentSprite;  // Button_Lv_2 - be
        [SerializeField] private Sprite hardSprite;    // Button_Lv_3 - tím
        [SerializeField] private Sprite veryHardSprite;// Button_Lv_4 - đỏ

        [Header("Cấu hình")]
        [SerializeField] private int maxLevel = 7;
        [SerializeField] private float spacing = 150f;

        private void Awake()
        {
            Build();
        }

        private void Build()
        {
            if (template == null || content == null) return;

            // dọn nút cũ nếu build lại
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                var child = content.GetChild(i);
                if (child.gameObject == template.gameObject) continue;
                Destroy(child.gameObject);
            }

            int current = EconomyService.Instance != null ? EconomyService.Instance.GetLevel() : 1;

            for (int lv = 1; lv <= maxLevel; lv++)
            {
                var go = Instantiate(template.gameObject, content);
                go.SetActive(true);

                var btn = go.GetComponent<LevelButton>();
                if (btn == null) continue;

                bool unlocked = lv <= current;
                bool isCurrent = lv == current;
                btn.Bind(lv, unlocked, isCurrent, normalSprite, currentSprite, OnPickLevel);

                var rt = go.transform as RectTransform;
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2((lv - (maxLevel + 1) * 0.5f) * spacing, 0f);
                }
            }
        }

        private static void OnPickLevel(int level)
        {
            if (EconomyService.Instance != null) EconomyService.Instance.PlayLevel(level);
            SceneManager.LoadScene("Gameplay");
        }
    }
}
