using System;
using _KIT;
using UnityEngine;

namespace _KIT
{
    [Serializable]
    public struct TweenCore
    {
        private static EasingFunction.Function defaultEasing = EasingFunction.GetEasingFunction(_KIT.Ease.Linear);
        [SerializeField] internal int ID; // only set from Create Method
        [SerializeField] internal int Index; // only set from Create Method
        internal PlayerLoop PlayerLoop;
        [SerializeField] internal bool IsValid; // only set from tween manager.
        private Action onComplete;
        private Action<float> onUpdate;
        private Action<float> onProgress;
        private EasingFunction.Function function;
        [SerializeField] private float duration;
        [SerializeField] private float elapsed;
        private float delay;

        internal TweenCore Create(int id, int index, float duration, Action<float> onProgress, PlayerLoop playerLoop)
        {            
            this.onProgress = onProgress;
            this.duration = duration;
            Index = index;
            PlayerLoop = playerLoop;
            IsValid = true;
            ID = id;
            elapsed = 0;
            delay = 0;
            onComplete = null;
            onUpdate = null;
            function = defaultEasing;
            return this;
        }

        public TweenCore Delay(float seconds)
        {
            TweenManager manager = TweenManager.Instance;
            ref TweenCore core = ref manager.Get(Index, PlayerLoop);
            if (core.IsValid && core.ID == ID)
            {
                core.delay = seconds;
            }
#if UNITY_EDITOR && DEBUG
            else
            {
                Debug.LogError("khong tim thay");
            }
#endif
            return this;
        }
        
        public TweenCore Ease(Ease easing)
        {
            TweenManager manager = TweenManager.Instance;
            ref TweenCore core = ref manager.Get(Index, PlayerLoop);
            if (core.IsValid && core.ID == ID)
            {
                core.function = EasingFunction.GetEasingFunction(easing);
            }
#if UNITY_EDITOR && DEBUG
            else
            {
                Debug.LogError(string.Format("Lỗi đăng kí Ease 'không tim thấy ID={0}->{1}, Index={2}->{3}'", ID,
                    core.ID, Index, core.Index));
            }
#endif
            return this;
        }

        public TweenCore OnUpdate(Action<float> onUpdate)
        {
            TweenManager manager = TweenManager.Instance;
            ref TweenCore core = ref manager.Get(Index, PlayerLoop);
            if (core.IsValid && core.ID == ID)
            {
                core.onUpdate += onUpdate;
            }
#if UNITY_EDITOR && DEBUG
            else
            {
                Debug.LogError(string.Format("Lỗi đăng kí OnUpdate 'không tim thấy ID={0}->{1}, Index={2}->{3}'", ID,
                    core.ID, Index, core.Index));
            }
#endif
            return this;
        }

        public TweenCore OnComplete(Action onComplete)
        {
            TweenManager manager = TweenManager.Instance;
            ref TweenCore core = ref manager.Get(Index, PlayerLoop);
            if (core.IsValid && core.ID == ID)
            {
                core.onComplete += onComplete;
            }
#if UNITY_EDITOR && DEBUG
            else
            {
                Debug.LogError(string.Format("Lỗi đăng kí OnComplete 'không tim thấy ID={0}->{1}, Index={2}->{3}'", ID,
                    core.ID, Index, core.Index));
            }
#endif
            return this;
        }

        internal void Execute(float delta)
        {
            if (!IsValid || elapsed >= duration + delay) return;

            float before = elapsed - delay;
            elapsed += delta;
            float realElapsed = elapsed - delay;

            if (realElapsed < 0) return; // todo: wait delay
            
            onUpdate?.Invoke(realElapsed);

            if (onProgress != null)
            {
                float progress = function(0, 1, Mathf.Min(realElapsed / duration, 1));
                onProgress.Invoke(progress);
            }

            if (onComplete != null)
            {
                if (before < duration && realElapsed >= duration) onComplete.Invoke();
            }
        }

        internal bool IsFinished => elapsed >= duration + delay && IsValid;
    }
}