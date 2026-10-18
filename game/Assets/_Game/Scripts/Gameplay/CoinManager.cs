using System;
using UnityEngine;
using _Game.Scripts.Core;

namespace _Game.Scripts.Gameplay
{
    // Simple singleton to manage player's coin balance.
    // Sử dụng PlayerPrefs để persist (key: "PlayerCoins").
    // Khi EconomyService đã khởi tạo (DontDestroyOnLoad), CoinManager delegated
    // sang EconomyService đểHomeUI/shop và WinPopup đọc cùng một nguồn coin.
    public class CoinManager : MonoBehaviour
    {
        public static CoinManager Instance { get; private set; }
        private const string PlayerPrefsKey = "PlayerCoins";
        public static event System.Action<int> OnBalanceChanged;

        private int _balance;
        private bool _syncedToEconomy;

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

        private void Start()
        {
            SyncFromEconomy();
        }

        private void SyncFromEconomy()
        {
            if (_syncedToEconomy) return;
            if (EconomyService.Instance != null)
            {
                _balance = EconomyService.Instance.GetCoin();
                _syncedToEconomy = true;
            }
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
            if (EconomyService.Instance != null)
            {
                EconomyService.Instance.AddCoins(amount, "level_win", "win_popup");
                _balance = EconomyService.Instance.GetCoin();
            }
            else
            {
                _balance += amount;
                SaveBalance();
            }
            Debug.Log($"[CoinManager] Added {amount} coins. New balance: {_balance}");
            OnBalanceChanged?.Invoke(_balance);
        }

        public bool ConsumeCoins(int amount)
        {
            if (amount <= 0) return true;
            if (EconomyService.Instance != null)
            {
                var ok = EconomyService.Instance.SpendCoins(amount, "consume", "coin_manager");
                _balance = EconomyService.Instance.GetCoin();
                if (!ok)
                {
                    Debug.LogWarning($"[CoinManager] Not enough coins. Have {_balance}, need {amount}");
                }
                return ok;
            }
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
