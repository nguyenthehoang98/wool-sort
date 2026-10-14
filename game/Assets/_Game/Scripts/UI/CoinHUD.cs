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
        [Header("UI References (drag in Inspector)")]
        public TextMeshProUGUI coinText;

        private void Awake()
        {
            // Initialize display
            UpdateCoinText(CoinManager.Instance?.GetBalance() ?? 0);
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
