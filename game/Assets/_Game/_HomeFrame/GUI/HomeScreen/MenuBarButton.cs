using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts
{
    // Nút trong menu bar (Home / Shop / Ranking).
    // Port từ dự án gốc: giữ nguyên tên field để MenuBar.prefab serialize đúng.
    // Đã bỏ SettingManager.PlaySound + LFramework.Vibration (target chưa có 2 hệ này).
    public class MenuBarButton : MonoBehaviour
    {
        [SerializeField] private Image icon;

        [SerializeField] private Vector3 unfocusIconPosition;
        [SerializeField] private float unfocusIconScale = 0.65f;
        [SerializeField] private Vector3 focusIconPosition;
        [SerializeField] private float focusIconScale = 1f;
        [SerializeField] private float duration = 0.35f;
        [SerializeField] private GameObject focusBackground;

        public Action OnClick;

        private Button _button;
        private RectTransform _rectTransform;

        private Sequence _focusSequence = null;
        private Sequence _unfocusSequence = null;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _button = GetComponent<Button>();
            if (_button != null)
                _button.onClick.AddListener(() => OnClick?.Invoke());
        }

        public void Init()
        {
        }

        public void Focus()
        {
            if (focusBackground != null) focusBackground.SetActive(true);
            if (icon == null) return;

            _focusSequence?.Kill();
            _focusSequence = DOTween.Sequence();
            _focusSequence.Join(icon.rectTransform.DOScale(focusIconScale, duration));
            _focusSequence.Join(icon.rectTransform.DOLocalMove(focusIconPosition, duration));
        }

        public void Unfocus()
        {
            if (focusBackground != null) focusBackground.SetActive(false);
            if (icon == null) return;

            _unfocusSequence?.Kill();
            _unfocusSequence = DOTween.Sequence();
            _unfocusSequence.Join(icon.rectTransform.DOScale(unfocusIconScale, duration));
            _unfocusSequence.Join(icon.rectTransform.DOLocalMove(unfocusIconPosition, duration));
        }

        public void OnClickButton()
        {
            OnClick?.Invoke();
        }
    }
}
