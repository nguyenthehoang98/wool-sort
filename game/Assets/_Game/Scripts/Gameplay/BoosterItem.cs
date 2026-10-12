using _Game.Scripts.Utils;
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

        private ButtonBoosterState _btnBoosterState;
        private bool isUnlocked;
        public bool IsUnlocked => isUnlocked;

        private void Awake()
        {
            buttonBooster.onClick.AddListener(() =>
            {
                OnClick();
            });
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
            _btnBoosterState = ButtonBoosterState.Noti;

            objNormal.SetActive(true);
            objLock.SetActive(false);
            objNoti.SetActive(false);
            objPlus.SetActive(false);

            var unlockLevel = Configs.LIST_BOOSTER_UNLOCK[(int)type];
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
            if (!GameManager.Instance.CanInteractWithGame() || BoosterManager.Instance.IsLockByUsingBooster) return;

            if (_btnBoosterState == ButtonBoosterState.Lock)
            {
                var unlockLevel = Configs.LIST_BOOSTER_UNLOCK[(int)type];
                KitTooltipManager.Instance.Show($"Unlock at level {unlockLevel}");
                return;
            }

            if (!BoosterManager.Instance.CanUseBooster(type))
            {
                return;
            }

            if (GameData.Instance.GetCount(type) <= 0)
            {
                OpenBuyPopup();
                return;
            }

            GameData.Instance.AddCount(type, -1, "use", "use_booster");
            BoosterManager.Instance.UseBooster(type);
        }

        private void OpenBuyPopup()
        {
            Debug.LogWarning($"[Stub] OpenBuyPopup {type} (shop chưa có trong bản này).");
        }

        public BoosterType GetBoosterType()
        {
            return type;
        }
    }
}
