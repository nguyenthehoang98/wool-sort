namespace _Game.Scripts.Meta
{
    using _Game.Scripts.Core;
    using _Game.Scripts.Gameplay;
    using TMPro;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>
    /// Popup cài đặt: hiệu ứng / nhạc nền / rung + phiên bản.
    /// Port lại từ SettingHomePopup của dự án gốc nhưng viết lại cho target
    /// (bản gốc dùng FrameBase + KitSDK + LFramework.Vibration đều không có ở target).
    /// </summary>
    public class SettingsPopup : MonoBehaviour
    {
        public static SettingsPopup Instance { get; private set; }

        [Header("Nút bật/tắt")]
        [SerializeField] private Button btnSound;
        [SerializeField] private Button btnMusic;
        [SerializeField] private Button btnVibration;
        [SerializeField] private Button btnClose;

        [Header("Nhãn trạng thái")]
        [SerializeField] private TMP_Text lblSound;
        [SerializeField] private TMP_Text lblMusic;
        [SerializeField] private TMP_Text lblVibration;
        [SerializeField] private TMP_Text txtVersion;

        [Header("Ẩn/hiện")]
        [SerializeField] private GameObject shade;
        [SerializeField] private GameObject card;

        // Chỉ prefab SettingPopupGamePlay (Gameplay) mới gán 2 nút này.
        [Header("Chỉ dùng trong Gameplay")]
        [SerializeField] private Button btnReplay;
        [SerializeField] private Button btnExitHome;

        [Header("Sprite nút bật/tắt")]
        [SerializeField] private Sprite toggleOn;
        [SerializeField] private Sprite toggleOff;

        private void Awake()
        {
            Instance = this;

            if (btnSound != null) btnSound.onClick.AddListener(OnToggleSound);
            if (btnMusic != null) btnMusic.onClick.AddListener(OnToggleMusic);
            if (btnVibration != null) btnVibration.onClick.AddListener(OnToggleVibration);
            if (btnClose != null) btnClose.onClick.AddListener(Hide);

            if (btnReplay != null) btnReplay.onClick.AddListener(OnReplay);
            if (btnExitHome != null) btnExitHome.onClick.AddListener(OnExitHome);

            SetVisible(false);
        }

        private void Start()
        {
            // EconomyService có thể tạo/load save sau Awake -> nạp lại cài đặt cho chắc.
            SettingsService.Load();
            Refresh();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Lấy popup, tự bật GameObject gốc nếu đang tắt (đặt tắt trong scene cho gọn Scene View).</summary>
        public static SettingsPopup Acquire()
        {
            if (Instance == null)
            {
                Instance = FindFirstObjectByType<SettingsPopup>(FindObjectsInactive.Include);
            }
            if (Instance == null) return null;

            if (!Instance.gameObject.activeSelf) Instance.gameObject.SetActive(true);
            return Instance;
        }

        public static void Open()
        {
            var popup = Acquire();
            if (popup == null)
            {
                Debug.LogWarning("[SettingsPopup] Không tìm thấy SettingsPopup trong scene.");
                return;
            }
            popup.Show();
        }

        public void Show()
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            Refresh();
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private void OnToggleSound()
        {
            SettingsService.ToggleSound();
            Refresh();
        }

        private void OnToggleMusic()
        {
            SettingsService.ToggleMusic();
            Refresh();
        }

        private void OnToggleVibration()
        {
            SettingsService.ToggleVibration();
            Refresh();
        }

        /// <summary>Replay: đóng popup và nạp lại màn đang chơi (giống nút Retry của LosePopup).</summary>
        private void OnReplay()
        {
            Hide();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadLevel();
            }
            else
            {
                Debug.LogWarning("[SettingsPopup] Không tìm thấy GameManager để Replay.");
            }
        }

        /// <summary>Exit Home: đóng popup và quay về scene Home.</summary>
        private void OnExitHome()
        {
            Hide();

            if (EntryFlow.Instance != null)
            {
                EntryFlow.Instance.GoHome();
            }
            else
            {
                // Chơi trực tiếp scene Gameplay (không qua Entry) thì chưa có EntryFlow.
                SceneManager.LoadScene("Home");
            }
        }

        private void Refresh()
        {
            if (txtVersion != null) txtVersion.text = "v" + Application.version;

            if (lblSound != null) lblSound.text = SettingsService.Sound ? "BẬT" : "TẮT";
            if (lblMusic != null) lblMusic.text = SettingsService.Music ? "BẬT" : "TẮT";
            if (lblVibration != null) lblVibration.text = SettingsService.Vibration ? "BẬT" : "TẮT";

            SetToggleImage(btnSound, SettingsService.Sound);
            SetToggleImage(btnMusic, SettingsService.Music);
            SetToggleImage(btnVibration, SettingsService.Vibration);
        }

        private void SetToggleImage(Button btn, bool on)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img == null) return;
            if (on && toggleOn != null) img.sprite = toggleOn;
            else if (!on && toggleOff != null) img.sprite = toggleOff;
            img.color = Color.white;
        }

        private void SetVisible(bool visible)
        {
            if (shade != null) shade.SetActive(visible);
            if (card != null) card.SetActive(visible);
        }
    }
}
