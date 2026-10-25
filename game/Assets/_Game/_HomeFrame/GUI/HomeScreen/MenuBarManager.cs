using System;
using HyperSonic;
using UnityEngine;

namespace _Game.Scripts
{
    // Quản lý menu bar 3 nút (Home / Shop / Ranking).
    // Chỉ giữ trạng thái tab + focus icon. Việc bật/tắt trang do HomeUI quản lý
    // (menu bar không còn điều khiển vị trí/slide frame).
    public class MenuBarManager : Singleton<MenuBarManager>
    {
        [SerializeField] private MenuBarButton buttonShop;
        [SerializeField] private MenuBarButton buttonHome;
        [SerializeField] private MenuBarButton buttonRanking;

        public Action OnClickOpenShop;
        public Action OnClickOpenHome;
        public Action OnClickOpenRanking;

        [SerializeField] private MenuBarOption _optionMenu = MenuBarOption.Home;

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
        }
    }
}
