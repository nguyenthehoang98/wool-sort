using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

// Đặt thanh menu bottom theo vị trí. Bản gốc dùng MediationManager (ads SDK) để
// đẩy bar lên khi có banner ads; target chưa có ads nên chỉ giữ vị trí "no ads".
public class FitPositionWithAds : MonoBehaviour
{
    [FormerlySerializedAs("_target")][SerializeField] private RectTransform target;

    [Header("Position")]
    [SerializeField] private int _positionYWithNoAds;
    [SerializeField] private int _positionYWithAds = 150;

    private float _oldPosY = 0;

    public void UpdatePositionBarBottom()
    {
        if (target == null) return;

        Vector3 pos = target.anchoredPosition;
        pos.y = _positionYWithNoAds;
        target.anchoredPosition = pos;
    }

    public void MoveDown()
    {
        if (target == null) return;
        _oldPosY = target.anchoredPosition.y;
    }

    public void MoveUp()
    {
        if (target == null) return;
        target.DOAnchorPos(new Vector2(0, _oldPosY), 0.5f);
    }

    public int GetPositionYWithAds() { return _positionYWithAds; }

    public int GetPositionYWithNoAds() { return _positionYWithNoAds; }
}
