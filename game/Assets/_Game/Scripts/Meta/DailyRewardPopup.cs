namespace _Game.Scripts.Meta
{
    using System.Collections.Generic;
    using _Game.Scripts.Core;
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Popup Daily Reward 7 ngày. Mỗi ngày có thể nhiều phần thưởng; claim 1 lần/ngày.
    /// Prefab cần: dayContainer + dayItemPrefab (DailyRewardItem), btnClaim, btnClaimInactive, btnClose, cardRect.</summary>
    public class DailyRewardPopup : MonoBehaviour
    {
        [Header("Config")]
        public DailyRewardConfig config;

        [Header("Containers")]
        public Transform dayContainer;
        public DailyRewardItem dayItemPrefab;

        [Header("Buttons")]
        public Button btnClaim;
        public Button btnClaimInactive;
        public Button btnClose;

        [Header("Hiệu ứng")]
        public RectTransform cardRect;

        private readonly List<DailyRewardItem> _items = new List<DailyRewardItem>(7);
        private bool _isAnimating;

        private void Awake()
        {
            if (btnClaim != null) btnClaim.onClick.AddListener(OnClaim);
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

        private void OnClaim()
        {
            var svc = DailyRewardService.Instance;
            if (svc == null || !svc.CanClaimToday()) return;

            int claimed = svc.Claim();
            if (claimed < 0) return;

            AudioService.Instance?.PlayCoinCollect();
            Refresh(); // giữ popup mở để thấy tick
        }

        private void Refresh()
        {
            if (dayContainer == null || dayItemPrefab == null)
            {
                Debug.LogWarning("[DailyRewardPopup] Thiếu dayContainer hoặc dayItemPrefab.");
                return;
            }

            while (_items.Count < 7)
            {
                var go = Instantiate(dayItemPrefab, dayContainer);
                _items.Add(go);
            }

            var svc = DailyRewardService.Instance;
            int streak = svc != null ? svc.Streak : 0;
            bool canClaim = svc != null && svc.CanClaimToday();

            for (int i = 0; i < 7; i++)
            {
                var day = config != null ? config.GetDay(i) : null;
                bool isToday = i == streak;
                bool claimed = i < streak;
                _items[i].Setup(i, day, isToday, claimed, false);
            }

            if (btnClaim != null) btnClaim.gameObject.SetActive(canClaim);
            if (btnClaimInactive != null) btnClaimInactive.gameObject.SetActive(!canClaim);
        }

        private void SetVisible(bool visible, bool instant)
        {
            if (cardRect != null) cardRect.DOKill();

            if (visible)
            {
                gameObject.SetActive(true);
                if (instant || cardRect == null)
                {
                    if (cardRect != null) cardRect.localScale = Vector3.one;
                }
                else
                {
                    _isAnimating = true;
                    cardRect.localScale = Vector3.one * 0.85f;
                    cardRect.DOScale(1f, 0.25f).SetEase(Ease.OutBack).OnComplete(() => _isAnimating = false);
                }
            }
            else
            {
                if (instant || cardRect == null)
                {
                    _isAnimating = false;
                    if (cardRect != null) cardRect.localScale = Vector3.one;
                    gameObject.SetActive(false);
                }
                else
                {
                    _isAnimating = true;
                    cardRect.DOScale(0.85f, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
                    {
                        _isAnimating = false;
                        gameObject.SetActive(false);
                    });
                }
            }
        }
    }
}
