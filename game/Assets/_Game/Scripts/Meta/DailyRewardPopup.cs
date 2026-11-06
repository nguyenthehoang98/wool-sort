namespace _Game.Scripts.Meta
{
    using System;
    using _Game.Scripts.Core;
    using DG.Tweening;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>UI tạm Daily Reward: 7 ô, claim 1 lần/ngày. Kéo ref vào Inspector.</summary>
    public class DailyRewardPopup : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private DailyRewardConfig config;

        [Header("Containers")]
        [SerializeField] private Transform dayContainer; // cha chứa 7 ô ngày
        [SerializeField] private GameObject dayItemPrefab; // prefab ô ngày (có btnClaim, txtDay, txtAmount, imgIcon, imgClaimed)

        [Header("Nút đóng")]
        [SerializeField] private Button btnClose;

        [Header("Hiệu ứng")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform cardRect;

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
        }

        public void Hide()
        {
            if (_isAnimating) return;
            SetVisible(false, true);
        }

        private void Refresh()
        {
            if (dayContainer == null || dayItemPrefab == null) return;

            // Xoá ô cũ
            foreach (Transform child in dayContainer)
                Destroy(child.gameObject);

            var service = DailyRewardService.Instance;
            int streak = service != null ? service.Streak : 0;
            bool canClaim = service != null && service.CanClaimToday();

            for (int i = 0; i < 7; i++)
            {
                var entry = config != null ? config.GetEntry(i) : null;
                bool unlocked = streak >= i; // có thể claim ngày i+1 nếu streak >= i
                bool claimedToday = !canClaim && streak > i; // đơn giản: nếu không claim được hôm nay và streak đã qua ngày này -> đã claim

                var go = Instantiate(dayItemPrefab, dayContainer);
                var btn = go.GetComponentInChildren<Button>(true);
                var txtDay = go.transform.Find("TxtDay")?.GetComponent<TMP_Text>();
                var txtAmount = go.transform.Find("TxtAmount")?.GetComponent<TMP_Text>();
                var imgIcon = go.transform.Find("Icon")?.GetComponent<Image>();
                var imgClaimed = go.transform.Find("Claimed")?.GetComponent<Image>();

                if (txtDay != null) txtDay.text = $"DAY {i + 1}";
                if (entry != null)
                {
                    if (txtAmount != null) txtAmount.text = entry.amount.ToString();
                    if (imgIcon != null && entry.icon != null) imgIcon.sprite = entry.icon;
                }

                if (btn != null)
                {
                    btn.interactable = unlocked && canClaim && !claimedToday;
                    var dayIndex = i;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnClaimDay(dayIndex));
                }

                if (imgClaimed != null) imgClaimed.gameObject.SetActive(claimedToday);
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
