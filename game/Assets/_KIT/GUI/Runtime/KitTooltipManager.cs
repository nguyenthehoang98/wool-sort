using System.Collections;
using _KIT;
using UnityEngine;

namespace _KIT
{
    public class KitTooltipManager : MonoBehaviour
    {
        public static KitTooltipManager Instance { get; private set; }

        [SerializeField] private Canvas canvas;
        [SerializeField] private Transform parent;
        [SerializeField] private KitTooltip kitTooltip;

        private KitTooltip currentItem;
        private float lastTimeShowTooltip;
        private float defaultPosY = 0;

        private void Awake()
        {
            // Singleton: scene nào cũng có thể chứa 1 instance. Instance đã tồn tại
            // (ví dụ do EntryScene tạo và DontDestroyOnLoad sang Home) thì giữ instance đó,
            // bản mới tự huỷ để không có 2 tooltip cùng lúc.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            KitPool.RegisterPool(kitTooltip.gameObject, 0);
            KitPool.Destroy(kitTooltip.gameObject);
            StartCoroutine(UpdateCameraCanvasIE());
        }

        private IEnumerator UpdateCameraCanvasIE()
        {
            while (true)
            {
                yield return new WaitForSeconds(2);
                if (canvas.worldCamera == null)
                {
                    canvas.worldCamera = Camera.main;
                }
            }
        }

        public void Show(string content, float duration = 3f)
        {
            if (lastTimeShowTooltip > Time.time)
                return;

            lastTimeShowTooltip = Time.time + 1;

            KitTooltip item = KitPool.Instantiate<KitTooltip>(kitTooltip.gameObject);
            item.transform.parent = parent.transform;

            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.localScale = Vector3.one;
            itemRect.anchorMin = new Vector2(0, 0.5f);
            itemRect.anchorMax = new Vector2(1, 0.5f);
            itemRect.pivot = new Vector2(0.5f, 0.5f);
            itemRect.anchoredPosition3D = new Vector3(0, defaultPosY, 0);
            itemRect.sizeDelta = new Vector2(0, 100);
            item.gameObject.SetActive(true);
            item.Show(content, duration);

            currentItem = item;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void ForceHideCurrentToolTip()
        {
            if (currentItem != null)
            {
                currentItem.gameObject.SetActive(false);
            }
        }
    }
}