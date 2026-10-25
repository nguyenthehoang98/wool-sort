namespace _Game.Scripts.Meta
{
    using System;
    using _Game.Scripts.Core;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public sealed class HomeUI : MonoBehaviour
    {
        public static HomeUI Instance { get; private set; }

        [Header("Tab bar (đặt dưới màn hình)")]
        [Tooltip("MenuBar gốc (3 nút Home / Shop / Ranking)")]
        [SerializeField] private MenuBarManager menuBar;

        [Header("Trang chủ (chỉ hiện khi tab Home được chọn)")]
        [SerializeField] private GameObject homePage;
        [SerializeField] private Button btnStart;
        [SerializeField] private TMP_Text txtLevel; // ví dụ: "LEVEL 3"

        [Header("Trang shop (chỉ hiện khi tab Shop được chọn)")]
        [SerializeField] private GameObject shopPage;
        [SerializeField] private ShopUI shopUI;

        [Header("Coming soon")]
        [SerializeField] private GameObject comingSoonPage;

        [Header("Coin HUD (trên cùng)")]
        [SerializeField] private TMP_Text txtCoin;

        [Header("Nút Settings (ẩn khi ở tab Shop)")]
        [Tooltip("Để trống sẽ tự tìm SettingsButton trong scene.")]
        [SerializeField] private SettingsButton settingsButton;

        private EconomyService _economy;
        private ShopService _shop;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // Không DontDestroyOnLoad: HomeUI gắn với scene Home, giữ lại sẽ để lại
            // reference tới UI của scene cũ (đã bị destroy) và làm menu chết.

            // Khi mở scene Home trực tiếp để test thì chưa qua Entry -> tự tạo
            // service để shop/coin vẫn hoạt động. Trong luồng thật (Entry -> Home)
            // các service đã tồn tại nên nhánh này không chạy.
            // `ServiceBootstrap` đã tạo sẵn từ `BeforeSceneLoad`; gọi lại là idempotent.
            ServiceBootstrap.EnsureEconomy();
            ServiceBootstrap.EnsureShop();

            if (settingsButton == null)
                settingsButton = FindObjectOfType<SettingsButton>(true);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            _economy = EconomyService.Instance;
            _shop = ShopService.Instance;
            if (_economy == null) Debug.LogError("[HomeUI] EconomyService thiếu.");
            if (_shop == null) Debug.LogError("[HomeUI] ShopService thiếu.");

            WireTabs();
            WireStart();

            ShowTab(HomeTab.Home);
            if (menuBar != null) menuBar.Focus(MenuBarOption.Home);
            RefreshCoin();
            RefreshLevel();

            if (_economy != null)
            {
                _economy.OnCoinChanged += _ => RefreshCoin();
            }
        }

        private void WireTabs()
        {
            if (menuBar == null)
            {
                Debug.LogWarning("[HomeUI] MenuBar chưa được gán.");
                return;
            }

            // MenuBar tự lo focus/unfocus; HomeUI chỉ lo bật/tắt trang.
            menuBar.OnClickOpenHome += () => ShowTab(HomeTab.Home);
            menuBar.OnClickOpenShop += () => ShowTab(HomeTab.Shop);
            menuBar.OnClickOpenRanking += () => ShowTab(HomeTab.ComingSoon);
        }

        private void WireStart()
        {
            if (btnStart != null) btnStart.onClick.AddListener(() =>
            {
                var flow = _Game.Scripts.Core.EntryFlow.Instance;
                flow?.StartCurrentLevel();
            });
        }

        private void ShowTab(HomeTab tab)
        {
            if (homePage != null) homePage.SetActive(tab == HomeTab.Home);
            if (shopPage != null) shopPage.SetActive(tab == HomeTab.Shop);
            if (comingSoonPage != null) comingSoonPage.SetActive(tab == HomeTab.ComingSoon);
            // Ẩn nút Settings khi ở tab Shop, các tab còn lại bật lại.
            if (settingsButton != null) settingsButton.gameObject.SetActive(tab != HomeTab.Shop);
            // ShopUI tự Refresh() trong OnEnable khi ShopPage được bật -> không
            // cần gọi thêm ở đây (gọi thêm sẽ dựng lại item lần nữa).
        }

        private void RefreshCoin()
        {
            if (txtCoin == null || _economy == null) return;
            txtCoin.text = _economy.GetCoin().ToString();
        }

        private void RefreshLevel()
        {
            if (txtCoin == null && txtLevel == null) return;
            var level = _economy != null ? _economy.GetLevel() : 1;
            if (txtLevel != null) txtLevel.text = $"LEVEL {level}";
        }

        public void OnCoinChanged(int amount) => RefreshCoin();
    }

    public enum HomeTab
    {
        Home,
        Shop,
        ComingSoon
    }
}
