namespace _Game.Scripts.Core
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public sealed class EntryFlow : MonoBehaviour
    {
        public static EntryFlow Instance { get; private set; }

        [Header("Scenes")]
        [Tooltip("Scene Home (menu chính)")]
        [SerializeField] private string homeSceneName = "Home";
        [Tooltip("Scene Gameplay")]
        [SerializeField] private string gameplaySceneName = "Gameplay";

        [Header("Routing")]
        [Tooltip("Bật: Entry luôn vào Home (menu chính), Home bấm PLAY mới vào Gameplay. " +
                 "Tắt: level 1 đi thẳng Gameplay (đúng hành vi bản gốc), level từ 2 mới vào Home.")]
        [SerializeField] private bool alwaysGoHome = true;

        [Header("Auto-advance")]
        [SerializeField] private bool autoLoad = true;
        [SerializeField] private float autoLoadDelay = 0.75f;

        private EconomyService _economy;

        private void Awake()
        {
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
            _economy = EconomyService.Instance;
            if (_economy == null)
            {
                Debug.LogError("[EntryFlow] EconomyService chưa có trong scene Entry.");
                return;
            }

            _economy.Initialize();

            if (autoLoad)
                Invoke(nameof(LoadTargetScene), autoLoadDelay);
        }

        /// <summary>
        /// Entry là scene boot: mặc định luôn vào Home (menu chính), người chơi bấm PLAY ở Home
        /// mới vào Gameplay. Bật alwaysGoHome = false thì quay về hành vi bản gốc:
        /// level 1 đi thẳng Gameplay, level từ 2 trở đi vào Home.
        /// </summary>
        [ContextMenu("Load Target Scene Now")]
        public void LoadTargetScene()
        {
            var level = _economy != null ? _economy.GetLevel() : 1;
            var target = (!alwaysGoHome && level == 1) ? gameplaySceneName : homeSceneName;
            Debug.Log($"[EntryFlow] Level={level} -> chuyển vào {target}");
            if (!string.IsNullOrEmpty(target))
                SceneManager.LoadScene(target);
        }

        /// <summary>Gọi từ nút Start trên Home để vào Gameplay level hiện tại.</summary>
        public void StartCurrentLevel()
        {
            var level = _economy != null ? _economy.GetLevel() : 1;
            _economy.SetLevel(level);
            SceneManager.LoadScene(gameplaySceneName);
        }

        public void GoHome()
        {
            SceneManager.LoadScene(homeSceneName);
        }

        public int GetCurrentLevel() => _economy != null ? _economy.GetLevel() : 1;
    }
}
