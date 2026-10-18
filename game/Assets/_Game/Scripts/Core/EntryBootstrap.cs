namespace _Game.Scripts.Core
{
    using UnityEngine;

    /// <summary>
    /// Đặt vào GameObject duy nhất trong scene Entry (đánh DontDestroyOnLoad).
    /// Đảm bảo EconomyService, ShopService, EntryFlow đã tồn tại trước khi chuyển
    /// sang Home/Gameplay; nếu scene đã có rồi thì không tạo thêm.
    /// </summary>
    public sealed class EntryBootstrap : MonoBehaviour
    {
        [SerializeField] private bool createServices = true;

        private void Awake()
        {
            if (!createServices) return;

            if (EconomyService.Instance == null)
                new GameObject("EconomyService").AddComponent<EconomyService>();
            if (ShopService.Instance == null)
                new GameObject("ShopService").AddComponent<ShopService>();
            if (EntryFlow.Instance == null)
                new GameObject("EntryFlow").AddComponent<EntryFlow>();
        }
    }
}
