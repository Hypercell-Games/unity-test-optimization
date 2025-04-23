using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Unpuzzle;

public class HookHandle : MonoBehaviour
{
    [SerializeField] private HookController _hookController;
    [SerializeField] private Transform _tweenObject;
    [SerializeField] private TrailRenderer _trail;
    [SerializeField] private SphereCollider _collider;
    [SerializeField] private CapsuleCollider _additionalCollider;
    [SerializeField] private HookMoveOutRaycaster _raycaster;
    [SerializeField] private List<BoxCollider> _sizableColliders;

    private Vector3 _defaultSize;

    private bool _isCollidersScaled;

    public List<BoxCollider> SizableColliders => _sizableColliders;

    public HookController HookController => _hookController;

    private void Awake()
    {
        if (_tweenObject != null)
        {
            _defaultSize = _tweenObject.localScale;
        }

        _hookController.hookHandle = this;
        _collider.radius = 1.9f * GameConfig.RemoteConfig.scalePinRadius;

        if (_additionalCollider != null)
        {
            _additionalCollider.enabled = GameConfig.RemoteConfig.additionalPinColliderEnabled;
        }
    }

    public event Action<HookController> OnHadleActivate = hookController => { };

    public void TryToRemoveHoook()
    {
        if (!_hookController.CanMoveOut)
        {
            return;
        }

        if (!LevelsController.Instance.CurrentGameController.CanMove())
        {
            return;
        }

        Physics.SyncTransforms();
        _hookController._pinOutLevel.MoveDone();
        _hookController.MoveOut(transform.up, transform.right, transform.forward, this, _trail, _raycaster);
        OnHadleActivate(_hookController);
    }

    public void Press()
    {
        if (_tweenObject != null)
        {
            _tweenObject.DOKill();
            _tweenObject.DOScale(0.8f * _defaultSize, 0.1f)
                .SetEase(Ease.Linear);
        }

        if (!_hookController.IsFrozen)
        {
            _hookController.Press(transform.up);
        }
    }

    public void Release()
    {
        if (_tweenObject != null)
        {
            _tweenObject.DOKill();
            _tweenObject.DOScale(_defaultSize, 0.1f)
                .SetEase(Ease.Linear);
        }

        _hookController.Unpress();
    }

    public void IncreaseColliderSize()
    {
        if (_isCollidersScaled)
        {
            return;
        }

        _isCollidersScaled = true;
        var direction = transform.up;
        foreach (var boxCollider in SizableColliders)
        {
            var sizeDirection = boxCollider.transform.InverseTransformDirection(direction);

            boxCollider.center += sizeDirection * 1.5f;

            boxCollider.size += sizeDirection * 3f;
        }
    }

    public void ResetColliderSize()
    {
        if (!_isCollidersScaled)
        {
            return;
        }

        _isCollidersScaled = false;

        var direction = transform.up;
        foreach (var boxCollider in SizableColliders)
        {
            var sizeDirection = boxCollider.transform.InverseTransformDirection(direction);

            boxCollider.center -= sizeDirection * 1.5f;

            boxCollider.size -= sizeDirection * 3f;
        }
    }
}
