namespace _Game.Scripts.Meta
{
    using System;
    using System.Collections.Generic;
    using _Game.Scripts.Core;
    using DG.Tweening;
    using TMPro;
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

        [Header("Text")]
        public TMP_Text txtCountdown;

        private readonly List<DailyRewardItem> _items = new List<DailyRewardItem>(7);
        private bool _isAnimating;

        private void Awake()
        {
            if (btnClaim != null) btnClaim.onClick.AddListener(OnClaim);
            if (btnClose != null) btnClose.onClick.AddListener(Hide);
            SetVisible(false, true);
        }

        private void OnEnable()
        {
            if (gameObject.activeSelf) Refresh();
        }

        private void Update()
        {
            UpdateCountdown();
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
            var svc = ServiceBootstrap.EnsureDailyReward();
            if (svc == null || !svc.CanClaimToday()) return;

            int claimed = svc.Claim();
            if (claimed < 0) return;

            AudioService.Instance?.PlayCoinCollect();
            Refresh(); // giữ popup mở để thấy tick
        }

        private void Refresh()
        {
            if (config == null)
                config = Resources.Load<DailyRewardConfig>("DailyRewardConfig");

            var svc = ServiceBootstrap.EnsureDailyReward();
            if (svc != null && svc.Config == null && config != null)
            {
                // service tự load từ Resources nếu cần; popup ưu tiên config hiện có
            }

            BuildItems();
            if (_items.Count == 0) return;

            int streak = svc != null ? Mathf.Clamp(svc.Streak, 0, 6) : 0;
            bool canClaim = svc != null && svc.CanClaimToday();
            int todayIndex = canClaim ? streak : (streak + 6) % 7;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item == null) continue;
                bool inCycle = i < 7;
                item.gameObject.SetActive(inCycle);
                if (!inCycle) continue;

                var day = config != null ? config.GetDay(i) : null;
                bool isToday = i == streak;
                bool claimed;
                if (canClaim)
                {
                    // streak = ngày tiếp theo có thể claim
                    claimed = streak == 0 ? false : i < streak;
                }
                else
                {
                    // đã claim hôm nay: streak đã tăng sang ngày kế tiếp
                    int lastClaimed = (streak + 6) % 7;
                    claimed = streak == 0 ? true : i <= lastClaimed;
                    isToday = i == streak; // ngày tiếp theo, đang chờ
                }

                item.Setup(i, day, isToday, claimed, false);
            }

            if (btnClaim != null) btnClaim.gameObject.SetActive(canClaim);
            if (btnClaimInactive != null) btnClaimInactive.gameObject.SetActive(!canClaim);
            UpdateCountdown();
        }

        private void BuildItems()
        {
            _items.Clear();
            if (dayContainer == null) return;

            // Ưu tiên item đã có sẵn trong container
            for (int i = 0; i < dayContainer.childCount; i++)
            {
                var it = dayContainer.GetChild(i).GetComponent<DailyRewardItem>();
                if (it != null) _items.Add(it);
            }

            if (_items.Count == 0 && dayItemPrefab != null)
            {
                for (int i = 0; i < 7; i++)
                {
                    var go = Instantiate(dayItemPrefab, dayContainer);
                    _items.Add(go);
                }
            }

            // đảm bảo đủ 7 nếu có prefab
            if (dayItemPrefab != null)
            {
                while (_items.Count < 7)
                {
                    var go = Instantiate(dayItemPrefab, dayContainer);
                    _items.Add(go);
                }
            }

            // nếu thừa thì chỉ dùng 7 cái đầu
            if (_items.Count > 7) _items.RemoveRange(7, _items.Count - 7);
        }

        private void UpdateCountdown()
        {
            if (txtCountdown == null) return;
            var svc = DailyRewardService.Instance ?? ServiceBootstrap.EnsureDailyReward();
            if (svc == null)
            {
                txtCountdown.text = string.Empty;
                return;
            }

            if (svc.CanClaimToday())
            {
                txtCountdown.text = "Claim now!";
                return;
            }

            var remain = svc.TimeUntilNextClaim();
            txtCountdown.text = $"Next reward in {remain.Hours:00}:{remain.Minutes:00}:{remain.Seconds:00}";
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
