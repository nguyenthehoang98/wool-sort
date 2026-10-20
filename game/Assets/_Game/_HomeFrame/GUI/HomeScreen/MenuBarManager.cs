using System;
using DG.Tweening;
using HyperSonic;
using UnityEngine;

namespace _Game.Scripts
{
    // Quản lý menu bar 3 nút (Home / Shop / Ranking).
    // Port từ dự án gốc. Đã bỏ các phần phụ thuộc hệ đã bị prune:
    //   - Databuckets.Instance.Data.IAPPlacement (IAP SDK)
    //   - GameConst.OnReloadBanner (banner ads)
    //   - KitSDK.AddTrackingEventFirebase (analytics)
    // Phần đổi trang (slide) giữ nguyên nhưng chỉ chạy khi rectHome/rectShop/rectRank
    // được gán; nếu không thì HomeUI tự quản lý bật/tắt trang.
    public class MenuBarManager : Singleton<MenuBarManager>
    {
        [SerializeField] private MenuBarButton buttonShop;
        [SerializeField] private MenuBarButton buttonHome;
        [SerializeField] private MenuBarButton buttonRanking;
        [SerializeField] private FitPositionWithAds fitPositionWithAds;
        [SerializeField] private RectTransform contents;

        public Action OnClickOpenShop;
        public Action OnClickOpenHome;
        public Action OnClickOpenRanking;

        private Canvas parent;
        private Sequence _sequence;

        [SerializeField] private MenuBarOption _optionMenu = MenuBarOption.Home;
        [SerializeField] private RectTransform rectHome;
        [SerializeField] private RectTransform rectShop;
        [SerializeField] private RectTransform rectRank;

        public MenuBarOption OptionMenu => _optionMenu;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        protected override void Awake()
        {
            base.Awake();

            if (buttonShop != null) buttonShop.OnClick += OpenShop;
            if (buttonHome != null) buttonHome.OnClick += OpenHome;
            if (buttonRanking != null) buttonRanking.OnClick += OpenRanking;
        }

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            buttonShop?.Init();
            buttonHome?.Init();
            buttonRanking?.Init();

            Focus(_optionMenu);
            UpdatePositionBarBottom();
        }

        private void UpdatePositionBarBottom()
        {
            fitPositionWithAds?.UpdatePositionBarBottom();
        }

        public void OpenShop()
        {
            OnClickOpenShop?.Invoke();
            Focus(MenuBarOption.Shop);
        }

        public void OpenHome()
        {
            OnClickOpenHome?.Invoke();
            Focus(MenuBarOption.Home);
        }

        public void OpenRanking()
        {
            OnClickOpenRanking?.Invoke();
            Focus(MenuBarOption.Ranking);
        }

        public void Focus(MenuBarOption option)
        {
            _optionMenu = option;

            buttonShop?.Unfocus();
            buttonHome?.Unfocus();
            buttonRanking?.Unfocus();

            switch (option)
            {
                case MenuBarOption.Shop:
                    buttonShop?.Focus();
                    break;
                case MenuBarOption.Home:
                    buttonHome?.Focus();
                    break;
                case MenuBarOption.Ranking:
                    buttonRanking?.Focus();
                    break;
            }

            MoveContentsBy(_optionMenu, option);
        }

        public void MoveDown()
        {
            fitPositionWithAds?.MoveDown();
        }

        public void MoveUp()
        {
            fitPositionWithAds?.MoveUp();
        }

        /// <summary>Slide giữa 3 trang. Chỉ chạy khi 3 rect trang đã được gán.</summary>
        public void MoveContentsBy(MenuBarOption optionBefore, MenuBarOption optionAfter)
        {
            if (rectHome == null || rectShop == null || rectRank == null) return;

            RectTransform rectMove = optionBefore switch
            {
                MenuBarOption.Home => rectHome,
                MenuBarOption.Shop => rectShop,
                _ => rectRank
            };

            RectTransform rectShow = optionAfter switch
            {
                MenuBarOption.Home => rectHome,
                MenuBarOption.Shop => rectShop,
                _ => rectRank
            };

            if (optionBefore == optionAfter)
            {
                rectHome.gameObject.SetActive(optionAfter == MenuBarOption.Home);
                rectShop.gameObject.SetActive(optionAfter == MenuBarOption.Shop);
                rectRank.gameObject.SetActive(optionAfter == MenuBarOption.Ranking);
                return;
            }

            CheckAndGetCanvasParent();
            if (parent == null) return;

            var posHide = parent.GetComponent<RectTransform>().sizeDelta.x;
            var posShowStart = -parent.GetComponent<RectTransform>().sizeDelta.x;
            bool moveRight = true;
            if (optionAfter == MenuBarOption.Ranking)
            {
                moveRight = false;
            }
            else if (optionAfter == MenuBarOption.Home && optionBefore == MenuBarOption.Shop)
            {
                moveRight = false;
            }
            if (moveRight == false)
            {
                posHide = -parent.GetComponent<RectTransform>().sizeDelta.x;
                posShowStart = parent.GetComponent<RectTransform>().sizeDelta.x;
            }

            rectShow.gameObject.SetActive(true);
            rectShow.anchoredPosition = new Vector2(posShowStart, rectShow.anchoredPosition.y);
            rectMove.gameObject.SetActive(true);

            _sequence?.Kill();
            _sequence = DOTween.Sequence();
            _sequence.Append(rectMove.DOAnchorPosX(posHide, 0.2f).SetEase(Ease.Linear));
            _sequence.Join(rectShow.DOAnchorPosX(0, 0.2f).SetEase(Ease.Linear));
            _sequence.OnComplete(() =>
            {
                rectHome.gameObject.SetActive(optionAfter == MenuBarOption.Home);
                rectShop.gameObject.SetActive(optionAfter == MenuBarOption.Shop);
                rectRank.gameObject.SetActive(optionAfter == MenuBarOption.Ranking);
            });
        }

        public void CheckAndGetCanvasParent()
        {
            if (parent == null)
            {
                parent = GetComponentInParent<Canvas>();
            }
        }
    }
}
