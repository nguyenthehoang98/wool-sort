using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts
{
    // Nút trong menu bar (Home / Shop / Ranking).
    // Port từ dự án gốc: giữ nguyên tên field để MenuBar.prefab serialize đúng.
    // Đã bỏ SettingManager.PlaySound + LFramework.Vibration (target chưa có 2 hệ này).
    //
    // Chỉ animate phần icon (scale + offset tương đối theo anchoredPosition gốc).
    // KHÔNG animate RectTransform của nút: nút dùng anchor chia 1/3 chiều ngang nên
    // localPosition phụ thuộc layout; animate localPosition sẽ kéo cả 3 nút về giữa
    // (chồng lên nhau) khi vào Play.
    public class MenuBarButton : MonoBehaviour
    {
        [SerializeField] private Image icon;

        [SerializeField] private Vector3 unfocusIconPosition;
        [SerializeField] private float unfocusIconScale = 0.85f;
        [SerializeField] private Vector3 focusIconPosition;
        [SerializeField] private float focusIconScale = 1.1f;
        [SerializeField] private float duration = 0.35f;

        public Action OnClick;

        private Button _button;
        private Vector2 _iconOriginAnchoredPosition;
        private Vector3 _iconOriginScale;

        private Sequence _focusSequence = null;
        private Sequence _unfocusSequence = null;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
                _button.onClick.AddListener(() => OnClick?.Invoke());

            if (icon != null)
            {
                _iconOriginAnchoredPosition = icon.rectTransform.anchoredPosition;
                _iconOriginScale = icon.rectTransform.localScale;
            }
        }

        public void Init()
        {
        }

        public void Focus()
        {
            _focusSequence?.Kill();
            _focusSequence = DOTween.Sequence();
            if (icon == null) return;

            _focusSequence.Join(icon.rectTransform
                .DOAnchorPos(_iconOriginAnchoredPosition + (Vector2)focusIconPosition, duration));
            _focusSequence.Join(icon.rectTransform
                .DOScale(_iconOriginScale * focusIconScale, duration));
        }

        public void Unfocus()
        {
            _unfocusSequence?.Kill();
            _unfocusSequence = DOTween.Sequence();
            if (icon == null) return;

            _unfocusSequence.Join(icon.rectTransform
                .DOAnchorPos(_iconOriginAnchoredPosition + (Vector2)unfocusIconPosition, duration));
            _unfocusSequence.Join(icon.rectTransform
                .DOScale(_iconOriginScale * unfocusIconScale, duration));
        }

        public void OnClickButton()
        {
            OnClick?.Invoke();
        }
    }
}
