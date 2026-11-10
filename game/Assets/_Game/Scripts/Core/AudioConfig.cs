namespace _Game.Scripts.Core
{
    using System;
    using UnityEngine;

    /// <summary>Tập hợp clip âm thanh của game. Tạo bằng chuột phải Assets → Create → WoolSort/Audio Config.
    /// Mỗi SFX có kèm 1 field volume (0..1) để chỉnh âm lượng riêng trong Inspector.</summary>
    [Serializable]
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "WoolSort/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        [Header("BGM (loop, persistent)")]
        public AudioClip bgmHome;
        public AudioClip bgmGameplay;

        [Header("SFX — gameplay")]
        public AudioClip sfxWin;
        [Range(0f, 2f)] public float sfxWinVolume = 1f;
        public AudioClip sfxLose;
        [Range(0f, 2f)] public float sfxLoseVolume = 1f;
        public AudioClip sfxBoosterUse;
        [Range(0f, 2f)] public float sfxBoosterUseVolume = 1f;
        public AudioClip sfxCoinCollect;
        [Range(0f, 2f)] public float sfxCoinCollectVolume = 1f;
        public AudioClip sfxBoxClose;
        [Range(0f, 2f)] public float sfxBoxCloseVolume = 1f;

        [Header("SFX — Yarn / Tap / Dissolve")]
        public AudioClip sfxTap;
        [Range(0f, 2f)] public float sfxTapVolume = 1f;
        public AudioClip sfxDissolve;
        [Range(0f, 2f)] public float sfxDissolveVolume = 1f;
        public AudioClip sfxRope;
        [Range(0f, 2f)] public float sfxRopeVolume = 1f;
        public AudioClip sfxHold;
        [Range(0f, 2f)] public float sfxHoldVolume = 1f;

        [Header("SFX — Box")]
        public AudioClip sfxBoxFull;
        [Range(0f, 2f)] public float sfxBoxFullVolume = 1f;

        [Header("SFX — Booster")]
        public AudioClip sfxBoosterAddSlot;
        [Range(0f, 2f)] public float sfxBoosterAddSlotVolume = 1f;
        public AudioClip sfxBoosterClean;
        [Range(0f, 2f)] public float sfxBoosterCleanVolume = 1f;
        public AudioClip sfxBoosterMagicBox;
        [Range(0f, 2f)] public float sfxBoosterMagicBoxVolume = 1f;

        [Header("SFX — Level / Revive")]
        public AudioClip sfxLevelStart;
        [Range(0f, 2f)] public float sfxLevelStartVolume = 1f;
        public AudioClip sfxRevive;
        [Range(0f, 2f)] public float sfxReviveVolume = 1f;

        [Header("SFX — UI")]
        public AudioClip sfxClick;
        [Range(0f, 2f)] public float sfxClickVolume = 1f;
        public AudioClip sfxPopupOpen;
        [Range(0f, 2f)] public float sfxPopupOpenVolume = 1f;
        public AudioClip sfxPopupClose;
        [Range(0f, 2f)] public float sfxPopupCloseVolume = 1f;
        public AudioClip sfxBuyCoin;
        [Range(0f, 2f)] public float sfxBuyCoinVolume = 1f;
        public AudioClip sfxBuyBooster;
        [Range(0f, 2f)] public float sfxBuyBoosterVolume = 1f;
    }
}
