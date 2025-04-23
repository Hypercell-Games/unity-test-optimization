using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteAlways]
[SelectionBase]
public class Block : MonoBehaviour
{
    [SerializeField] private BlockDirection _direction;
    [SerializeField] private BlockColor _color;

    [Space] [SerializeField] private BlockColorsScheme _blockColorsScheme;

    [SerializeField] private BlockView _blockView;
    [SerializeField] private BlockTrail _trail;
    [SerializeField] private Collider _collider;
    [SerializeField] private Collider _tapHelperCollider;
    private float _scaler;

    private bool _tapable;

    public BlockDirection Direction => _direction;
    public bool Completed { get; private set; }

    private void Update()
    {
        if (Application.IsPlaying(this))
        {
            return;
        }

        Validate();
    }

    public void SetDirection(BlockDirection direction)
    {
        _direction = direction;
        ValidateDirection();
    }

    public void Init(float levelScale, bool isNearest, float delay, Action callback)
    {
        _scaler = levelScale;
        _blockView.name = $"{_blockView.name} {GetPosString(transform.localPosition)}";
        _blockView.transform.SetParent(transform.parent, true);
        _tapable = true;
        _tapHelperCollider.enabled = false;
        _trail.gameObject.SetActive(false);
        _trail.Init(levelScale, _blockColorsScheme.GetMaterials(_color));

        var lp = _blockView.transform.localPosition;
        var lr = _blockView.transform.localRotation;

        const float duration = 1f;
        if (!isNearest)
        {
            _blockView.transform.localPosition = lp + lp.normalized * 30f;
            _blockView.transform.localRotation = Quaternion.Euler(Random.onUnitSphere * 180f) * lr;
            _blockView.transform.localScale = Vector3.zero;

            DOTween.Sequence()
                .SetLink(gameObject)
                .SetDelay(delay * 1f + Random.value * 0.1f + 0.05f)
                .Append(_blockView.transform.DOLocalMove(lp, duration).SetEase(Ease.OutQuint))
                .Join(_blockView.transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuint))
                .Join(_blockView.transform.DOLocalRotateQuaternion(lr, duration).SetEase(Ease.OutCubic))
                .OnComplete(() => callback?.Invoke());
        }
        else
        {
            _blockView.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 10f;
            DOTween.Sequence()
                .SetLink(gameObject)
                .Append(_blockView.transform.DOLocalMove(lp, duration).SetEase(Ease.InOutQuint))
                .Join(_blockView.transform.DOScale(1.15f, 0.3f).SetEase(Ease.OutQuad))
                .Insert(0.3f, _blockView.transform.DOScale(1f, duration).SetEase(Ease.OutQuint))
                .OnComplete(() => callback?.Invoke());
        }
    }

    public TapOnBlockResult Tap(ref int rightClickRage, ref bool isReverse, Action callback)
    {
        if (!_tapable)
        {
            return TapOnBlockResult.NotTapped;
        }

        if (Gamestoty.debugDevice)
        {
            _blockView.TapEffect();
        }

        callback?.Invoke();
        var p0 = transform.position;
        var forward = transform.forward;

        if (Physics.Raycast(p0, forward, out var hitInfo))
        {
            var rb = hitInfo.collider.attachedRigidbody;
            if (rb && rb.TryGetComponent<Block>(out var block))
            {
                rightClickRage = 0;
                isReverse = false;
                var blockViewParent = _blockView.transform.parent;
                var nextBlockPos = block.transform.position;
                var p1 = nextBlockPos - forward * _scaler;
                var distance = Mathf.Max(0f, Vector3.Distance(p0, p1));
                var lp0 = blockViewParent.InverseTransformPoint(p0);
                var lp1 = blockViewParent.InverseTransformPoint(p1);
                var bounceDir = Vector3.ClampMagnitude(blockViewParent.InverseTransformPoint(nextBlockPos) - lp0, 3f);
                if (distance / _scaler < 0.001f)
                {
                    VibrationController.Instance.Play(EVibrationType.Failure);
                    Bounce(bounceDir);
                }
                else
                {
                    VibrationController.Instance.Play(EVibrationType.LightImpact);
                    _tapable = false;
                    _blockView.StopBounce();

                    var duration = distance * 0.2f / _scaler;
                    DOVirtual.DelayedCall(duration - 0.1f, () =>
                    {
                        VibrationController.Instance.Play(EVibrationType.Failure);
                    }).SetLink(gameObject);
                    DOTween.Sequence()
                        .SetLink(gameObject)
                        .Append(_blockView.transform.DOLocalMove(lp1, duration).SetEase(Ease.InSine))
                        .AppendCallback(() =>
                        {
                            block.Bounce(bounceDir);
                        })
                        .Append(_blockView.transform.DOLocalMove(lp0, duration).SetEase(Ease.OutSine))
                        .OnComplete(() => _tapable = true);
                }
            }

            return TapOnBlockResult.WrongClick;
        }

        {
            Completed = true;

            VibrationController.Instance.Play(EVibrationType.LightImpact);
            _tapable = false;
            _collider.enabled = false;
            _tapHelperCollider.enabled = false;
            _blockView.StopBounce();
            _blockView.transform.SetParent(null, true);
            var distance = 20f;

            var duration = distance * 0.07f;
            _trail.gameObject.SetActive(true);
            DOTween.Sequence()
                .SetLink(gameObject)
                .Append(_blockView.transform.DOMove(p0 + forward * distance * _scaler, duration).SetEase(Ease.Linear))
                .Insert(duration - 0.4f,
                    _blockView.transform.DOScale(0f, 0.4f).SetEase(Ease.InBack)
                        .OnUpdate(() => _trail.SetScale(_blockView.transform.lossyScale.x)))
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            return TapOnBlockResult.RightClick;
        }
    }

    public void Bounce(Vector3 dir)
    {
        if (!_tapable)
        {
            return;
        }

        _blockView.Bounce(dir);
        var dir1 = transform.parent.TransformDirection(dir);
        var pos = transform.position;
        if (Physics.Raycast(pos, dir1, out var hitInfo))
        {
            var rb = hitInfo.collider.attachedRigidbody;
            if (rb && rb.TryGetComponent<Block>(out var block) &&
                Vector3.Distance(block.transform.position, pos) / _scaler < 1.1f)
            {
                DOVirtual.DelayedCall(0.07f, () => block.Bounce(dir)).SetLink(gameObject);
            }
        }
    }

    private void Validate()
    {
        _blockView.SetColor(_blockColorsScheme.GetMaterials(_color));

        static float Nearest(float value)
        {
            return Mathf.RoundToInt(value);
        }

        var pos = transform.localPosition;
        pos.x = Nearest(pos.x);
        pos.y = Nearest(pos.y);
        pos.z = Nearest(pos.z);
        transform.localPosition = pos;
        transform.name = $"Block {GetPosString(pos)} ({_color})";
        ValidateDirection();
    }

    private void ValidateDirection()
    {
        transform.localRotation = Quaternion.Euler(_direction switch
        {
            BlockDirection.up => new Vector3(-90f, 0f, 0f),
            BlockDirection.forward => new Vector3(0f, 0f, 0f),
            BlockDirection.left => new Vector3(0f, -90f, 0f),
            BlockDirection.back => new Vector3(0f, -180f, 0f),
            BlockDirection.right => new Vector3(0f, -270f, 0f),
            BlockDirection.down => new Vector3(90f, 0f, 0f),
            _ => Vector3.zero
        });
    }

    private string GetPosString(Vector3 pos)
    {
        return $"({(int)pos.x},{(int)pos.y},{(int)pos.z})";
    }
}

public enum BlockDirection
{
    up = 0,
    forward = 1,
    left = 2,
    back = 3,
    right = 4,
    down = 5
}

public enum BlockColor
{
    None,
    Green,
    Blue,
    RedPink,
    Black,
    White,
    Gray,
    Wood,
    GreenDark,
    Brown,
    BrownRed,
    Yellow,
    PinkDark,
    Red,
    Skin
}

public enum TapOnBlockResult
{
    NotTapped,
    WrongClick,
    RightClick
}
