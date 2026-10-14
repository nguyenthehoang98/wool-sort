using System;
using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    // Simple singleton to manage player's coin balance.
    // Uses PlayerPrefs for persistence (key: "PlayerCoins").
    public class CoinManager : MonoBehaviour
    {
        public static CoinManager Instance { get; private set; }
        private const string PlayerPrefsKey = "PlayerCoins";
        public static event System.Action<int> OnBalanceChanged;

        private int _balance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadBalance();
        }

        private void LoadBalance()
        {
            _balance = PlayerPrefs.GetInt(PlayerPrefsKey, 0);
        }

        private void SaveBalance()
        {
            PlayerPrefs.SetInt(PlayerPrefsKey, _balance);
            PlayerPrefs.Save();
        }

        public int GetBalance() => _balance;

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            _balance += amount;
            SaveBalance();
            Debug.Log($"[CoinManager] Added {amount} coins. New balance: {_balance}");
            OnBalanceChanged?.Invoke(_balance);
        }

        public bool ConsumeCoins(int amount)
        {
            if (amount <= 0) return true; // nothing to consume
            if (_balance < amount)
            {
                Debug.LogWarning($"[CoinManager] Not enough coins. Have {_balance}, need {amount}");
                return false;
            }
            _balance -= amount;
            SaveBalance();
            Debug.Log($"[CoinManager] Consumed {amount} coins. New balance: {_balance}");
            OnBalanceChanged?.Invoke(_balance);
            return true;
        }
    }
}
