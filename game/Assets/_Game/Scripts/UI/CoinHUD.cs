using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _Game.Scripts.Gameplay;


namespace _Game.Scripts.UI
{
    // Simple HUD to display current coin balance.
    // Attach this component to a UI GameObject that has a TextMeshProUGUI reference.
    public class CoinHUD : MonoBehaviour
    {
        public static CoinHUD Instance { get; private set; }

        [Header("UI References (drag in Inspector)")]
        public TextMeshProUGUI coinText;

        [Header("FX (drag in Inspector)")]
        public RectTransform iconCoin; // vị trí phát hiệu ứng get-coin + đích bay của coin

        private RectTransform _rectTransform;
        private Vector3 _oriAnchoredPos;

        private void Awake()
        {
            Instance = this;
            _rectTransform = transform as RectTransform;
            if (_rectTransform != null) _oriAnchoredPos = _rectTransform.anchoredPosition;

            // Initialize display
            UpdateCoinText(CoinManager.Instance?.GetBalance() ?? 0);
        }

        private void OnDestroy()
        {
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
            UpdateCoinText(CoinManager.Instance != null ? CoinManager.Instance.GetBalance() : 0);
        }

        private void OnDisable()
        {
            CoinManager.OnBalanceChanged -= UpdateCoinText;
        }

        private void Start()
        {
            // CoinManager.Awake may run after this HUD's Awake/OnEnable,
            // so refresh once more after all Awake calls have finished.
            UpdateCoinText(CoinManager.Instance != null ? CoinManager.Instance.GetBalance() : 0);
        }

        private void UpdateCoinText(int newBalance)
        {
            if (coinText != null)
                coinText.text = $"Coins: {newBalance}";
        }
    }
}
