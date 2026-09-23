using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _KIT
{
    public static class Tween
    {
        #region TRANSFORM

        public static TweenCore LocalPosition(this Transform transform, Vector3 from, Vector3 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            return Lerp(from, target, duration, position => { transform.localPosition = position; }, playerLoop);
        }

        public static TweenCore LocalPositionX(this Transform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 position = transform.localPosition;
            Vector3 end = position;
            position.x = from;
            end.x = target;
            return Lerp(position, end, duration, vector => { transform.localPosition = vector; }, playerLoop);
        }

        public static TweenCore LocalPositionY(this Transform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 position = transform.localPosition;
            Vector3 end = position;
            position.y = from;
            end.y = target;
            return Lerp(position, end, duration, vector => { transform.localPosition = vector; }, playerLoop);
        }

        public static TweenCore LocalPositionZ(this Transform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 position = transform.localPosition;
            Vector3 end = position;
            position.z = from;
            end.z = target;
            return Lerp(position, end, duration, vector => { transform.localPosition = vector; }, playerLoop);
        }

        public static TweenCore LocalPosition(this Transform transform, Vector3 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 start = transform.localPosition;
            return Lerp(start, target, duration, position => { transform.localPosition = position; }, playerLoop);
        }

        public static TweenCore LocalPositionX(this Transform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 position = transform.localPosition;
            Vector3 end = position;
            end.x = target;
            return Lerp(position, end, duration, vector => { transform.localPosition = vector; }, playerLoop);
        }

        public static TweenCore LocalPositionY(this Transform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 position = transform.localPosition;
            Vector3 end = position;
            end.y = target;
            return Lerp(position, end, duration, vector => { transform.localPosition = vector; }, playerLoop);
        }

        public static TweenCore LocalPositionZ(this Transform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 position = transform.localPosition;
            Vector3 end = position;
            end.z = target;
            return Lerp(position, end, duration, vector => { transform.localPosition = vector; }, playerLoop);
        }

        public static TweenCore Position(this Transform transform, Vector3 from, Vector3 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 start = from;
            return Lerp(start, target, duration, position => { transform.position = position; }, playerLoop);
        }

        public static TweenCore PositionX(this Transform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 position = transform.position;
            Vector3 end = position;
            position.x = from;
            end.x = target;
            return Lerp(position, end, duration, vector => { transform.position = vector; }, playerLoop);
        }

        public static TweenCore PositionY(this Transform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 position = transform.position;
            Vector3 end = position;
            position.y = from;
            end.y = target;
            return Lerp(position, end, duration, vector => { transform.position = vector; }, playerLoop);
        }

        public static TweenCore PositionZ(this Transform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 position = transform.position;
            Vector3 end = position;
            position.z = from;
            end.z = target;
            return Lerp(position, end, duration, vector => { transform.position = vector; }, playerLoop);
        }

        public static TweenCore Position(this Transform transform, Vector3 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 start = transform.position;
            return Lerp(start, target, duration, position => { transform.position = position; }, playerLoop);
        }

        public static TweenCore PositionX(this Transform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 position = transform.position;
            Vector3 end = position;
            end.x = target;
            return Lerp(position, end, duration, vector => { transform.position = vector; }, playerLoop);
        }

        public static TweenCore PositionY(this Transform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 position = transform.position;
            Vector3 end = position;
            end.y = target;
            return Lerp(position, end, duration, vector => { transform.position = vector; }, playerLoop);
        }

        public static TweenCore PositionZ(this Transform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 position = transform.position;
            Vector3 end = position;
            end.z = target;
            return Lerp(position, end, duration, vector => { transform.position = vector; }, playerLoop);
        }

        public static TweenCore LocalEulerAngles(this Transform transform, Vector3 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 start = transform.localEulerAngles;
            return Lerp(start, target, duration, vector => { transform.localEulerAngles = vector; }, playerLoop);
        }

        public static TweenCore LocalEulerAngles(this Transform transform, Vector3 from, Vector3 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            return Lerp(from, target, duration, vector => { transform.localEulerAngles = vector; }, playerLoop);
        }

        public static TweenCore LocalEulerAnglesX(this Transform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 eulerAngles = transform.localEulerAngles;
            Vector3 end = eulerAngles;
            end.x = target;
            return Lerp(eulerAngles, end, duration, vector => { transform.localEulerAngles = vector; }, playerLoop);
        }

        public static TweenCore LocalEulerAnglesX(this Transform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 eulerAngles = transform.localEulerAngles;
            Vector3 end = eulerAngles;
            eulerAngles.x = from;
            end.x = target;
            return Lerp(eulerAngles, end, duration, vector => { transform.localEulerAngles = vector; }, playerLoop);
        }

        public static TweenCore LocalEulerAnglesY(this Transform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 eulerAngles = transform.localEulerAngles;
            Vector3 end = eulerAngles;
            end.y = target;
            return Lerp(eulerAngles, end, duration, vector => { transform.localEulerAngles = vector; }, playerLoop);
        }

        public static TweenCore LocalEulerAnglesY(this Transform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 eulerAngles = transform.localEulerAngles;
            Vector3 end = eulerAngles;
            eulerAngles.y = from;
            end.y = target;
            return Lerp(eulerAngles, end, duration, vector => { transform.localEulerAngles = vector; }, playerLoop);
        }

        public static TweenCore LocalEulerAnglesZ(this Transform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 eulerAngles = transform.localEulerAngles;
            Vector3 end = eulerAngles;
            end.z = target;
            return Lerp(eulerAngles, end, duration, vector => { transform.localEulerAngles = vector; }, playerLoop);
        }

        public static TweenCore LocalEulerAnglesZ(this Transform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 eulerAngles = transform.localEulerAngles;
            Vector3 end = eulerAngles;
            eulerAngles.z = from;
            end.z = target;
            return Lerp(eulerAngles, end, duration, vector => { transform.localEulerAngles = vector; }, playerLoop);
        }

        public static TweenCore EulerAngles(this Transform transform, Vector3 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 start = transform.eulerAngles;
            return Lerp(start, target, duration, vector => { transform.eulerAngles = vector; }, playerLoop);
        }

        public static TweenCore EulerAngles(this Transform transform, Vector3 from, Vector3 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            return Lerp(from, target, duration, vector => { transform.eulerAngles = vector; }, playerLoop);
        }

        public static TweenCore EulerAnglesX(this Transform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 eulerAngles = transform.eulerAngles;
            Vector3 end = eulerAngles;
            end.x = target;
            return Lerp(eulerAngles, end, duration, vector => { transform.eulerAngles = vector; }, playerLoop);
        }

        public static TweenCore EulerAnglesX(this Transform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 eulerAngles = transform.eulerAngles;
            Vector3 end = eulerAngles;
            eulerAngles.x = from;
            end.x = target;
            return Lerp(eulerAngles, end, duration, vector => { transform.eulerAngles = vector; }, playerLoop);
        }

        public static TweenCore EulerAnglesY(this Transform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 eulerAngles = transform.eulerAngles;
            Vector3 end = eulerAngles;
            end.y = target;
            return Lerp(eulerAngles, end, duration, vector => { transform.eulerAngles = vector; }, playerLoop);
        }

        public static TweenCore EulerAnglesY(this Transform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 eulerAngles = transform.eulerAngles;
            Vector3 end = eulerAngles;
            eulerAngles.y = from;
            end.y = target;
            return Lerp(eulerAngles, end, duration, vector => { transform.eulerAngles = vector; }, playerLoop);
        }

        public static TweenCore EulerAnglesZ(this Transform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 eulerAngles = transform.eulerAngles;
            Vector3 end = eulerAngles;
            end.z = target;
            return Lerp(eulerAngles, end, duration, vector => { transform.eulerAngles = vector; }, playerLoop);
        }

        public static TweenCore EulerAnglesZ(this Transform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 eulerAngles = transform.eulerAngles;
            Vector3 end = eulerAngles;
            eulerAngles.z = from;
            end.z = target;
            return Lerp(eulerAngles, end, duration, vector => { transform.eulerAngles = vector; }, playerLoop);
        }

        public static TweenCore LocalScale(this Transform transform, Vector3 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 start = transform.localScale;
            return Lerp(start, target, duration, vector => { transform.localScale = vector; }, playerLoop);
        }

        public static TweenCore LocalScale(this Transform transform, Vector3 from, Vector3 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            return Lerp(from, target, duration, vector => { transform.localScale = vector; }, playerLoop);
        }

        public static TweenCore LocalScaleX(this Transform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 localScale = transform.localScale;
            Vector3 end = localScale;
            end.x = target;
            return Lerp(localScale, end, duration, vector => { transform.localScale = vector; }, playerLoop);
        }

        public static TweenCore LocalScaleX(this Transform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 localScale = transform.localScale;
            Vector3 end = localScale;
            localScale.x = from;
            end.x = target;
            return Lerp(localScale, end, duration, vector => { transform.localScale = vector; }, playerLoop);
        }

        public static TweenCore LocalScaleY(this Transform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 localScale = transform.localScale;
            Vector3 end = localScale;
            end.y = target;
            return Lerp(localScale, end, duration, vector => { transform.localScale = vector; }, playerLoop);
        }

        public static TweenCore LocalScaleY(this Transform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 localScale = transform.localScale;
            Vector3 end = localScale;
            localScale.y = from;
            end.y = target;
            return Lerp(localScale, end, duration, vector => { transform.localScale = vector; }, playerLoop);
        }

        public static TweenCore LocalScaleZ(this Transform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 localScale = transform.localScale;
            Vector3 end = localScale;
            end.z = target;
            return Lerp(localScale, end, duration, vector => { transform.localScale = vector; }, playerLoop);
        }

        public static TweenCore LocalScaleZ(this Transform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 localScale = transform.localScale;
            Vector3 end = localScale;
            localScale.z = from;
            end.z = target;
            return Lerp(localScale, end, duration, vector => { transform.localScale = vector; }, playerLoop);
        }

        #endregion

        #region RECT-TRANSFORM

        public static TweenCore AnchoredPosition3D(this RectTransform transform, Vector3 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 start = transform.anchoredPosition3D;
            return Lerp(start, target, duration, position => { transform.anchoredPosition3D = position; }, playerLoop);
        }

        public static TweenCore AnchoredPosition3D(this RectTransform transform, Vector3 from, Vector3 target,
            float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            return Lerp(from, target, duration, position => { transform.anchoredPosition3D = position; }, playerLoop);
        }

        public static TweenCore AnchoredPosition(this RectTransform transform, Vector2 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector2 start = transform.anchoredPosition3D;
            return Lerp(start, target, duration, position => { transform.anchoredPosition = position; }, playerLoop);
        }

        public static TweenCore AnchoredPosition(this RectTransform transform, Vector2 from, Vector2 target,
            float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            return Lerp(from, target, duration, position => { transform.anchoredPosition = position; }, playerLoop);
        }

        public static TweenCore AnchoredPosition3DX(this RectTransform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 start = transform.anchoredPosition3D;
            Vector3 end = start;
            end.x = target;
            return Lerp(start, end, duration, position => { transform.anchoredPosition3D = position; }, playerLoop);
        }

        public static TweenCore AnchoredPosition3DX(this RectTransform transform, float from, float target,
            float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 start = transform.anchoredPosition3D;
            Vector3 end = start;
            start.x = from;
            end.x = target;
            return Lerp(start, end, duration, position => { transform.anchoredPosition3D = position; }, playerLoop);
        }

        public static TweenCore AnchoredPositionX(this RectTransform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector2 start = transform.anchoredPosition;
            Vector2 end = start;
            end.x = target;
            return Lerp(start, end, duration, position => { transform.anchoredPosition = position; }, playerLoop);
        }

        public static TweenCore AnchoredPositionX(this RectTransform transform, float from, float target,
            float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector2 start = transform.anchoredPosition;
            Vector2 end = start;
            start.x = from;
            end.x = target;
            return Lerp(start, end, duration, position => { transform.anchoredPosition = position; }, playerLoop);
        }

        public static TweenCore AnchoredPosition3DY(this RectTransform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 start = transform.anchoredPosition3D;
            Vector3 end = start;
            end.y = target;
            return Lerp(start, end, duration, position => { transform.anchoredPosition3D = position; }, playerLoop);
        }

        public static TweenCore AnchoredPosition3DY(this RectTransform transform, float from, float target,
            float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 start = transform.anchoredPosition3D;
            Vector3 end = start;
            start.y = from;
            end.y = target;
            return Lerp(start, end, duration, position => { transform.anchoredPosition3D = position; }, playerLoop);
        }

        public static TweenCore AnchoredPositionY(this RectTransform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector2 start = transform.anchoredPosition;
            Vector2 end = start;
            end.y = target;
            return Lerp(start, end, duration, position => { transform.anchoredPosition = position; }, playerLoop);
        }

        public static TweenCore AnchoredPositionY(this RectTransform transform, float from, float target,
            float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector2 start = transform.anchoredPosition;
            Vector2 end = start;
            start.y = from;
            end.y = target;
            return Lerp(start, end, duration, position => { transform.anchoredPosition = position; }, playerLoop);
        }

        public static TweenCore AnchoredPosition3DZ(this RectTransform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 start = transform.anchoredPosition3D;
            Vector3 end = start;
            end.z = target;
            return Lerp(start, end, duration, position => { transform.anchoredPosition3D = position; }, playerLoop);
        }

        public static TweenCore AnchoredPosition3DZ(this RectTransform transform, float from, float target,
            float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector3 start = transform.anchoredPosition3D;
            Vector3 end = start;
            start.z = from;
            end.z = target;
            return Lerp(start, end, duration, position => { transform.anchoredPosition3D = position; }, playerLoop);
        }

        public static TweenCore AnchorMin(this RectTransform transform, Vector2 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector2 start = transform.anchorMin;
            return Lerp(start, target, duration, anchorMin => { transform.anchorMin = anchorMin; }, playerLoop);
        }
        
        public static TweenCore AnchorMin(this RectTransform transform, Vector2 from, Vector2 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            return Lerp(from, target, duration, anchorMin => { transform.anchorMin = anchorMin; }, playerLoop);
        }
        
        public static TweenCore AnchorMax(this RectTransform transform, Vector2 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector2 start = transform.anchorMax;
            return Lerp(start, target, duration, anchorMax => { transform.anchorMax = anchorMax; }, playerLoop);
        }
        
        public static TweenCore AnchorMax(this RectTransform transform, Vector2 from, Vector2 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            return Lerp(from, target, duration, anchorMax => { transform.anchorMax = anchorMax; }, playerLoop);
        }
        
        public static TweenCore Pivot(this RectTransform transform, Vector2 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector2 start = transform.pivot;
            return Lerp(start, target, duration, pivot => { transform.pivot = pivot; }, playerLoop);
        }
        
        public static TweenCore Pivot(this RectTransform transform, Vector2 from, Vector2 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            return Lerp(from, target, duration, pivot => { transform.pivot = pivot; }, playerLoop);
        }
        
        public static TweenCore PivotX(this RectTransform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector2 start = transform.pivot;
            Vector2 end = start;
            end.x = target;
            return Lerp(start, end, duration, pivot => { transform.pivot = pivot; }, playerLoop);
        }
        
        public static TweenCore PivotX(this RectTransform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector2 start = transform.pivot;
            Vector2 end = start;
            start.x = from;
            end.x = target;
            return Lerp(start, end, duration, pivot => { transform.pivot = pivot; }, playerLoop);
        }
        
        public static TweenCore PivotY(this RectTransform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector2 start = transform.pivot;
            Vector2 end = start;
            end.y = target;
            return Lerp(start, end, duration, pivot => { transform.pivot = pivot; }, playerLoop);
        }
        
        public static TweenCore PivotY(this RectTransform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector2 start = transform.pivot;
            Vector2 end = start;
            start.y = from;
            end.y = target;
            return Lerp(start, end, duration, pivot => { transform.pivot = pivot; }, playerLoop);
        }
        
        public static TweenCore SizeDelta(this RectTransform transform, Vector2 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector2 start = transform.sizeDelta;
            return Lerp(start, target, duration, sizeDelta => { transform.sizeDelta = sizeDelta; }, playerLoop);
        }
        
        public static TweenCore SizeDelta(this RectTransform transform, Vector2 from, Vector2 target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            return Lerp(from, target, duration, sizeDelta => { transform.sizeDelta = sizeDelta; }, playerLoop);
        }
        
        public static TweenCore SizeDeltaX(this RectTransform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector2 start = transform.sizeDelta;
            Vector2 end = start;
            end.x = target;
            return Lerp(start, end, duration, sizeDelta => { transform.sizeDelta = sizeDelta; }, playerLoop);
        }
        
        public static TweenCore SizeDeltaX(this RectTransform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector2 start = transform.sizeDelta;
            Vector2 end = start;
            start.x = from;
            end.x = target;
            return Lerp(start, end, duration, sizeDelta => { transform.sizeDelta = sizeDelta; }, playerLoop);
        }
        
        public static TweenCore SizeDeltaY(this RectTransform transform, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector2 start = transform.sizeDelta;
            Vector2 end = start;
            end.y = target;
            return Lerp(start, end, duration, sizeDelta => { transform.sizeDelta = sizeDelta; }, playerLoop);
        }
        
        public static TweenCore SizeDeltaY(this RectTransform transform, float from, float target, float duration,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            Vector2 start = transform.sizeDelta;
            Vector2 end = start;
            start.y = from;
            end.y = target;
            return Lerp(start, end, duration, sizeDelta => { transform.sizeDelta = sizeDelta; }, playerLoop);
        }
        
        #endregion

        #region TEMP

        public static TweenCore Lerp(this Vector3 start, Vector3 target, float duration, Action<Vector3> onResult,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            return TweenManager.Instance.CreateTween(duration,
                f => { onResult?.Invoke(Vector3.Lerp(start, target, f)); }, playerLoop);
        }

        public static TweenCore Lerp(this Vector2 start, Vector2 target, float duration, Action<Vector2> onResult,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            return TweenManager.Instance.CreateTween(duration,
                f => { onResult?.Invoke(Vector2.Lerp(start, target, f)); }, playerLoop);
        }

        public static TweenCore Lerp(this float start, float target, float duration, Action<float> onResult,
            PlayerLoop playerLoop = PlayerLoop.Update)
        {
            return TweenManager.Instance.CreateTween(duration, f => { onResult?.Invoke(start + (target - start) * f); },
                playerLoop);
        }

        public static TweenCore Wait(float duration, Action onFinish)
        {
            TweenCore tween = TweenManager.Instance.CreateTween(duration, null, PlayerLoop.Update);
            tween.OnComplete(onFinish);
            return tween;
        }

        public static TweenCore WaitFrame(int frame, Action onFinish)
        {
            int cur = frame;
            float duration = int.MaxValue;
            TweenCore core = TweenManager.Instance.CreateTween(duration, null, PlayerLoop.Update);
            core.OnUpdate(f =>
            {
                cur--;
                if (cur <= 0)
                {
                    core.Kill();
                    onFinish?.Invoke();
                }
            });
            return core;
        }

        public static TweenCore Timer(float interval, Func<float> funcInvoke, Action onComplete)
        {
            float time = 0;
            float preTime = 0;
            float duration = float.MaxValue;
            TweenCore tween = TweenManager.Instance.CreateTween(duration, null, PlayerLoop.Update);
            tween.OnUpdate(f =>
            {
                float deltaTime = f - preTime;
                preTime = f;
                time += deltaTime;
                if (time >= interval)
                {
                    time -= interval;
                    if (funcInvoke.Invoke() <= 0)
                    {
                        tween.Kill();
                        onComplete?.Invoke();
                    }
                }
            });
            return tween;
        }

        public static TweenCore Progress(float duration, Action<float> onProgress)
        {
            return TweenManager.Instance.CreateTween(duration, onProgress, PlayerLoop.Update);
        }

        public static void Shake(float duration, float magnitude, Action<Vector3> onProgress, Action onCompleted)
        {
            TweenManager.Instance.CreateTween(duration, f =>
            {
                float ease = magnitude * Mathf.Sin(f * Mathf.PI);
                onProgress?.Invoke(new Vector3(
                    Random.Range(-1f, 1f) * ease,
                    Random.Range(-1f, 1f) * ease,
                    Random.Range(-1f, 1f) * ease)
                );
            }, PlayerLoop.Update).OnComplete(onCompleted);
        }

        public static void Kill(this TweenCore core)
        {
            TweenManager.Instance.Kill(core);
        }

        #endregion
    }
}