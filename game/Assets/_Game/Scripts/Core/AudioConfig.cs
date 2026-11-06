namespace _Game.Scripts.Core
{
    using System;
    using UnityEngine;

    /// <summary>Tập hợp clip âm thanh của game. Tạo bằng chuột phải Assets → Create → WoolSort/Audio Config.</summary>
    [Serializable]
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "WoolSort/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        [Header("BGM (loop, persistent)")]
        public AudioClip bgmHome;
        public AudioClip bgmGameplay;

        [Header("SFX — gameplay")]
        public AudioClip sfxWin;
        public AudioClip sfxLose;
        public AudioClip sfxBoosterUse;
        public AudioClip sfxCoinCollect;
        public AudioClip sfxBoxClose;

        [Header("SFX — UI")]
        public AudioClip sfxClick;
        public AudioClip sfxPopupOpen;
        public AudioClip sfxPopupClose;
        public AudioClip sfxBoosterBuy;
    }
}
