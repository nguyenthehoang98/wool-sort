using System;
using System.Collections;
using DG.Tweening;
using HyperSonic;
using UnityEngine;

namespace _Game.Scripts
{
    // Port từ dự án gốc: sinh ra các đồng xu văng từ objSpawn rồi bay về objTarget.
    // Bản gốc dùng ObjectPool (Spawn/Recycle); bản này dùng Instantiate/Destroy.
    public class SplashItem : Singleton<SplashItem>
    {
        [SerializeField] private GameObject prfCoin;

        [SerializeField] private AnimationCurve coinSplashCurve;
        [SerializeField] private AnimationCurve coinToBarCurve;

        public void SplashCoin(int quantity, GameObject objSpawn, GameObject objTarget, float timeDelayLastCallback,
            Action callbackPerCoin, Action firstCallback, Action lastCallback)
        {
            if (prfCoin == null || objSpawn == null || objTarget == null)
            {
                Debug.LogWarning("[SplashItem] Thiếu prfCoin/objSpawn/objTarget, bỏ qua hiệu ứng.");
                lastCallback?.Invoke();
                return;
            }

            var totalCoin = quantity;
            var totalCoinLeft = quantity;

            for (int i = 0; i < quantity; i++)
            {
                float timeDelay = 0.05f * i;

                GameObject item = Instantiate(prfCoin, transform);
                item.transform.SetAsFirstSibling();
                item.transform.position = objSpawn.transform.position;
                item.transform.localScale = Vector3.zero;

                var range = 200f;
                float x = UnityEngine.Random.Range(-range, range);
                float y = UnityEngine.Random.Range(-range, range);

                Sequence mySequence = DOTween.Sequence();
                mySequence.AppendInterval(timeDelay);
                mySequence.Append(item.transform.DOMove(item.transform.position + new Vector3(x, y), 0.4f).SetEase(coinSplashCurve));
                mySequence.Insert(timeDelay, item.transform.DOScale(1.5f, 0.1f));
                mySequence.Insert(timeDelay + 0.1f, item.transform.DOScale(1f, 0.1f));
                mySequence.Append(item.transform.DOMove(objTarget.transform.position, 0.75f).SetEase(coinToBarCurve));
                mySequence.AppendCallback(() =>
                {
                    totalCoinLeft -= 1;
                    callbackPerCoin?.Invoke();

                    if (totalCoinLeft >= totalCoin - 1)
                    {
                        firstCallback?.Invoke();
                    }

                    if (totalCoinLeft <= 0)
                    {
                        StartCoroutine(DelayLastCallback(timeDelayLastCallback, lastCallback));
                    }

                    if (item != null) Destroy(item);
                });
            }
        }

        private IEnumerator DelayLastCallback(float timeDelay, Action callback)
        {
            yield return new WaitForSeconds(timeDelay);
            callback?.Invoke();
        }
    }
}
