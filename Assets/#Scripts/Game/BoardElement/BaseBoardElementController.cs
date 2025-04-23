using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public abstract class BaseBoardElementController : MonoBehaviour, IBoardElementController
{
    [SerializeField] protected EBoardElementType _boardElementController = EBoardElementType.EMPTY;
    [SerializeField] protected Transform _elementTransform;
    [SerializeField] protected Transform _shakeTransform;
    [SerializeField] protected SpriteRenderer _lockedStageSpriteRenderer;
    [SerializeField] protected ParticleSystem _unlockStageParticles;
    [SerializeField] protected Transform _hintPivot;

    [Header("Sort")] [SerializeField] protected List<SpriteRenderer> _sprites;

    [Header("Group")] [SerializeField] protected List<WrongDefaultGroup> _wrongDefaultGroups = new();

    [SerializeField] protected Transform _bounceParentTransform;

    private Sequence _bounceSizeSeq;

    private Tweener _bounceTweener;
    private Sequence _hintSeq;
    private Sequence _shakeTween;

    private int _sortOrder;
    private Sequence _unlockScaleSeq;
    private Sequence _unlockSeq;

    public Vector2Int GridPosition { get; set; }

    public EBoardElementType BoardElementType
    {
        get => _boardElementController;
        set => _boardElementController = value;
    }

    public event Action<BaseBoardElementController, EActionWithElement> onActionWithElement;

    public abstract void InitializeBoardElementInfo();

    public void DoActionWithTile(EActionWithElement actionWithElement)
    {
        onActionWithElement?.Invoke(this, actionWithElement);
    }

    public void SetStageLayer(int stage, bool fast, float delay = 0f, float duration = 0f)
    {
        if (_lockedStageSpriteRenderer == null)
        {
            return;
        }

        var locked = stage > 0;
        OnStageLayerUpdate(locked);

        if (fast)
        {
            _lockedStageSpriteRenderer.gameObject.SetActive(locked);
            return;
        }

        if (!locked)
        {
            UnlockTile(delay, duration);
        }
    }

    protected virtual void OnStageLayerUpdate(bool isLocked) { }

    private void UnlockTile(float delay, float duration)
    {
        var delay1 = delay * duration;
        var scale = transform.localScale;
        _unlockSeq?.Kill();
        _unlockSeq = DOTween.Sequence()
            .SetLink(_lockedStageSpriteRenderer.gameObject)
            .SetDelay(delay1)
            .AppendCallback(() =>
            {
                _lockedStageSpriteRenderer.gameObject.SetActive(false);
                if (_unlockStageParticles)
                {
                    _unlockStageParticles.gameObject.SetActive(true);
                    _unlockStageParticles.Stop();
                    _unlockStageParticles.Play();
                    var pls = _unlockStageParticles.transform.lossyScale;
                    _unlockStageParticles.transform.SetParent(null, true);
                    _unlockStageParticles.transform.localScale = pls * 2f;

                    _unlockScaleSeq?.Kill();
                    transform.localScale = scale * 0.33f;
                    _unlockScaleSeq = DOTween.Sequence()
                        .SetLink(gameObject)
                        .SetDelay(0.05f)
                        .Append(transform.DOScale(scale * 1.08f, 0.07f))
                        .Append(transform.DOScale(scale, 0.2f).SetEase(Ease.OutQuad));
                }

                if (GameConfig.RemoteConfig.unlockStagesAnimation && GameConfig.RemoteConfig.unlockStagesHaptic)
                {
                    var isLastTile = delay >= 1f;
                    VibrationController.Instance.Play(isLastTile
                        ? EVibrationType.MediumImpact
                        : EVibrationType.LightImpact);
                }
            });
    }

    public virtual void OnWrongClick(Action callback = null)
    {
        SetWrongEffectState(true);

        if (_bounceTweener != null)
        {
            _bounceTweener.Restart();
        }
        else
        {
            _bounceTweener = _bounceParentTransform.DOPunchScale(Vector3.one * 0.05f, 0.5f, 1).SetEase(Ease.Linear)
                .OnComplete(delegate
                {
                    SetWrongEffectState(false);
                    _bounceTweener = null;
                    callback?.Invoke();
                })
                .SetLink(gameObject);
        }
    }

    protected void SetWrongEffectState(bool state)
    {
        foreach (var group in _wrongDefaultGroups)
        {
            group.WrongSprite.sortingOrder = group.DefaultSprite.sortingOrder;

            group.WrongSprite.gameObject.SetActive(state);
            group.DefaultSprite.gameObject.SetActive(!state);
        }
    }

    public void ShakeTile(Action callback = null)
    {
        if (_shakeTween != null)
        {
            _shakeTween.Restart();
        }
        else
        {
            if (GameConfig.RemoteConfig.wrongClickAnimationVersion <= 0)
            {
                _shakeTween = DOTween.Sequence()
                    .SetLink(gameObject)
                    .Append(transform.DOShakePosition(0.2f, new Vector3(0.06f, 0f, 0f), 20, 0f, false, false))
                    .OnComplete(() =>
                    {
                        _shakeTween = null;
                        callback?.Invoke();
                    });
            }
            else
            {
                const float duration = 0.5f;
                var posX = transform.localPosition.x;
                transform.localPosition = transform.localPosition + Vector3.right * -0.06f;
                _shakeTween = DOTween.Sequence()
                    .SetLink(gameObject)
                    .Append(transform.DOLocalMoveX(posX, duration).SetEase(Ease.OutElastic))
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        _shakeTween = null;
                        callback?.Invoke();
                    });
            }
        }
    }

    public virtual int ChangeSortOrder(int sortOrder)
    {
        _sortOrder = sortOrder;
        var highestSortOrder = sortOrder;
        for (var i = 0; i < _sprites.Count; i++)
        {
            _sprites[i].sortingOrder = highestSortOrder;
            highestSortOrder += 2;
        }


        if (_lockedStageSpriteRenderer != null)
        {
            _lockedStageSpriteRenderer.sortingOrder = sortOrder + 300;
        }

        return highestSortOrder;
    }

    public virtual void AddSortOrder(int value)
    {
        ChangeSortOrder(_sortOrder + value);
    }

    public void BounceSize()
    {
        var startScale = _elementTransform.localScale;
        var minScale = startScale * 0.5f;
        var maxScale = startScale * 1.2f;

        _elementTransform.localScale = minScale;
        _bounceSizeSeq?.Kill();
        _bounceSizeSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(_elementTransform.DOScale(maxScale, 0.15f).SetEase(Ease.Linear))
            .Append(_elementTransform.DOScale(startScale, 0.15f).SetEase(Ease.Linear))
            .OnComplete(() => _bounceSizeSeq = null);
    }

    public virtual void OnBeforeDestroy() { }

    public virtual void ShowHint()
    {
        if (_hintPivot == null)
        {
            return;
        }


        if (this is TileElementController tile)
        {
            if (_hintSeq != null)
            {
                return;
            }

            HideHint();
            var dir = tile.MoveDirection;
            var scale0 = Vector3.one;
            var scaleFactor = GameConfig.RemoteConfig.hintWhenPlayerStuckScale;
            var scale1 =
                dir.x != 0 ? new Vector3(scale0.x * scaleFactor, scale0.y / scaleFactor, scale0.z) :
                dir.y != 0 ? new Vector3(scale0.x / scaleFactor, scale0.y * scaleFactor, scale0.z) : scale0;
            var pos1 = (scaleFactor + scaleFactor - 2f) * (Vector2)dir;
            _hintSeq = DOTween.Sequence()
                .SetLink(_hintPivot.gameObject)
                .SetLoops(-1)
                .Append(_hintPivot.DOLocalMove(pos1, 0.2f))
                .Join(_hintPivot.DOScale(scale1, 0.2f))
                .Append(_hintPivot.DOLocalMove(Vector3.zero, 0.2f))
                .Join(_hintPivot.DOScale(scale0, 0.2f))
                .Append(_hintPivot.DOLocalMove(pos1, 0.2f))
                .Join(_hintPivot.DOScale(scale1, 0.2f))
                .Append(_hintPivot.DOLocalMove(Vector3.zero, 1.2f).SetEase(Ease.OutExpo))
                .Join(_hintPivot.DOScale(scale0, 1.2f).SetEase(Ease.OutExpo));
        }
    }

    public void HideHint()
    {
        _hintSeq?.Kill();
        _hintSeq = null;
        _hintPivot.localPosition = Vector3.zero;
        _hintPivot.localScale = Vector3.one;
    }
}

[Serializable]
public class WrongDefaultGroup
{
    public SpriteRenderer DefaultSprite;
    public SpriteRenderer WrongSprite;
}

public enum EActionWithElement
{
    MOVE = 0,
    DESTROY = 1
}
