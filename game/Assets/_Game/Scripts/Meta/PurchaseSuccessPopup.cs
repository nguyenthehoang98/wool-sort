namespace _Game.Scripts.Meta
{
    using _Game.Scripts;
    using _Game.Scripts.Core;
    using _Game.Scripts.Gameplay;
    using _Game.Scripts.UI;
    using _Game.Scripts.Utils;
    using DG.Tweening;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    // Màn hình nhận thưởng sau khi mua hàng trong shop.
    // Port từ PurchaseSuccessPopup của dự án gốc nhưng viết lại cho target:
    // không dùng FrameBase / UIScale / UIAnimation / I2 Localize (đều đã bị prune).
    public class PurchaseSuccessPopup : MonoBehaviour
    {
        public static PurchaseSuccessPopup Instance { get; private set; }

        [Header("Parts (gán trong prefab)")]
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text txtTitle;
        [SerializeField] private TMP_Text txtName;
        [SerializeField] private TMP_Text txtAmount;
        [SerializeField] private Button btnOk;
        [SerializeField] private Button btnClose;
        [SerializeField] private GameObject shade;
        [SerializeField] private GameObject card;
        [SerializeField] private RectTransform cardRect;

        private ShopItem _item;

        private void Awake()
        {
            Instance = this;

            if (btnOk != null) btnOk.onClick.AddListener(Hide);
            if (btnClose != null) btnClose.onClick.AddListener(Hide);

            SetVisible(false);
        }

        /// <summary>
        /// Bắn coin từ popup lên HUD coin (giống lúc Win). Chạy NGAY khi popup hiện lên
        /// (lúc mua xong), KHÔNG đợi bấm xác nhận. Chỉ áp dụng cho gói coin.
        /// </summary>
        private void PlayCoinFly()
        {
            var hud = CoinHUD.Instance;
            bool isCoinPack = _item != null && !_item.IsBooster && _item.coinAmount > 0;
            if (!isCoinPack || hud == null) return;

            var splash = SplashItem.HasInstance ? SplashItem.Instance : null;
            if (splash == null) return;

            var from = cardRect != null ? cardRect.gameObject
                     : (card != null ? card : gameObject);
            splash.SplashCoin(5, from, hud.IconTransform.gameObject, 0.5f, null,
                () => hud.ShowVFX(), null);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Lấy popup nhận thưởng và bật lên nếu đang tắt.
        ///
        /// GameObject gốc được để TẮT trong scene (để Scene View gọn và không
        /// chặn input) — nên Awake chưa chạy, Instance còn null, và phải tìm cả
        /// object inactive mới thấy. Đây là lý do popup không hiện nếu chỉ dùng
        /// PurchaseSuccessPopup.Instance.
        /// </summary>
        public static PurchaseSuccessPopup Acquire()
        {
            if (Instance == null)
            {
                Instance = FindFirstObjectByType<PurchaseSuccessPopup>(FindObjectsInactive.Include);
            }

            if (Instance == null) return null;

            if (!Instance.gameObject.activeSelf)
            {
                // Awake chạy khi bật -> Instance chính thức + tự ẩn card đi
                Instance.gameObject.SetActive(true);
            }

            return Instance;
        }

        /// <summary>Hiện màn nhận thưởng cho gói vừa mua.</summary>
        public void Show(ShopItem item)
        {
            if (item == null) return;

            _item = item;

            // GameObject gốc có thể đang tắt (đặt vậy trong scene) -> bật lên.
            if (!gameObject.activeSelf) gameObject.SetActive(true);

            if (icon != null) icon.sprite = item.icon;
            if (txtTitle != null) txtTitle.text = "Purchase Complete!";
            if (txtName != null) txtName.text = item.displayName;
            if (txtAmount != null) txtAmount.text = BuildAmount(item);

            SetVisible(true);

            AudioService.Instance?.PlayPopupOpen();

            // Bắn coin lên HUD ngay khi popup hiện lên (không đợi bấm OK).
            PlayCoinFly();

            if (cardRect != null)
            {
                cardRect.DOKill();
                cardRect.localScale = Vector3.one * 0.85f;
                cardRect.DOScale(Vector3.one, 0.18f).SetEase(Ease.OutBack);
            }
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (shade != null) shade.SetActive(visible);
            if (card != null) card.SetActive(visible);
        }

        private static string BuildAmount(ShopItem item)
        {
            if (item.IsBooster) return $"x{item.boosterAmount} {item.boosterType}";
            return item.coinAmount > 0 ? $"+{NumberFormatter.FormatCoin(item.coinAmount)} coins" : string.Empty;
        }
    }
}
