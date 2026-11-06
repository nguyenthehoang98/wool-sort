namespace _Game.Scripts.Meta
{
    using System;
    using System.Collections.Generic;
    using _Game.Scripts.Core;
    using DG.Tweening;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Popup Daily Reward 7 ngày. Claim 1 lần/ngày, streak tăng theo ngày liên tiếp.
    /// Prefab cần: 7 con DailyRewardItem trong dayContainer, nút Close, canvasGroup, cardRect.</summary>
    public class DailyRewardPopup : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private DailyRewardConfig config;

        [Header("Containers")]
        [SerializeField] private Transform dayContainer;
        [SerializeField] private DailyRewardItem dayItemPrefab;

        [Header("Nút đóng")]
        [SerializeField] private Button btnClose;

        [Header("Hiệu ứng")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform cardRect;

        private readonly List<DailyRewardItem> _items = new List<DailyRewardItem>(7);
        private bool _isAnimating;

        private void Awake()
        {
            if (btnClose != null) btnClose.onClick.AddListener(Hide);
            SetVisible(false, true);
        }

        public void Show()
        {
            if (_isAnimating) return;
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            Refresh();
            SetVisible(true, false);
            AudioService.Instance?.PlayPopupOpen();
        }

        public void Hide()
        {
            if (_isAnimating) return;
            AudioService.Instance?.PlayPopupClose();
            SetVisible(false, true);
        }

        private void Refresh()
        {
            if (dayContainer == null || dayItemPrefab == null) return;

            var service = DailyRewardService.Instance;
            int streak = service != null ? service.Streak : 0;
            bool canClaim = service != null && service.CanClaimToday();

            // Đảm bảo đủ 7 item
            while (_items.Count < 7)
            {
                var go = Instantiate(dayItemPrefab, dayContainer);
                _items.Add(go.GetComponent<DailyRewardItem>());
            }

            for (int i = 0; i < 7; i++)
            {
                var entry = config != null ? config.GetEntry(i) : null;
                // Ngày i+1 có thể claim nếu streak >= i (đã claim đủ i ngày trước) và chưa claim hôm nay
                bool unlocked = streak >= i;
                bool claimedToday = !canClaim && streak > i;
                bool available = unlocked && canClaim && !claimedToday;

                var item = _items[i];
                item.Setup(i + 1, entry, unlocked, available, claimedToday, OnClaimDay);
            }
        }

        private void OnClaimDay(int dayIndex)
        {
            var service = DailyRewardService.Instance;
            if (service == null) return;

            var entry = service.Claim();
            if (entry == null) return;

            AudioService.Instance?.PlayCoinCollect();
            Refresh();
            service.OnRewardClaimed?.Invoke(entry);
            Hide();
        }

        private void SetVisible(bool visible, bool instant)
        {
            if (canvasGroup == null)
            {
                gameObject.SetActive(visible);
                return;
            }

            canvasGroup.DOKill();
            cardRect?.DOKill();

            if (visible)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                if (instant)
                {
                    canvasGroup.alpha = 1f;
                    cardRect.transform.localScale = Vector3.one;
                }
                else
                {
                    _isAnimating = true;
                    canvasGroup.alpha = 0f;
                    cardRect.transform.localScale = Vector3.one * 0.85f;
                    canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutBack);
                    cardRect.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack)
                        .OnComplete(() => _isAnimating = false);
                }
            }
            else
            {
                if (instant)
                {
                    canvasGroup.alpha = 0f;
                    cardRect.transform.localScale = Vector3.one;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                }
                else
                {
                    _isAnimating = true;
                    canvasGroup.DOFade(0f, 0.25f).SetEase(Ease.InBack);
                    cardRect.transform.DOScale(0.85f, 0.25f).SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            _isAnimating = false;
                            canvasGroup.interactable = false;
                            canvasGroup.blocksRaycasts = false;
                            gameObject.SetActive(false);
                        });
                }
            }
        }
    }
}
