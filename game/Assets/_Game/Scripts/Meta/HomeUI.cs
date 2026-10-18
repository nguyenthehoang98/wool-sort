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
        [SerializeField] private RectTransform tabBar;
        [SerializeField] private Button btnTabHome;
        [SerializeField] private Button btnTabShop;
        [SerializeField] private Button btnTabComingSoon;

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
            DontDestroyOnLoad(gameObject);
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
            RefreshCoin();
            RefreshLevel();

            if (_economy != null)
            {
                _economy.OnCoinChanged += _ => RefreshCoin();
            }
        }

        private void WireTabs()
        {
            if (btnTabHome != null) btnTabHome.onClick.AddListener(() => ShowTab(HomeTab.Home));
            if (btnTabShop != null) btnTabShop.onClick.AddListener(() => ShowTab(HomeTab.Shop));
            if (btnTabComingSoon != null) btnTabComingSoon.onClick.AddListener(() => ShowTab(HomeTab.ComingSoon));
        }

        private void WireStart()
        {
            if (btnStart != null)
                btnStart.onClick.AddListener(() =>
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

            if (btnTabHome != null) btnTabHome.interactable = tab != HomeTab.Home;
            if (btnTabShop != null) btnTabShop.interactable = tab != HomeTab.Shop;
            if (btnTabComingSoon != null) btnTabComingSoon.interactable = tab != HomeTab.ComingSoon;

            if (tab == HomeTab.Shop && shopUI != null)
                shopUI.Refresh();
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
