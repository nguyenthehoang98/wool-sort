namespace _Game.Scripts.Core
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Đặt sẵn vào EntryScene (hoặc scene đầu tiên). Tự ensure AudioService, load AudioConfig từ Resources,
    /// và phát BGM phù hợp khi chuyển scene Home / Gameplay.</summary>
    public class AudioBootstrap : MonoBehaviour
    {
        [Header("BGM theo scene (nếu không gán sẽ lấy từ AudioConfig)")]
        [SerializeField] private AudioClip bgmHome;
        [SerializeField] private AudioClip bgmGameplay;

        private static bool _booted;

        private void Awake()
        {
            // Chỉ boot 1 lần (DontDestroyOnLoad).
            if (_booted) { Destroy(gameObject); return; }
            _booted = true;
            DontDestroyOnLoad(gameObject);

            // Đảm bảo AudioService có sẵn.
            var audioService = AudioService.EnsureInstance();
            if (audioService.Config == null)
            {
                var cfg = Resources.Load<AudioConfigAsset>("AudioConfig");
                if (cfg != null) audioService.SetConfig(cfg.config);
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var audioService = AudioService.Instance;
            if (audioService == null || audioService.EffectiveConfig == null) return;

            var clip = scene.name.Equals("Home") ? (bgmHome ?? audioService.EffectiveConfig.bgmHome)
                 : scene.name.Equals("Gameplay") ? (bgmGameplay ?? audioService.EffectiveConfig.bgmGameplay)
                 : null;

            if (clip != null) audioService.PlayMusic(clip, loop: true, persist: true);
        }
    }
}
