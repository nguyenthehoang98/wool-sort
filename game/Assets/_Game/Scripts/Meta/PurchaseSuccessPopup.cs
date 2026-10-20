namespace _Game.Scripts.Meta
{
    using _Game.Scripts.Core;
    using _Game.Scripts.Gameplay;
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

        private void Awake()
        {
            Instance = this;

            if (btnOk != null) btnOk.onClick.AddListener(Hide);
            if (btnClose != null) btnClose.onClick.AddListener(Hide);

            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Hiện màn nhận thưởng cho gói vừa mua.</summary>
        public void Show(ShopItem item)
        {
            if (item == null) return;

            if (icon != null) icon.sprite = item.icon;
            if (txtTitle != null) txtTitle.text = "Nhận thưởng!";
            if (txtName != null) txtName.text = item.displayName;
            if (txtAmount != null) txtAmount.text = BuildAmount(item);

            SetVisible(true);

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
            if (item.boosterType != BoosterType.None) return $"x{item.boosterAmount} {item.boosterType}";
            return item.coinAmount > 0 ? $"+{item.coinAmount} coin" : string.Empty;
        }
    }
}
