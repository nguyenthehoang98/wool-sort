namespace _Game.Scripts.Core
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Phát BGM theo scene (Home/Gameplay). Tự boot ở MỌI scene — không phụ thuộc việc scene
    /// có đặt sẵn component hay không, và phát lại BGM mỗi khi load scene khác.
    /// </summary>
    public class AudioBootstrap : MonoBehaviour
    {
        [Header("BGM theo scene (nếu không gán sẽ lấy từ AudioConfig)")]
        [SerializeField] private AudioClip bgmHome;
        [SerializeField] private AudioClip bgmGameplay;

        private static AudioBootstrap _instance;

        // Reset static khi vào Play mode (kể cả khi Domain Reload tắt) — nếu không, giá trị
        // cũ còn sót lại làm lần Play sau không boot được BGM.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _instance = null;

        // Tự tạo AudioBootstrap nếu scene hiện tại không có, rồi phát BGM của scene đang mở.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBoot()
        {
            var existing = FindFirstObjectByType<AudioBootstrap>();
            if (existing != null)
            {
                _instance = existing;
                existing.PlayBgm(SceneManager.GetActiveScene());
                return;
            }

            var go = new GameObject("AudioBootstrap");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<AudioBootstrap>(); // Awake chạy ngay, giữ DontDestroyOnLoad
            _instance.PlayBgm(SceneManager.GetActiveScene());
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureAudioService();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (_instance == this) _instance = null;
        }

        private void EnsureAudioService()
        {
            var audioService = AudioService.EnsureInstance();
            if (audioService.Config == null)
            {
                var cfg = Resources.Load<AudioConfig>("AudioConfig");
                if (cfg != null) audioService.SetConfig(cfg);
                else Debug.LogWarning("[AudioBootstrap] AudioConfig not found in Resources!");
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => PlayBgm(scene);

        /// <summary>Phát BGM khớp tên scene (Home → bgmHome, Gameplay → bgmGameplay).</summary>
        private void PlayBgm(Scene scene)
        {
            var audioService = AudioService.Instance;
            if (audioService == null) return;

            var cfg = audioService.EffectiveConfig;
            var clip = scene.name.Equals("Home") ? (bgmHome != null ? bgmHome : cfg?.bgmHome)
                     : scene.name.Equals("Gameplay") ? (bgmGameplay != null ? bgmGameplay : cfg?.bgmGameplay)
                     : null;

            if (clip != null) audioService.PlayMusic(clip, loop: true, persist: true);
        }
    }
}
