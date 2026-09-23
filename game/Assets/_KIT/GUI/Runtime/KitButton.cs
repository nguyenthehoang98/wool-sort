using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _KIT
{
    /// <summary>
    /// Button có trạng thái bật/tắt (select/deselect)
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class KitButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private static HashSet<KitButton> MemoryButtons { get; set; } = new HashSet<KitButton>();

        [SerializeField] private ButtonHighlightMode highlightMode = ButtonHighlightMode.ActiveObject;
        [SerializeField] private ButtonMode buttonMode = ButtonMode.Button;
        [SerializeField] private string group = "";
        [SerializeField] private Vector2 scaleTarget = new Vector2(1.25f, 1.25f);
        [SerializeField] private float scaleDuration = 0.15f;
        [SerializeField] private Color pressedColor = new Color32(200, 200, 200, 255);

        public RectTransform ActiveRectTransform { get; private set; }
        public RectTransform InactiveRectTransform { get; private set; }
        public event UnityAction<bool> OnChecked;
        public event UnityAction OnClicked;

        public bool IsChecked { get; private set; }

        private readonly Color normalColor = Color.white;
        private Transform activeContainer;
        private Transform inactiveContainer;
        private Vector2 originScale;
        private Image buttonImage;
        private bool pressing;

        private bool hasStart = false;
        private DOTweenAnimation _dotweenAnim;

        private void Awake()
        {
            activeContainer = transform.Find("active");
            if (activeContainer != null) ActiveRectTransform = activeContainer.GetComponent<RectTransform>();
#if UNITY_EDITOR
            if (activeContainer == null)
                Debug.LogWarning($"Not found GameObject with name 'active' inside parent {name}");
#endif
            inactiveContainer = transform.Find("inactive");
            if (inactiveContainer != null) InactiveRectTransform = inactiveContainer.GetComponent<RectTransform>();
#if UNITY_EDITOR
            if (inactiveContainer == null)
                Debug.LogWarning($"Not found GameObject with name 'deactive' inside parent {name}");
#endif
            SetContainerActive(false);

            if (transform.childCount > 0)
            {
                buttonImage = transform.GetChild(0).GetComponent<Image>();
            }

            Button button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                //GameSound.Instance.TapUISfx();
                // HapticManager.Instance.TapUIHaptic();
                if (!enabled) return;
                switch (buttonMode)
                {
                    case ButtonMode.Button:
                        OnClicked?.Invoke();
                        break;
                    case ButtonMode.Toggle:
                        IsChecked = !IsChecked;
                        SetContainerActive(IsChecked);
                        OnChecked?.Invoke(IsChecked);
                        break;
                    case ButtonMode.RadioButton:
                        IsChecked = true;
                        SetContainerActive(true);
                        OnChecked?.Invoke(true);
                        KitButton[] allButtons = MemoryButtons.ToArray();
                        foreach (KitButton kitButton in allButtons)
                        {
                            if (kitButton == null || kitButton == this) continue;
                            bool match = kitButton.buttonMode == ButtonMode.RadioButton;
                            match = match && !string.IsNullOrEmpty(group) && !string.IsNullOrEmpty(kitButton.group);
                            match = match && kitButton.group.Equals(group);
                            if (match)
                            {
                                kitButton.IsChecked = false;
                                kitButton.SetContainerActive(false);
                                kitButton.OnChecked?.Invoke(false);
                            }
                        }

                        break;
                }
            });

            _dotweenAnim = GetComponent<DOTweenAnimation>();
        }

        private void Start()
        {
            originScale = transform.localScale;

            hasStart = true;

            scaleTarget = new Vector2(0.9f, 0.9f);
            scaleDuration = 0.1f;
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            activeContainer = transform.Find("active");
            inactiveContainer = transform.Find("inactive");
#endif
        }

        private void OnEnable()
        {
            MemoryButtons.Add(this);

            if (highlightMode != ButtonHighlightMode.ScaleObject)
                return;

            if (hasStart && originScale != Vector2.zero)
            {
                transform.localScale = originScale;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Button button = GetComponent<Button>();
            if (!button.interactable) return;

            if (highlightMode == ButtonHighlightMode.Animation)
            {
                SetContainerAnimation(true);
                pressing = true;
            }
            else if (highlightMode == ButtonHighlightMode.TintColor)
            {
                SetContainerColor(true);
                pressing = true;
            }
            else
            {
                SetContainerScale(true);
                pressing = true;
                switch (buttonMode)
                {
                    case ButtonMode.Button:
                        SetContainerActive(true);
                        break;
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Button button = GetComponent<Button>();
            if (!button.interactable) return;

            if (highlightMode == ButtonHighlightMode.Animation)
            {
                SetContainerAnimation(false);
                pressing = false;
            }
            else if (highlightMode == ButtonHighlightMode.TintColor)
            {
                SetContainerColor(false);
                pressing = false;
            }
            else
            {
                SetContainerScale(false);
                pressing = false;
                switch (buttonMode)
                {
                    case ButtonMode.Button:
                        SetContainerActive(false);
                        break;
                }
            }
        }

        /// <summary>
        /// Hàm cung cấp phương thức thay đổi visual & trạng thái của button (Toggle) nhưng không bắn event
        /// </summary>
        public void SetToggle(bool value, bool ignoreCheckMode = false)
        {
            if (buttonMode != ButtonMode.Toggle)
                return;
            IsChecked = value;
            SetContainerActive(IsChecked, ignoreCheckMode);
        }

        public void SetRadioButton(bool value)
        {
            if (buttonMode != ButtonMode.RadioButton)
                return;
            IsChecked = value;
            SetContainerActive(value);
        }

        private void OnDisable()
        {
            MemoryButtons.Remove(this);
        }

        private void SetContainerActive(bool value, bool ignoreCheckMode = false)
        {
            if (highlightMode != ButtonHighlightMode.ActiveObject && !ignoreCheckMode)
                return;
            if (activeContainer != null)
                activeContainer.gameObject.SetActive(value);
            if (inactiveContainer != null)
                inactiveContainer.gameObject.SetActive(!value);
        }

        public void ForceSwitchVisual(bool active)
        {
            if (activeContainer != null)
                activeContainer.gameObject.SetActive(active);
            if (inactiveContainer != null)
                inactiveContainer.gameObject.SetActive(!active);
        }

        private void SetContainerScale(bool value)
        {
            if (highlightMode != ButtonHighlightMode.ScaleObject)
                return;
            if (!pressing && value)
            {
                StartCoroutine(DoScaleIE(transform, scaleTarget, scaleDuration, value));
            }
            if (pressing && !value)
            {
                StartCoroutine(DoScaleIE(transform, originScale, scaleDuration, value));
            }
        }

        private IEnumerator DoScaleIE(Transform target, Vector3 scaleValue, float duration, bool isDown)
        {
            if (isDown && _dotweenAnim != null)
            {
                _dotweenAnim.DOPause();
            }

            Vector3 initialScale = transform.localScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime; // Cập nhật thời gian
                float t = elapsed / duration; // Tính tỷ lệ (từ 0 đến 1)
                target.localScale = Vector3.Lerp(initialScale, scaleValue, t); // Nội suy kích thước
                yield return null; // Chờ frame tiếp theo
            }

            // Đảm bảo giá trị cuối cùng chính xác
            transform.localScale = scaleValue;

            if (!isDown && _dotweenAnim != null)
            {
                _dotweenAnim.DORestart();
            }
        }

        private void SetContainerAnimation(bool value)
        {
            if (highlightMode != ButtonHighlightMode.Animation)
                return;
            if (!pressing && value)
            {
                GetComponent<Animator>().Play("Button_P1");
            }
            if (pressing && !value)
            {
                GetComponent<Animator>().Play("Button_P2");
            }
        }

        private void SetContainerColor(bool value)
        {
            if (highlightMode != ButtonHighlightMode.TintColor)
                return;
            if (!pressing && value && buttonImage != null)
            {
                buttonImage.color = pressedColor;
            }
            if (pressing && !value && buttonImage != null)
            {
                buttonImage.color = normalColor;
            }
        }

        internal enum ButtonMode
        {
            Button,
            RadioButton,
            Toggle,
        }

        internal enum ButtonHighlightMode
        {
            ActiveObject,
            ScaleObject,
            Animation,
            TintColor
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(KitButton))]
    [CanEditMultipleObjects]
    public class KitButtonInspector : UnityEditor.Editor
    {
        private SerializedProperty scaleTargetProperty;
        private SerializedProperty scaleDurationProperty;
        private SerializedProperty modeProperty;
        private SerializedProperty groupProperty;
        private SerializedProperty highlightModeProperty;
        private SerializedProperty pressedColorProperty;

        public virtual void OnEnable()
        {
            scaleTargetProperty = serializedObject.FindProperty("scaleTarget");
            scaleDurationProperty = serializedObject.FindProperty("scaleDuration");
            modeProperty = serializedObject.FindProperty("buttonMode");
            groupProperty = serializedObject.FindProperty("group");
            highlightModeProperty = serializedObject.FindProperty("highlightMode");
            pressedColorProperty = serializedObject.FindProperty("pressedColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(highlightModeProperty);
            switch ((KitButton.ButtonHighlightMode)highlightModeProperty.enumValueIndex)
            {
                case KitButton.ButtonHighlightMode.ActiveObject:
                    break;
                case KitButton.ButtonHighlightMode.ScaleObject:
                    EditorGUILayout.PropertyField(scaleTargetProperty);
                    EditorGUILayout.PropertyField(scaleDurationProperty);
                    break;
                case KitButton.ButtonHighlightMode.TintColor:
                    EditorGUILayout.PropertyField(pressedColorProperty);
                    break;
            }
            EditorGUILayout.Space(15);

            EditorGUILayout.PropertyField(modeProperty);
            switch ((KitButton.ButtonMode)modeProperty.enumValueIndex)
            {
                case KitButton.ButtonMode.Button:
                    break;
                case KitButton.ButtonMode.RadioButton:
                    EditorGUILayout.PropertyField(groupProperty);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}