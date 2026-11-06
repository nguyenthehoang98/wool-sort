using _Game.Scripts.Utils;
using _Game.Scripts.Core;
using _KIT;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Gameplay
{
    public class BoosterItem : MonoBehaviour
    {
        [Header("Chọn loại Booster")]
        [SerializeField] private BoosterType type;

        [Header("UI References")]
        [SerializeField] private Button buttonBooster;

        [SerializeField] private GameObject objNormal;
        [SerializeField] private GameObject objLock;

        [SerializeField] private GameObject objNoti;
        [SerializeField] private TMP_Text countText;

        [SerializeField] private GameObject objPlus;

        [Header("Popup mua booster (kéo thả, tùy chọn)")]
        [Tooltip("Để trống cũng được — sẽ lấy từ GameUIController.moreBoosterPopup.")]
        [SerializeField] private MoreBoosterPopupUI moreBoosterPopup;

        private ButtonBoosterState _btnBoosterState;
        private bool isUnlocked;
        public bool IsUnlocked => isUnlocked;

        private void Awake()
        {
            if (buttonBooster != null)
            {
                buttonBooster.onClick.AddListener(() =>
                {
                    OnClick();
                });
            }
        }

        // Gán reference khi dựng UI bằng code (BoosterBarUI) thay vì prefab.
        public void Bind(BoosterType boosterType, Button button, GameObject normal, GameObject lockObj, GameObject noti, TMP_Text count, GameObject plus)
        {
            type = boosterType;
            if (buttonBooster != null)
                buttonBooster.onClick.RemoveAllListeners();
            buttonBooster = button;
            objNormal = normal;
            objLock = lockObj;
            objNoti = noti;
            countText = count;
            objPlus = plus;
            if (buttonBooster != null)
                buttonBooster.onClick.AddListener(OnClick);
            Setup();
        }

        private void OnEnable()
        {
            GameConst.OnChangeBooster += OnChangeBooster;
        }

        private void OnDisable()
        {
            GameConst.OnChangeBooster -= OnChangeBooster;
        }

        private void OnChangeBooster(ChangeBoosterArgs args)
        {
            Setup();
        }

        public void Setup()
        {
            if (objNormal == null || objLock == null || objNoti == null || objPlus == null || countText == null)
            {
                Debug.LogWarning($"[BoosterItem] {type} thiếu ref UI (dựng bằng BoosterBarUI để đủ ref).", this);
                return;
            }
            _btnBoosterState = ButtonBoosterState.Noti;

            objNormal.SetActive(true);
            objLock.SetActive(false);
            objNoti.SetActive(false);
            objPlus.SetActive(false);

            var unlockLevel = Configs.GetBoosterUnlockLevel(type);
            isUnlocked = true;

            // với booster clean thì ở level tut phải hiện tut mới show
            if (type == BoosterType.Clean)
            {
                if (GameData.Instance.Level() < unlockLevel)
                {
                    _btnBoosterState = ButtonBoosterState.Lock;
                    isUnlocked = false;
                }
                else if (GameData.Instance.Level() == unlockLevel)
                {
                    if (!GameData.Instance.IsBoosterTutShown(type))
                    {
                        _btnBoosterState = ButtonBoosterState.Lock;
                        isUnlocked = false;
                    }
                }
            }
            else
            {
                if (GameData.Instance.Level() < unlockLevel)
                {
                    _btnBoosterState = ButtonBoosterState.Lock;
                    isUnlocked = false;
                }
            }

            if (_btnBoosterState != ButtonBoosterState.Lock)
            {
                int count = GameData.Instance.GetCount(type);
                if (count > 0)
                {
                    _btnBoosterState = ButtonBoosterState.Noti;
                }
                else
                {
                    _btnBoosterState = ButtonBoosterState.Plus;
                }

                objNoti.SetActive(_btnBoosterState == ButtonBoosterState.Noti);
                objPlus.gameObject.SetActive(_btnBoosterState == ButtonBoosterState.Plus);

                countText.text = count.ToString();
            }
            else
            {
                objLock.SetActive(true);
                objNormal.SetActive(false);
            }
        }

        public void OnClick()
        {
            var gameManager = GameManager.Instance;
            var boosterManager = BoosterManager.Instance;
            if (gameManager == null || boosterManager == null)
            {
                Debug.LogWarning("[BoosterItem] Thiếu GameManager/BoosterManager trong scene, bỏ qua click.", this);
                return;
            }
            if (!gameManager.CanInteractWithGame() || boosterManager.IsLockByUsingBooster) return;

            if (_btnBoosterState == ButtonBoosterState.Lock)
            {
                var unlockLevel = Configs.GetBoosterUnlockLevel(type);
                ShowTooltip($"Unlocks at level {unlockLevel}");
                return;
            }

            if (!boosterManager.CanUseBooster(type))
            {
                return;
            }

            if (EconomyService.Instance.GetBoosterCount(type) <= 0)
            {
                OpenBuyPopup();
                return;
            }

            EconomyService.Instance.SpendBooster(type, 1, "use", "use_booster");
            boosterManager.UseBooster(type);
        }

        private static void ShowTooltip(string message)
        {
            // KitTooltipManager tự tạo instance nếu scene chưa có (mở thẳng scene
            // Gameplay) → thông báo luôn hiện lên màn hình.
            KitTooltipManager.ShowMessage(message);
        }

        private void OpenBuyPopup()
        {
            // Ưu tiên ref kéo thả trên GameUIController; nếu chưa gán thì
            // MoreBoosterPopupUI.Show tự tìm instance trong scene.
            var popup = moreBoosterPopup != null
                ? moreBoosterPopup
                : (GameUIController.Instance != null ? GameUIController.Instance.moreBoosterPopup : null);

            if (popup != null)
            {
                popup.ShowFor(type);
                return;
            }

            MoreBoosterPopupUI.Show(type);
        }

        public BoosterType GetBoosterType()
        {
            return type;
        }
    }
}
