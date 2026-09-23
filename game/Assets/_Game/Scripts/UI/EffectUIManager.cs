using System.Collections;
using HyperSonic;
using UnityEngine;

public class EffectUIManager : Singleton<EffectUIManager>
{
    [SerializeField] private ParticleSystem prfGetCoin;
    [SerializeField] private ParticleSystem prfTouchFX;
    [SerializeField] private ParticleSystem prfGetReward;

    public void ShowGetCoinEffect(Vector3 pos) => Play(prfGetCoin, pos);
    public void ShowTouchFX(Vector3 pos) => Play(prfTouchFX, pos);
    public void ShowGetRewardEffect(Vector3 pos) => Play(prfGetReward, pos);

    private void Play(ParticleSystem prefab, Vector3 position)
    {
        if (prefab == null) return;
        var effect = Instantiate(prefab, position, Quaternion.identity, transform);
        effect.Play();
        StartCoroutine(DestroyAfter(effect, 1f));
    }

    private static IEnumerator DestroyAfter(ParticleSystem effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (effect != null) Object.Destroy(effect.gameObject);
    }
}
