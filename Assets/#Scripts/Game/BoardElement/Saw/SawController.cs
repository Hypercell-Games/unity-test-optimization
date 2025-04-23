using DG.Tweening;
using UnityEngine;

public class SawController : BaseBoardElementController
{
    [SerializeField] private Transform _shadowTransform;
    private readonly float _rotateTime = 0.3f;

    [Header("Settings")] private readonly Vector3 _rotateVector = new(0, 0, -60);

    public override void InitializeBoardElementInfo()
    {
        DoRotate();
    }

    private void DoRotate()
    {
        RotateTransform(_elementTransform);
        RotateTransform(_shadowTransform);
    }

    private void RotateTransform(Transform targetTransform)
    {
        targetTransform.DOLocalRotate(_rotateVector, _rotateTime, RotateMode.FastBeyond360).SetRelative()
            .SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental).SetLink(targetTransform.gameObject);
    }
}
