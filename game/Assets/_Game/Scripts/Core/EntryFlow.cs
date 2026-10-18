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
        /// Level 1 đi thẳng Gameplay; từ level 2 trở đi vào Home (theo spec progression).
        /// </summary>
        [ContextMenu("Load Target Scene Now")]
        public void LoadTargetScene()
        {
            var level = _economy != null ? _economy.GetLevel() : 1;
            var target = level == 1 ? gameplaySceneName : homeSceneName;
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
