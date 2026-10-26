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
    // Focus/Unfocus animate 2 phần:
    //   - Icon: offset + scale (scale là hệ số nhân với scale gốc của icon).
    //   - Frame (RectTransform gán vào field `frame`, để trống thì dùng rect của nút): chỉ offset, không scale.
    // Cả 2 đều dùng anchoredPosition tương đối với giá trị gốc (không dùng localPosition)
    // vì nút dùng anchor chia 1/3 chiều ngang: localPosition phụ thuộc layout, còn
    // anchoredPosition thì không -> tránh việc cả 3 nút bị kéo về giữa khi vào Play.
    public class MenuBarButton : MonoBehaviour
    {
        [SerializeField] private Image icon;

        [Header("Icon")]
        [SerializeField] private Vector3 unfocusIconPosition;
        [SerializeField] private float unfocusIconScale = 0.85f;
        [SerializeField] private Vector3 focusIconPosition;
        [SerializeField] private float focusIconScale = 1.1f;

        [Header("Frame")]
        [Tooltip("RectTransform sẽ được di chuyển khi focus/unfocus. Để trống = dùng chính RectTransform của nút.")]
        [SerializeField] private RectTransform frame;
        [SerializeField] private Vector3 unfocusFramePosition;
        [SerializeField] private Vector3 focusFramePosition;

        [SerializeField] private float duration = 0.35f;

        public Action OnClick;

        private Button _button;
        private RectTransform _frameRect;
        private Vector2 _frameOriginAnchoredPosition;
        private Vector2 _iconOriginAnchoredPosition;
        private Vector3 _iconOriginScale;

        private Sequence _focusSequence = null;
        private Sequence _unfocusSequence = null;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
                _button.onClick.AddListener(() => OnClick?.Invoke());

            _frameRect = frame != null ? frame : GetComponent<RectTransform>();
            if (_frameRect != null)
                _frameOriginAnchoredPosition = _frameRect.anchoredPosition;

            if (icon != null)
            {
                _iconOriginAnchoredPosition = icon.rectTransform.anchoredPosition;
                _iconOriginScale = icon.rectTransform.localScale;
            }
        }

        public void Init()
        {
        }

        public void Focus(bool instant = false)
        {
            KillSequences();

            if (instant)
            {
                ApplyState(true);
                return;
            }

            _focusSequence = DOTween.Sequence();

            if (_frameRect != null)
                _focusSequence.Join(_frameRect
                    .DOAnchorPos(_frameOriginAnchoredPosition + (Vector2)focusFramePosition, duration));

            if (icon != null)
            {
                _focusSequence.Join(icon.rectTransform
                    .DOAnchorPos(_iconOriginAnchoredPosition + (Vector2)focusIconPosition, duration));
                _focusSequence.Join(icon.rectTransform
                    .DOScale(_iconOriginScale * focusIconScale, duration));
            }
        }

        public void Unfocus(bool instant = false)
        {
            KillSequences();

            if (instant)
            {
                ApplyState(false);
                return;
            }

            _unfocusSequence = DOTween.Sequence();

            if (_frameRect != null)
                _unfocusSequence.Join(_frameRect
                    .DOAnchorPos(_frameOriginAnchoredPosition + (Vector2)unfocusFramePosition, duration));

            if (icon != null)
            {
                _unfocusSequence.Join(icon.rectTransform
                    .DOAnchorPos(_iconOriginAnchoredPosition + (Vector2)unfocusIconPosition, duration));
                _unfocusSequence.Join(icon.rectTransform
                    .DOScale(_iconOriginScale * unfocusIconScale, duration));
            }
        }

        /// <summary>Đặt ngay trạng thái focus/unfocus, không chạy tween.</summary>
        private void ApplyState(bool focused)
        {
            if (_frameRect != null)
            {
                _frameRect.anchoredPosition = _frameOriginAnchoredPosition +
                    (Vector2)(focused ? focusFramePosition : unfocusFramePosition);
            }

            if (icon != null)
            {
                icon.rectTransform.anchoredPosition = _iconOriginAnchoredPosition +
                    (Vector2)(focused ? focusIconPosition : unfocusIconPosition);
                icon.rectTransform.localScale = _iconOriginScale *
                    (focused ? focusIconScale : unfocusIconScale);
            }
        }

        private void KillSequences()
        {
            _focusSequence?.Kill();
            _unfocusSequence?.Kill();
        }

        public void OnClickButton()
        {
            OnClick?.Invoke();
        }
    }
}
