using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScaleAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Transform _customTargetTransform;

    private Transform _targetTransform;

    private void OnEnable()
    {
        _targetTransform = transform;

        if (_customTargetTransform != null)
        {
            _targetTransform = _customTargetTransform;
        }

        _targetTransform.localScale = Vector3.one;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _targetTransform.DOKill();
        _targetTransform.DOScale(0.85f, 0.2f)
            .SetEase(Ease.OutSine)
            .SetLink(_targetTransform.gameObject);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _targetTransform.DOKill();
        _targetTransform.DOScale(1f, 0.3f)
            .SetEase(Ease.OutBack)
            .SetLink(_targetTransform.gameObject);
    }
}
