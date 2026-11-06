using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _Game.Scripts.Core;
using _Game.Scripts.Gameplay;
using _Game.Scripts.Utils;


namespace _Game.Scripts.UI
{
    // Simple HUD to display current coin balance.
    // Attach this component to a UI GameObject that has a TextMeshProUGUI reference.
    // Dùng được ở Gameplay (CoinManager) và ở Home/Shop (EconomyService).
    public class CoinHUD : MonoBehaviour
    {
        public static CoinHUD Instance { get; private set; }

        [Header("UI References (drag in Inspector)")]
        public TextMeshProUGUI coinText;

        [Header("FX (drag in Inspector)")]
        public RectTransform iconCoin; // vị trí phát hiệu ứng get-coin + đích bay của coin

        private RectTransform _rectTransform;
        private Vector3 _oriAnchoredPos;
        private EconomyService _economy;

        private void Awake()
        {
            Instance = this;
            _rectTransform = transform as RectTransform;
            if (_rectTransform != null) _oriAnchoredPos = _rectTransform.anchoredPosition;

            BindEconomy();
            Refresh();
        }

        private void OnDestroy()
        {
            UnbindEconomy();
            if (Instance == this) Instance = null;
        }

        // Đích bay của đồng xu (SplashItem) và vị trí phát burst.
        public Transform IconTransform => iconCoin != null ? iconCoin : transform;

        // Nảy coin bar + phát burst get-coin tại icon (port từ CoinBar.ShowVFX gốc).
        public void ShowVFX()
        {
            if (_rectTransform != null)
            {
                _rectTransform.DOKill();
                _rectTransform.anchoredPosition = _oriAnchoredPos;
                _rectTransform.DOAnchorPos(_oriAnchoredPos + Vector3.up * 20f, 0.1f).OnComplete(() =>
                {
                    _rectTransform.DOAnchorPos(_oriAnchoredPos, 0.1f);
                });
            }

            if (EffectUIManager.Instance != null)
                EffectUIManager.Instance.ShowGetCoinEffect(IconTransform.position);
        }

        private void OnEnable()
        {
            CoinManager.OnBalanceChanged += UpdateCoinText;
            BindEconomy();
            Refresh();
        }

        private void OnDisable()
        {
            CoinManager.OnBalanceChanged -= UpdateCoinText;
            UnbindEconomy();
        }

        private void Start()
        {
            // CoinManager.Awake / EconomyService có thể chạy sau HUD này,
            // nên refresh + bind lại lần nữa khi mọi Awake đã xong.
            BindEconomy();
            Refresh();
        }

        /// <summary>
        /// EconomyService là nguồn coin chung (Home/Shop/Gameplay đều dùng),
        /// CoinManager chỉ là lớp delegate trong Gameplay.
        /// </summary>
        private void BindEconomy()
        {
            var eco = EconomyService.Instance;
            if (eco == null) return;
            if (ReferenceEquals(_economy, eco)) return;

            UnbindEconomy();
            _economy = eco;
            _economy.OnCoinChanged += OnEconomyCoinChanged;
        }

        private void UnbindEconomy()
        {
            if (_economy != null) _economy.OnCoinChanged -= OnEconomyCoinChanged;
            _economy = null;
        }

        private void OnEconomyCoinChanged(int amount) => UpdateCoinText(amount);

        private void Refresh()
        {
            if (_economy != null)
            {
                UpdateCoinText(_economy.GetCoin());
                return;
            }

            var cm = CoinManager.Instance;
            if (cm != null) UpdateCoinText(cm.GetBalance());
        }

        private void UpdateCoinText(int newBalance)
        {
            // Chi hien so luong coin (icon coin ben canh lo).
            if (coinText != null)
                coinText.text = NumberFormatter.FormatCoin(newBalance);
        }
    }
}
