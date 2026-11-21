namespace _Game.Scripts.Core
{
    using UnityEngine;

    /// <summary>
    /// Tạo các service singleton TRƯỚC khi bất kỳ scene nào Awake.
    ///
    /// Lý do: `EconomyService.Instance` được set trong Awake nhưng dữ liệu lại init
    /// ở Start → giữa 2 mốc đó mọi accessor đọc `_data` sẽ NRE. Scene nào gọi
    /// `EconomyService.Instance.GetLevel()` trong Awake của nó (vd `LevelList`)
    /// sẽ crash tuỳ thứ tự Awake — mà Unity KHÔNG bảo đảm thứ tự Awake.
    ///
    /// `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)` đảm bảo service tồn tại và
    /// đã init trước mọi Awake của scene. Các scene/service khác vẫn có thể gọi
    /// `ServiceBootstrap.EnsureEconomy()` / `EnsureShop()` — hàm idempotent.
    /// </summary>
    public static class ServiceBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            EnsureEconomy();
            EnsureShop();
            EnsureDailyReward();
        }

        /// <summary>Đảm bảo EconomyService tồn tại và đã init (trả về null nếu không tạo được).</summary>
        public static EconomyService EnsureEconomy()
        {
            if (EconomyService.Instance != null) return EconomyService.Instance;
            return new GameObject("EconomyService").AddComponent<EconomyService>();
        }

        /// <summary>Đảm bảo ShopService tồn tại (trả về null nếu không tạo được).</summary>
        public static ShopService EnsureShop()
        {
            if (ShopService.Instance != null) return ShopService.Instance;
            return new GameObject("ShopService").AddComponent<ShopService>();
        }
        /// <summary>Đảm bảo DailyRewardService tồn tại (trả về null nếu không tạo được).</summary>
        public static DailyRewardService EnsureDailyReward()
        {
            if (DailyRewardService.Instance != null) return DailyRewardService.Instance;
            return new GameObject("DailyRewardService").AddComponent<DailyRewardService>();
        }
    }
}
