// YarnPart.cs

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Touch;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEventBus;

namespace _Game.Scripts.Gameplay
{
    public enum InnerColorType { Random, Fixed }

    public class YarnPart : MonoBehaviour
    {
        [Header("Chọn màu khởi tạo (Editor & Runtime)")]
        public YarnColor initColor;

        [Header("––– Chỉ cấu hình trên LỚP GỐC –––")]
        [Tooltip("Số lớp con tối đa (không kể lớp gốc)")]
        public int totalInnerCount = 0; // default là không có lớp con nào

        [Header("Cấu hình từng inner layer")]
        public List<InnerColorType> innerColorTypes = new List<InnerColorType>();
        public List<YarnColor> innerFixedColors = new List<YarnColor>();

        // do Spawner gán
        [HideInInspector] public YarnPart ancestor; // gốc đầu tiên
        [HideInInspector] public YarnPart rootPart; // lớp ngay trước
        [HideInInspector] public YarnPart childPart; // lớp ngay sau
        public int layerIndex = 1; //1 là layer ngoài cùng, sau đó tăng tiến dần

        private MeshRenderer _renderer;
        private YarnColor _currentColor;
        public YarnColor CurrentColor => _currentColor;

        public float DissolveProgress { get; private set; }

        private float dissolveDuration;

        [Header("Decoration Settings")]
        [Tooltip("Child decorations (chứa MeshRenderer + Collider)")]
        public List<YarnDecoration> decorations;

        public bool IsDissolving { get; private set; }

        MeshRenderer meshRenderer;
        MaterialPropertyBlock mpb;
        static readonly int ProgressID = Shader.PropertyToID("_Dissolve");
        static readonly int CutoffID = Shader.PropertyToID("_Cutoff");
        static readonly int WipeAxisID = Shader.PropertyToID("_WipeAxis");
        static readonly int WipeMinID = Shader.PropertyToID("_WipeMin");
        static readonly int WipeMaxID = Shader.PropertyToID("_WipeMax");

        // Wipe dissolve theo TRỤC CAO NHẤT của mesh (object space, shader
        // YarnDissolve riêng): tự đo bounds từng mesh trong Awake nên model
        // nào cũng wipe hết đúng thời gian, hướng nhìn rõ nhất.
        float wipeAxis = 1f;
        float wipeMin = 0f;
        float wipeMax = 1f;

        // Struct để track threshold mỗi decoration
        struct DecoInfo { public YarnDecoration deco; public float threshold; }
        List<DecoInfo> _decoInfos;
        bool _initialized;

        // thêm field để cache layer mask
        private int yarnPartLayerMask;

        private float holdThreshold;
        private float fingerDownTime;
        private bool isHolding = false;
        private LeanFinger currentFinger;
        private MeshCollider meshCollider;

        private Coroutine holdCoroutine;
        private Coroutine dissolveRoutine;

        // flag báo đã pop xong → không cần grow nữa
        public bool IsPopping { get; private set; }

        void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            ApplyColor(initColor);

            meshCollider = GetComponent<MeshCollider>();

            yarnPartLayerMask = 1 << LayerMask.NameToLayer("YarnPart");

            meshRenderer = GetComponent<MeshRenderer>();
            // Tự đo trục cao nhất của mesh (kể cả part clone từ Instantiate)
            var meshFilter = GetComponent<MeshFilter>();
            var sharedMesh = meshFilter != null ? meshFilter.sharedMesh : null;
            if (sharedMesh != null)
            {
                Vector3 size = sharedMesh.bounds.size;
                Vector3 min = sharedMesh.bounds.min;
                Vector3 max = sharedMesh.bounds.max;
                if (size.y >= size.x && size.y >= size.z) { wipeAxis = 1f; wipeMin = min.y; wipeMax = max.y; }
                else if (size.x >= size.z) { wipeAxis = 0f; wipeMin = min.x; wipeMax = max.x; }
                else { wipeAxis = 2f; wipeMin = min.z; wipeMax = max.z; }
                if (wipeMax - wipeMin < 0.0001f) { wipeAxis = 1f; wipeMin = 0f; wipeMax = 1f; }
            }
            mpb = new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat(ProgressID, 1f);
            mpb.SetFloat(CutoffID, 0f);
            mpb.SetFloat(WipeAxisID, wipeAxis);
            mpb.SetFloat(WipeMinID, wipeMin);
            mpb.SetFloat(WipeMaxID, wipeMax);
            meshRenderer.SetPropertyBlock(mpb);

            // Nếu chưa assign decorations, tự tìm tất cả YarnDecoration con
            if (decorations == null || decorations.Count == 0)
                decorations = new List<YarnDecoration>(GetComponentsInChildren<YarnDecoration>());

            // Khởi tạo threshold list
            _decoInfos = new List<DecoInfo>();
            var bounds = meshRenderer.localBounds;
            foreach (var deco in decorations)
            {
                if (deco == null)
                {
                    Debug.LogError($"[Decor] {name}: decorations list chứa slot NULL (kéo thiếu trong Inspector).", this);
                    continue;
                }
                // Convert world position của deco sang local của part
                Vector3 localPos = transform.InverseTransformPoint(deco.transform.position);
                // Normalized Y trong [0,1]
                float normY = Mathf.InverseLerp(bounds.min.y, bounds.max.y, localPos.y);
                // Khi progress ≤ normY thì drop
                _decoInfos.Add(new DecoInfo { deco = deco, threshold = normY });
                Debug.Log($"[DecorDbg] {name}: deco {deco.name} localY={localPos.y:F3} bounds=[{bounds.min.y:F3},{bounds.max.y:F3}] threshold={normY:F3}", deco);
                if (normY < 0f || normY > 1f)
                    Debug.LogError($"[Decor] {name} -> {deco.name}: threshold={normY:F2} ngoài [0,1] (decor nằm ngoài bounds len). Nó sẽ rơi trễ (cuối dissolve) hoặc ngay frame đầu. Dời decor sát mặt len hơn.", deco);
                if (deco.GetComponent<Collider>() == null)
                    Debug.LogError($"[Decor] {deco.name} (con của {name}): thiếu Collider nên tap/dissolve vẫn chạy nhưng vật lý rơi có thể sai.", deco);
                if (deco.GetComponent<Rigidbody>() == null)
                    Debug.LogError($"[Decor] {deco.name} (con của {name}): thiếu Rigidbody (runtime sẽ tự Add, nhưng nên gắn sẵn, isKinematic=true).", deco);
            }
            if (_decoInfos.Count == 0)
                Debug.Log($"[DecorDbg] {name}: KHÔNG có YarnDecoration con nào (layerIndex={layerIndex}). Nếu đây là layer 1 mà bạn mong có decor rơi thì decor đang mồ côi hoặc là SpecialDecor.", this);

            dissolveDuration = GameManager.Instance.GameConfig.dissolveDuration;
            holdThreshold = GameManager.Instance.GameConfig.holdThreshold;
            DissolveProgress = 1;
            _initialized = true;
        }

        /// <summary>
        /// Gọi ngay sau khi Instantiate để setup đúng layer và scale+color
        /// </summary>
        public void Initialize(YarnPart root, int layerIndex, float scale, YarnColor yarnColor)
        {
            this.ancestor = root.ancestor;
            this.rootPart = root; // chưa sử dụng
            this.layerIndex = layerIndex;
            transform.localScale = ancestor.transform.localScale * scale;
            transform.localRotation = root.transform.localRotation;

            initColor = yarnColor;
            gameObject.SetActive(true);
            meshCollider.enabled = true;
            ApplyColor(initColor);
        }

        void OnEnable()
        {
            LeanTouch.OnFingerTap += HandleFingerTap;
            LeanTouch.OnFingerDown += HandleFingerDown;
            LeanTouch.OnFingerUp += HandleFingerUp;
        }

        void OnDisable()
        {
            LeanTouch.OnFingerTap -= HandleFingerTap;
            LeanTouch.OnFingerDown -= HandleFingerDown;
            LeanTouch.OnFingerUp -= HandleFingerUp;
        }

        private void HandleFingerTap(LeanFinger finger)
        {
            if (Input.touchCount > 1) return;
            if (!GameManager.Instance.CanInteractWithGame()) return;
            if (finger.IsOverGui) return;
            if (IsDissolving || dissolveRoutine != null) return;

            // Debug.Log("Handle Finger Tap");
            Ray ray = Camera.main.ScreenPointToRay(finger.ScreenPosition);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, yarnPartLayerMask))
            {
                // Nếu tap lên part hoặc decoration con
                var part = hit.collider.GetComponentInParent<YarnPart>();
                if (part == this)
                {
                    var slot = SlotManager.Instance.GetSlotToRope(this);
                    if (slot == null)
                    {
                        Debug.LogWarning("Không tìm thấy slot nào để spawn rope");
                        return;
                    }

                    if (!GameManager.Instance.TryBeginYarnTap() ||
                        !SlotManager.Instance.SpawnRope(this, slot)) return;

                    IsDissolving = true;
                    Debug.Log("Handle Finger Tap " + part.name);
                    GlobalBus.Send<YarnClickedMessage>(new YarnClickedMessage(this));
                    YarnPartLayerSpawner.Instance.SpawnInnerOnClick(this);

                    PopEffect(() =>
                    {
                        if (dissolveRoutine == null)
                        {
                            dissolveRoutine = GameManager.Instance.StartCoroutine(
                                DissolveAndDrop(
                                    true)); // sử dụng game manager để cố fix bug không dissolve được thi thoảng xảy ra
                        }
                    });
                }
            }
        }

        public void ForceDissolve(bool needGrowInner)
        {
            if (IsDissolving || dissolveRoutine != null) return;

            IsDissolving = true;
            gameObject.SetActive(true);
            dissolveRoutine = GameManager.Instance.StartCoroutine(DissolveAndDrop(needGrowInner)); // sử dụng game manager để cố fix bug không dissolve được thi thoảng xảy ra
        }

        private void HandleFingerDown(LeanFinger finger)
        {
            if (Input.touchCount > 1) return;
            if (!GameManager.Instance.CanInteractWithGame()) return;
            if (finger.IsOverGui) return;
            if (IsDissolving) return;

            Ray ray = Camera.main.ScreenPointToRay(finger.ScreenPosition);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, yarnPartLayerMask))
            {
                // Nếu tap lên part hoặc decoration con
                var part = hit.collider.GetComponentInParent<YarnPart>();
                if (part == this)
                {
                    currentFinger = finger;
                    fingerDownTime = Time.time;
                    holdCoroutine = StartCoroutine(CheckHold());
                }
            }
        }

        private void HandleFingerUp(LeanFinger finger)
        {
            if (finger != currentFinger) return;

            float duration = Time.time - fingerDownTime;

            if (holdCoroutine != null)
            {
                StopCoroutine(holdCoroutine);
                holdCoroutine = null;
            }

            if (duration > holdThreshold)
            {
                // Hold → reset về material cũ
                _renderer.material = YarnColorManager.Instance.GetYarnMaterial(_currentColor);
                isHolding = false;

                // ẩn child part
                if (childPart != null)
                {
                    childPart.gameObject.SetActive(false);
                }

                // đã hold thành công
                GlobalBus.Send<YarnHoldedMessage>(new YarnHoldedMessage(this));
            }

            currentFinger = null;
        }

        private IEnumerator CheckHold()
        {
            yield return new WaitForSeconds(holdThreshold);

            // Sau ngưỡng thời gian, nếu vẫn giữ thì xử lý hold
            if (currentFinger != null)
            {
                _renderer.material = YarnColorManager.Instance.GetTransparentMaterial();
                isHolding = true;

                // show child part
                if (childPart != null)
                {
                    childPart.gameObject.SetActive(true);
                }

                GlobalBus.Send<YarnStartHoldMessage>(new YarnStartHoldMessage(this));
            }
        }

        public void ForceVisualHold()
        {
            _renderer.material = YarnColorManager.Instance.GetTransparentMaterial();
            if (childPart != null)
            {
                childPart.gameObject.SetActive(true);
            }
        }

        public void ForceVisualRelease()
        {
            _renderer.material = YarnColorManager.Instance.GetYarnMaterial(_currentColor);
            if (childPart != null)
            {
                childPart.gameObject.SetActive(false);
            }
        }

        private void PopEffect(Action callback)
        {
            IsPopping = true;
            Debug.Log("Pop effect của " + gameObject.name);
            var original = transform.localScale;
            DOTween.Sequence()
                .Append(transform.DOScale(original * 1.05f, 0.05f).SetEase(Ease.OutSine))
                .Append(transform.DOScale(original, 0.05f).SetEase(Ease.InSine))
                .OnComplete(() =>
                {
                    IsPopping = false;
                    callback?.Invoke();
                });
        }

        public IEnumerator DissolveAndDrop(bool needGrowInner)
        {
            Debug.Log("DissolveAndDrop - 1 của " + gameObject.name);

            // yield return new WaitForEndOfFrame();

            meshCollider.enabled = false;

            float t = 0f;
            while (t < dissolveDuration)
            {
                t += Time.deltaTime;
                float progress = Mathf.Clamp01(1f - t / dissolveDuration);
                mpb.SetFloat(ProgressID, progress);
                mpb.SetFloat(CutoffID, 1f - progress);
                meshRenderer.SetPropertyBlock(mpb);
                DissolveProgress = progress;

                // Check each decoration
                for (int i = _decoInfos.Count - 1; i >= 0; i--)
                {
                    var info = _decoInfos[i];
                    if (info.deco == null) { _decoInfos.RemoveAt(i); continue; }
                    if (progress <= info.threshold)
                    {
                        Debug.Log($"[DecorDbg] {name}: progress={progress:F2} <= threshold={info.threshold:F2} -> Drop {info.deco.name}", info.deco);
                        info.deco.Drop();
                        _decoInfos.RemoveAt(i);
                    }
                }
                yield return null;
            }

            if (needGrowInner)
            {
                YarnPartLayerSpawner.Instance.HandleLayerDissolved(this);
            }

            // Gọi drop cho mọi deco còn lại
            foreach (var info in _decoInfos)
                info.deco.Drop();
            _decoInfos.Clear();

            Debug.Log("DissolveAndDrop - 2 của " + gameObject.name);

            // Ẩn hết phần len
            mpb.SetFloat(ProgressID, 0f);
            mpb.SetFloat(CutoffID, 1f);
            meshRenderer.SetPropertyBlock(mpb);
            DissolveProgress = 0;

            // nếu đây là layer cuối (không spawn thêm inner)
            if (layerIndex > totalInnerCount || childPart == null)
                GlobalBus.Send<YarnLastLayerCleared>(new YarnLastLayerCleared(this));

            gameObject.SetActive(false);
            IsDissolving = false;
            dissolveRoutine = null;
        }

        public void ClearDeco()
        {
            decorations.Clear();
            _decoInfos?.Clear();
        }

        /// <summary>
        /// Đổi màu runtime (khi game đang chạy)
        /// </summary>
        public void SetColor(YarnColor newColor)
        {
            ApplyColor(newColor);
        }

        private void ApplyColor(YarnColor color)
        {
            if (_renderer == null) _renderer = GetComponent<MeshRenderer>();

            _currentColor = color;
            var mat = YarnColorManager.Instance.GetYarnMaterial(color);
            if (mat == null)
            {
                // Không có material dissolve-capable -> SetFillAmount(_Dissolve/_Cutoff)
                // sẽ thành no-op thầm lặng, model không bao giờ tan dần!
                Debug.LogWarning($"YarnPart {name}: thiếu yarn material cho màu {color} — dissolve sẽ không hiển thị!", this);
                return;
            }
            // Luôn gán (kể cả trùng màu): renderer phải dùng đúng ThreadMat có
            // _Dissolve/_Cutoff. Bản cũ return sớm khi _currentColor == color
            // (mặc định đã là Blue) khiến part Blue giữ material prefab
            // không-dissolve -> bấm không bao giờ thấy tan dần.
            _renderer.material = mat;
        }

        public void SetTransparent()
        {
            var mat = YarnColorManager.Instance.GetTransparentMaterial();
            if (mat != null)
                _renderer.material = mat;
        }

        public void ShowChildPart(bool value)
        {
            if (childPart != null)
            {
                childPart.gameObject.SetActive(value);
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            // 0) Tự sửa lỗi hay gặp ở Editor: ancestor null -> decor Special không bao giờ rơi
            if (layerIndex == 1 && ancestor == null)
            {
                ancestor = this;
                rootPart = this;
                EditorUtility.SetDirty(this);
                Debug.LogWarning($"{name}: ancestor null đã tự gán = chính nó. Kiểm tra lại bằng Tools/Puzzle/Validate decor gắn len.", this);
            }
            // 0b) Tự gom decor con để tránh decorations rỗng khi quên kéo vào list
            var foundDecos = GetComponentsInChildren<YarnDecoration>(true);
            if ((decorations == null || decorations.Count == 0) && foundDecos.Length > 0)
            {
                decorations = new List<YarnDecoration>(foundDecos);
                EditorUtility.SetDirty(this);
                Debug.LogWarning($"{name}: decorations rỗng đã tự gom {foundDecos.Length} con. Nên lưu prefab.", this);
            }
            // 1) Ensure inner-lists always match totalInnerCount:
            if (innerColorTypes == null) innerColorTypes = new List<InnerColorType>();
            if (innerFixedColors == null) innerFixedColors = new List<YarnColor>();

            // Grow
            while (innerColorTypes.Count < totalInnerCount) innerColorTypes.Add(InnerColorType.Random);
            while (innerFixedColors.Count < totalInnerCount) innerFixedColors.Add(initColor);
            // Shrink
            while (innerColorTypes.Count > totalInnerCount) innerColorTypes.RemoveAt(innerColorTypes.Count - 1);
            while (innerFixedColors.Count > totalInnerCount) innerFixedColors.RemoveAt(innerFixedColors.Count - 1);


            // 1) đảm bảo có renderer
            if (_renderer == null)
                _renderer = GetComponent<MeshRenderer>();

            // 2) detect từ sharedMaterial → initColor
            if (_renderer.sharedMaterial != null &&
                YarnColorManager.Instance != null &&
                YarnColorManager.Instance.TryGetColorFromMaterial(_renderer.sharedMaterial, out var detectedColor))
            {
                // chỉ gán khi khác
                if (initColor != detectedColor)
                {
                    // 3) gán vào field để Unity serialize
                    initColor = detectedColor;

                    // 4) update luôn sharedMaterial để preview (nếu muốn)
                    _renderer.sharedMaterial = YarnColorManager.Instance.GetYarnMaterial(detectedColor);

                    // 5) đánh dấu dirty để Unity lưu giá trị initColor
                    EditorUtility.SetDirty(this);
                    if (!Application.isPlaying)
                        EditorSceneManager.MarkSceneDirty(gameObject.scene);
                }
            }

            // 3) Finally, tell the LevelYarnInfoHolder to recompute **all** its counts:
            var holder = transform.root.GetComponent<LevelYarnInfoHolder>();
            if (holder != null)
            {
                holder.UpdateDebugInfo();
                EditorUtility.SetDirty(holder);
                if (!Application.isPlaying)
                    EditorSceneManager.MarkSceneDirty(holder.gameObject.scene);
            }
        }
#endif
    }

    public class YarnClickedMessage
    {
        public YarnPart YarnPart { get; }
        public YarnClickedMessage(YarnPart part) { YarnPart = part; }
    }

    public class YarnLastLayerCleared
    {
        public YarnPart YarnPart { get; }
        public YarnLastLayerCleared(YarnPart part) { YarnPart = part; }
    }

    public class YarnStartHoldMessage
    {
        public YarnPart YarnPart { get; }
        public YarnStartHoldMessage(YarnPart part) { YarnPart = part; }
    }

    public class YarnHoldedMessage
    {
        public YarnPart YarnPart { get; }
        public YarnHoldedMessage(YarnPart part) { YarnPart = part; }
    }
}
