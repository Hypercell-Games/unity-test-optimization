using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Unpuzzle;
using Random = UnityEngine.Random;

public class HookController : MonoBehaviour
{
    [SerializeField] private PinType _pinType;
    [SerializeField] private Rigidbody _rigidBody;
    [SerializeField] private List<MeshRenderer> _meshRenderers;
    [SerializeField] private List<HookMaterialApplier> _hookMaterialApplier;
    [SerializeField] private Transform _visual;
    [SerializeField] private PinFXController _pinFxController;

    [SerializeField] private bool _isColorOverrided;
    [SerializeField] private PinColorID _overrideColor;

    [SerializeField] private bool _materialOverrided;
    [SerializeField] private PinMaterialId _overridedMaterial;
    [SerializeField] private List<Transform> _boundsPivots;
    [SerializeField] private bool _isTarget;
    [SerializeField] private bool _isContainKey;
    [SerializeField] private bool _frozen;
    [SerializeField] private bool _isGhost;
    [SerializeField] private bool _isFire;
    [SerializeField] private IceBlock _iceBlock;
    [SerializeField] private List<ParticleSystem> _foamHidden;
    [SerializeField] private Collider[] _colliders = new Collider[0];
    [SerializeField] private HookHandle[] _hookHandles;
    [SerializeField] private List<HookController> _blockedPins;
    [SerializeField] private float _ghostRevealDistance = 5f;
    [SerializeField] private Transform _lenghtAchor;

    public PinOutLevel _pinOutLevel;

    public HookHandle hookHandle;

    [SerializeField] private List<BoxCollider> _hiddenCheckCollider = new();

    private readonly List<IceBlockController> _blockedByFrozenPins = new();
    private readonly List<HookController> _blockers = new();

    private readonly List<Material> _burnMaterials = new();

    private readonly List<GhostPinTriggeredInfo> _ghostPinsTriggered = new();

    private readonly int _layerIgnoreRaycast = 2;
    private readonly List<HookController> _pinsHiddenReveal = new();

    private readonly RaycastHit[] _shakeHits = new RaycastHit[40];

    private HookChain _cashedChain;
    private HookScalerCorner _cashedHookCorner;

    private HookScaler _cashedHookScaler;
    private HookScalerPartLine _cashedHookScalerPartLine;
    private HookScalerQuad _cashedHookScalerQuad;
    private HookScalerUniversal _cashedHookScalerUniversal;

    private Color _color;

    private PinColorID _colorId;
    private FireTrigger _fireTrigger;

    private bool _hintShowed;

    private IHookScaler _hookScaler;

    private int _iceBreakeCount = 3;
    private bool _isBurned;
    private Vector3 _lastDirection;
    private HookHandle _lastHandle;

    private Tween _materialTweening;
    private Sequence _moveOutSeq;
    private bool _moving;

    private Vector3 _movingVector;
    private Collider[] _physicsColliders;
    private PinMaterial _pinMaterial;

    private Shader _shader;
    private int _shakeOrder = int.MaxValue;

    private Vector3 _startPos;
    private Sequence _visualMoveOutSeq;
    public Action<HookController> OnBlockedByIce = hook => { };
    public Action<HookController> OnDestroyIce = hook => { };

    public Action<HookController, bool> OnMoveOut = (hook, removedByHook) => { };
    public Action<HookController> OnPostMoveOut = hook => { };

    [NonSerialized] public PinOutLevelDataElement SaveData;

    public Collider[] Colliders => _colliders;

    public PinType pinType => _pinType;

    public List<HookController> BlockedPins => _blockedPins;

    public bool IsInActive => _blockers != null && _blockers.Count > 0;

    public IceBlock IceBlock
    {
        get => _iceBlock;
        set => _iceBlock = value;
    }

    public bool IsFrozen
    {
        get => _frozen;
        set
        {
            _frozen = value;
            if (_frozen)
            {
                _isGhost = false;
            }
        }
    }

    public bool IsTarget
    {
        get => _isTarget;
        set
        {
            _isTarget = value;
            if (_isTarget)
            {
                _isContainKey = false;
            }
        }
    }

    public bool IsContainKey
    {
        get => _isContainKey;
        set
        {
            _isContainKey = value;

            if (_isContainKey)
            {
                _isTarget = false;
            }
        }
    }

    public bool IsFire
    {
        get => _isFire;
        set => _isFire = value;
    }

    public List<MeshRenderer> MeshRenderers => _meshRenderers;

    public List<Transform> BoundPivots => _boundsPivots;

    public bool IsMaterialOverrided
    {
        get => _materialOverrided;
        set => _materialOverrided = value;
    }

    public PinMaterialId OverridedMaterial
    {
        get => _overridedMaterial;
        set => _overridedMaterial = value;
    }

    public bool IsColorOverrided
    {
        get => _isColorOverrided;
        set => _isColorOverrided = value;
    }

    public PinColorID OverrideColor
    {
        set => _overrideColor = value;
        get => _overrideColor;
    }

    public Color Color => _color;
    public Shader OutlineShader => _pinMaterial.OutlineShader;

    public bool CanMoveOut => !_moving && !IsReturning && !_isGhost;

    public bool IsReturning { get; private set; }

    public List<HookPinTargetSwitcher> TargetSwitchers { get; } = new();

    public bool Removed { get; private set; }

    public bool IsGhost
    {
        get => _isGhost;
        set
        {
            _isGhost = value;
            if (value)
            {
                Array.ForEach(_colliders, a =>
                {
                    Debug.Log("SET LAYER");
                    a.gameObject.SetLayer(_layerIgnoreRaycast);
                });
            }
        }
    }

    private void Update()
    {
        if (_moving && _ghostPinsTriggered.Count > 0)
        {
            var revealed = new List<GhostPinTriggeredInfo>();
            var distance = _ghostRevealDistance;
            if (_lenghtAchor)
            {
                distance += transform.InverseTransformPoint(_lenghtAchor.position).magnitude;
            }

            for (var i = 0; i < _ghostPinsTriggered.Count; i++)
            {
                var ghostPosition = _ghostPinsTriggered[i]._ghost.transform.position - transform.position;
                var touchOffset = Vector3.Project(ghostPosition, _lastDirection).magnitude;
                if (touchOffset > distance)
                {
                    ParticlesSpawner.Instance.SpawnParticle(ParticleId.reveal, _ghostPinsTriggered[i]._touchPoint,
                        transform.rotation, true);
                    revealed.Add(_ghostPinsTriggered[i]);
                    _ghostPinsTriggered[i]._ghost.Reveal(this);
                }
            }

            _ghostPinsTriggered.RemoveAll(x => revealed.Contains(x));
        }
    }

    public void CacheRefs()
    {
        _cashedHookScaler = GetComponent<HookScaler>();
        _cashedHookScalerQuad = GetComponent<HookScalerQuad>();
        _cashedHookCorner = GetComponent<HookScalerCorner>();
        _cashedHookScalerUniversal = GetComponent<HookScalerUniversal>();
        _cashedHookScalerPartLine = GetComponent<HookScalerPartLine>();
        _cashedChain = GetComponent<HookChain>();
    }

    public void UpdateValues(PinOutLevelDataElement data)
    {
        _isColorOverrided = data.colOvrd;
        _overrideColor = data.colId;
        _materialOverrided = data.matOvrd;
        _overridedMaterial = data.matId;

        if (_cashedHookScaler)
        {
            _cashedHookScaler.UpdateValues(data.scaled, data.pinScl.y);
        }

        if (_cashedHookScalerQuad)
        {
            _cashedHookScalerQuad.UpdateValues(data.scaled, data.pinScl);
        }

        if (_cashedHookCorner)
        {
            _cashedHookCorner.UpdateValues(data.scaled, data.pinScl);
        }

        if (_cashedHookScalerUniversal)
        {
            _cashedHookScalerUniversal.UpdateValues(data.scaled, data.pinScl);
        }

        if (_cashedChain)
        {
            _cashedChain.UpdateValue(data.chains, data.chainsScl, data.chainRot);
        }

        if (_cashedHookScalerPartLine)
        {
            _cashedHookScalerPartLine.UpdateValues(data.scaled, data.pinScl.x, data.pinScl.y);
        }
    }

    public void UpdateMaterial()
    {
        if (_isGhost)
        {
            var ghostMaterial = ColorHolder.Instance.TransparentMaterial;
            for (var i = 0; i < _meshRenderers.Count; i++)
            {
                _meshRenderers[i].material = ghostMaterial;
            }

            TargetSwitchers.ForEach(a => a.SetInActiveMaterial(ghostMaterial));

            return;
        }

        if (_isFire)
        {
            var fire = ColorHolder.Instance.FireMaterial;
            for (var i = 0; i < _meshRenderers.Count; i++)
            {
                _meshRenderers[i].material = fire;
            }

            TargetSwitchers.ForEach(a => a.SetInActiveMaterial(fire));

            return;
        }


        if (IsInActive)
        {
            var inActiveMaterial = _pinMaterial.InActiveMaterial;
            for (var i = 0; i < _meshRenderers.Count; i++)
            {
                _meshRenderers[i].material = inActiveMaterial;
            }

            TargetSwitchers.ForEach(a => a.SetInActiveMaterial(inActiveMaterial));

            return;
        }

        var propertyBlock = ColorHolder.Instance.GetPropertyBlockData(_pinMaterial.MaterialId, _colorId);
        var material = pinType == PinType.pinBolt ? ColorHolder.Instance.BoltPinMaterial : propertyBlock.GetMaterial;

        for (var i = 0; i < _meshRenderers.Count; i++)
        {
            _meshRenderers[i].SetPropertyBlock(null);
            _meshRenderers[i].material = material;
        }


        TargetSwitchers.ForEach(a => a.UpdateMaterial());

        if (_pinMaterial.NeedUvCorrection)
        {
            _hookMaterialApplier.ForEach(a => a.ApplyMaterial(material));
        }
    }

    public void AddTargetSwitcher(HookPinTargetSwitcher switcher)
    {
        TargetSwitchers.Add(switcher);
    }

    public Vector3 GetBounds()
    {
        var bounds = Vector3.zero;

        _boundsPivots.ForEach(a =>
        {
            var boundPos = transform.InverseTransformPoint(a.position);
            bounds.x = Mathf.Max(bounds.x, boundPos.x);
            bounds.y = Mathf.Max(bounds.y, boundPos.y);
            bounds.z = Mathf.Max(bounds.z, boundPos.z);
        });

        return bounds;
    }

    public int GetTargetCount()
    {
        if (!_isTarget)
        {
            return 0;
        }

        return TargetSwitchers.Count;
    }

    public void Initialize()
    {
        _rigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        _frozen = _frozen && _iceBlock;

        if (_iceBlock)
        {
            _iceBlock.gameObject.SetActive(_frozen);
        }

        if (_isFire)
        {
            foreach (var collider in _colliders)
            {
                var boxColider = collider as BoxCollider;

                if (boxColider == null)
                {
                    continue;
                }

                var particleCount = Mathf.Max(boxColider.size.x, boxColider.size.y, boxColider.size.z) / 5f;
                for (var i = 0; i < particleCount; i++)
                {
                    var position =
                        boxColider.transform.TransformPoint(boxColider.center +
                                                            GetRandomPositionInBoxCollider(boxColider));

                    var fire = ParticlesSpawner.Instance.GetFireParticle(collider.transform, position);
                }
            }
        }

        _startPos = transform.localPosition;
        _pinMaterial = !ColorHolder._isMaterialOverride && _materialOverrided
            ? ColorHolder.Instance.GetMaterial(_overridedMaterial)
            : ColorHolder.Instance.GetMaterial();
        var pinColor = _isColorOverrided
            ? ColorHolder.Instance.GetColor(_pinMaterial, OverrideColor)
            : ColorHolder.Instance.GetRandomColor(_pinMaterial);
        _color = pinColor.PrimaryColor;
        _color.a = _pinMaterial.Alpha;
        _shader = GameConfig.RemoteConfig.outlineVariant && _pinMaterial.OutlineVariant != null
            ? _pinMaterial.OutlineVariant
            : _pinMaterial.Material.shader;

        _colorId = pinColor.ColorId;

        _pinFxController.SetTrailColor(_color);
        UpdateMaterial();
        TargetSwitchers.ForEach(a => a.UpdatePin());

        Vector3 GetRandomPositionInBoxCollider(BoxCollider collider)
        {
            var halfSize = collider.size / 2f;
            return new Vector3(
                Random.Range(-halfSize.x, halfSize.x),
                Random.Range(-halfSize.y, halfSize.y),
                Random.Range(-halfSize.z, halfSize.z));
        }

        _physicsColliders = new Collider[_colliders.Length];
        for (var i = 0; i < _physicsColliders.Length; i++)
        {
            var collider = _colliders[i];
            var physicsCollider = Instantiate(collider, collider.transform.position, collider.transform.rotation,
                collider.transform.parent);
            physicsCollider.isTrigger = false;
            physicsCollider.name = $"{collider.name}_Physics";
            physicsCollider.gameObject.layer = LayersUtils.PhysicsLayer;
            _physicsColliders[i] = physicsCollider;
        }

        _rigidBody.isKinematic = true;
    }

    public void SetBlockedPins(List<HookController> hookController)
    {
        _blockedPins = hookController;
        _blockedPins.ForEach(a => a.AddActivator(this));
    }

    public void AddActivator(HookController hookController)
    {
        _blockers.Add(hookController);
    }

    public void RemoveActivator(HookController hookController)
    {
        _blockers.Remove(hookController);
        if (_blockers.Count == 0)
        {
            _visual.transform.localScale = Vector3.one;
            _visual.DOScale(1.1f, 0.1f)
                .SetLoops(2, LoopType.Yoyo);


            UpdateMaterial();
        }
    }

    [ContextMenu("Check frozen")]
    public List<HookController> CheckAndSetBlockedByFrozen()
    {
        if (!_frozen)
        {
            return null;
        }

        var result = new List<HookController>();


        return result;
    }

    private void CheckRevealHiddenPins()
    {
        foreach (var overlapCollider in _hiddenCheckCollider)
        {
            var overlapTransform = overlapCollider.transform;
            var colliders = Physics.OverlapBox(overlapTransform.TransformPoint(overlapCollider.center),
                overlapCollider.size / 2f, overlapTransform.rotation);

            if (colliders.Length == 0)
            {
                continue;
            }

            foreach (var collider in colliders)
            {
                var pinPart = collider.GetComponent<HookPart>();

                if (pinPart == null || pinPart.Controller == null)
                {
                    continue;
                }

                if (pinPart.Controller == this)
                {
                    continue;
                }

                if (_pinsHiddenReveal.Contains(pinPart.Controller))
                {
                    continue;
                }

                _pinsHiddenReveal.Add(pinPart.Controller);
                pinPart.Controller.OnPostMoveOut += Reveal;
            }
        }
    }

    public void Reveal(HookController moveOutPin)
    {
        if (!_isGhost)
        {
            return;
        }

        SaveData.sRevealed = 1;
        Array.ForEach(_colliders, a => a.gameObject.SetLayer(0));
        _isGhost = false;
        var moveOutPinVector = moveOutPin.transform.up * 1000;
        _visual.DOPunchScale(Vector3.one * 0.1f, 0.1f);
        UpdateMaterial();
        TargetSwitchers.ForEach(a => a.UpdateMaterial());
    }

    public void AddCheckRevealPinColldier(BoxCollider collider)
    {
        _hiddenCheckCollider.Add(collider);
    }

    public void AddFrozenBlock(IceBlockController iceBlock)
    {
        _blockedByFrozenPins.Add(iceBlock);
    }

    public void OnTouchStaticObject(ICollisionElement staticObjectPart)
    {
        if (!_moving)
        {
            return;
        }

        CancellMoving();
        staticObjectPart.TouchFeedack();
    }

    public void OnPartCollision(HookController otherHook, Vector3 closestPoint)
    {
        if (otherHook._pinOutLevel != _pinOutLevel)
        {
            return;
        }

        if (!_moving)
        {
            return;
        }

        if (otherHook.IsGhost)
        {
            if (_ghostPinsTriggered.Find(a => a._ghost == otherHook) != null)
            {
                return;
            }

            _ghostPinsTriggered.Add(new GhostPinTriggeredInfo
            {
                _ghost = otherHook,
                _touchedDistance =
                    Vector3.Project(otherHook.transform.position - transform.position, _lastDirection),
                _touchPoint = closestPoint
            });
            return;
        }

        if (Removed)
        {
            return;
        }

        if (otherHook.IsFrozen)
        {
            OnBlockedByIce(this);
            otherHook.IceTouch();

            if (!otherHook.IsFrozen)
            {
                OnDestroyIce(this);
            }
        }
        else
        {
            otherHook.Touches(_movingVector);
        }

        CancellMoving();
    }

    public void EnableTutorial(bool isBoosterUsed = false)
    {
        _hintShowed = true;

        if (isBoosterUsed)
        {
            TargetSwitchers.ForEach(a => a.SetTutorialLayer());
        }
    }

    public void DisableTutorial(HookController pressedHook)
    {
        _hintShowed = false;
        _pinFxController.TutorialDisable();
    }

    public void AddBoundPivot(Transform boundPivot)
    {
        _boundsPivots.Add(boundPivot);
    }

    public void Press(Vector3 direction)
    {
        direction = transform.InverseTransformVector(direction);
        _visualMoveOutSeq?.Kill();
        _visual.DOKill();
        _visual.DOScale(0.95f, 0.1f);
        _visual.DOLocalMove(-direction, 0.1f)
            .OnComplete(() =>
            {
                _visual.DOShakePosition(0.2f, 0.3f, 25)
                    .SetLoops(-1, LoopType.Restart);
            });

        _pinFxController.OnPress();
    }

    public void Unpress()
    {
        _visualMoveOutSeq?.Kill();
        _visual.DOKill();
        _visual.DOScale(1f, 0.1f);
        _visual.DOLocalMove(Vector3.zero, 0.1f);

        _pinFxController.OnUp();
    }

    public void Shake(Vector3 direction)
    {
        _visual.DOShakePosition(GameConfig.RemoteConfig.blockedShakeStrenght, 0.5f);
    }

    public void IceTouch()
    {
        _iceBreakeCount--;

        VibrationController.Instance.Play(EVibrationType.LightImpact);

        if (_iceBreakeCount > 0)
        {
            _iceBlock.Punch(3 - _iceBreakeCount);
            return;
        }

        _iceBlock.Breake();
        _frozen = false;
    }

    public void SetRemoved()
    {
        Removed = true;
        SaveData.sIsDestroyed = 1;
    }

    public void Burn(Vector3 position)
    {
        _pinFxController.DisableFx();

        SetRemoved();

        TargetSwitchers.ForEach(a => _burnMaterials.AddRange(a.SetBurnMaterial(position)));
        var material = new Material(_meshRenderers[0].material);
        _burnMaterials.Add(material);
        _meshRenderers.ForEach(a =>
        {
            a.material = material;
        });

        _burnMaterials.ForEach(burnMaterial =>
        {
            burnMaterial.shader = ColorHolder.Instance.FireShader;
            burnMaterial.SetVector("_FireSource", position);
            burnMaterial.renderQueue = 3001;
        });

        var burnEffect = ParticlesSpawner.Instance.GetBurnEffect();
        burnEffect.transform.position = transform.position;
        burnEffect.transform.rotation = transform.rotation;
        burnEffect.transform.SetParent(transform.parent);
        burnEffect.SetAndPlay(GetBounds());

        DOTween.To(x =>
            {
                var radius = x * 40f;
                _burnMaterials.ForEach(burnMaterial =>
                {
                    burnMaterial.SetFloat("_FireRadius", radius);
                });
            }, 0f, 1f, 0.5f)
            .SetEase(Ease.InSine)
            .SetDelay(0.2f)
            .OnComplete(() =>
            {
                OnMoveOutInvoke(this, false);
                gameObject.SetActive(false);
            });

        _isBurned = true;
    }

    public void BoosterRemovePin()
    {
        if (IsFrozen)
        {
            _iceBlock.Breake();
            _frozen = false;
            Removed = false;
            return;
        }

        Removed = true;

        if (_isTarget && TargetSwitchers.Count > 0)
        {
            TargetSwitchers.ForEach(a =>
            {
                a.TargetPin.SetLayer(6);
                a.DefaultAppear();
            });
        }

        PinRemoved();
        gameObject.SetActive(false);
        OnMoveOutInvoke(this, true);
        OnPostMoveOutInvoke(this);
    }

    public void OnMoveOutInvoke(HookController hook, bool removedByHook)
    {
        var startPos = _lastHandle ? _lastHandle.transform.position : _hookHandles[0].transform.position;
        _blockedPins.ForEach(a =>
        {
            var fireFly = ParticlesSpawner.Instance.GetActivatorFireFly();
            fireFly.StartFly(startPos, a.transform.position, _lastDirection, () =>
            {
                a.RemoveActivator(this);
            });
        });
        OnMoveOut(hook, removedByHook);
    }

    public void OnPostMoveOutInvoke(HookController hook)
    {
        SaveData.sIsDestroyed = 1;
        _ghostPinsTriggered.ForEach(a => a._ghost.Reveal(this));
        _ghostPinsTriggered.Clear();
        OnPostMoveOut?.Invoke(hook);
    }

    public void PinRemoved()
    {
        if (_isFire && _fireTrigger != null)
        {
            _fireTrigger.DestroyFiredItems();
        }
    }

    public void Touches(Vector3 force)
    {
        VibrationController.Instance.Play(EVibrationType.MediumImpact);

        _materialTweening?.Kill(true);

        if (!IsInActive && !_isFire && !_isBurned)
        {
            _pinFxController.BlockFXActivate(_color);
        }

        _pinOutLevel.Hooks.ForEach(a =>
        {
            if (a != this)
            {
                a.Shake(force);
            }
        });

        _visualMoveOutSeq?.Kill();
        _visual.DOKill();
        _visual.localScale = Vector3.one;
        _visual.DOLocalMove(transform.InverseTransformVector(force) * 0.3f, 0.2f)
            .OnComplete(() =>
            {
                _visual.DOLocalMove(Vector3.zero, 0.1f);
            });
    }

    private void CancellMoving()
    {
        _ghostPinsTriggered.Clear();
        _pinFxController.TrailFXActivate(null);
        _pinOutLevel.MovingBlocked();
        _moving = false;
        _moveOutSeq?.Kill();
        transform.DOKill();
        _rigidBody.DOKill();
        IsReturning = true;

        _visualMoveOutSeq?.Kill();
        _visual.DOKill();
        _visual.DOLocalMove(Vector3.zero, 0.3f);
        _visual.DOScale(Vector3.one, 0.3f);

        if (_isFire)
        {
            BoosterRemovePin();
            LevelsController.Instance.CurrentGameController.CheckMoves();
            return;
        }


        if (_lastHandle)
        {
            _lastHandle.ResetColliderSize();
        }

        LevelsController.Instance.CurrentGameController.CheckMoves();

        var moveTween =
            _rigidBody.DOMove(transform.parent.TransformPoint(_startPos), 0.3f)
                .SetUpdate(UpdateType.Fixed)
                .SetEase(Ease.OutCirc)
                .OnComplete(() =>
                {
                    IsReturning = false;
                    _rigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                });

        if (_pinType == PinType.pinBolt)
        {
            moveTween.OnUpdate(() =>
            {
                _visual.Rotate(Vector3.up, -900f * Time.deltaTime, Space.Self);
            });
        }
    }

    public void SetupMeshes(List<MeshRenderer> meshes)
    {
        _meshRenderers = meshes;
    }

    public void SetupColliders(Collider[] colliders)
    {
        _colliders = colliders;
    }

    public void SetupMaterialApplier(List<HookMaterialApplier> materialAppliers)
    {
        _hookMaterialApplier = materialAppliers;
    }

    public void ForceMoveOut(float delay)
    {
        Removed = true;
        MoveOutAnimation(hookHandle.transform.up, true, delay);
        _rigidBody.isKinematic = true;
    }

    private void MoveOutAnimation(Vector3 direction, bool force, float delay = 0f)
    {
        const float distance = 100f;
        var speed = GameConfig.RemoteConfig.speedToHide;
        var duration = distance / speed;
        var scaleDelay = Mathf.Max(duration - 0.5f, 0f);

        _moveOutSeq?.Kill();
        _moveOutSeq = DOTween.Sequence().SetLink(gameObject)
            .SetUpdate(UpdateType.Normal);

        var ghostPinRevealSeq =
            DOTween.Sequence()
                .SetLink(gameObject)
                .SetUpdate(UpdateType.Normal)
                .AppendInterval(scaleDelay)
                .AppendCallback(() =>
                {
                    Removed = true;
                    _ghostPinsTriggered.ForEach(a => a._ghost.Reveal(this));
                    _ghostPinsTriggered.Clear();
                    PinRemoved();
                });

        if (delay > 0f)
        {
            _moveOutSeq.SetDelay(delay);
        }

        _moveOutSeq.Append(
            _rigidBody.DOMove(_rigidBody.position + direction * distance, duration)
                .OnUpdate(() =>
                {
                    if (_pinType == PinType.pinBolt)
                    {
                        _visual.Rotate(Vector3.up, -900f * Time.deltaTime, Space.Self);
                    }
                })
                .SetUpdate(UpdateType.Fixed));
        _moveOutSeq.Join(transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetDelay(scaleDelay));
        _moveOutSeq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public bool CanMoveOutTutorial()
    {
        for (var i = 0; i < _hookHandles.Length; i++)
        {
            var hookHandle = _hookHandles[i];
            if (CanMoveOutHandle())
            {
                return true;
            }

            bool CanMoveOutHandle()
            {
                var direction = hookHandle.transform.up;
                for (var j = 0; j < _colliders.Length; j++)
                {
                    var collider = _colliders[j];
                    switch (collider)
                    {
                        case BoxCollider boxCollider:
                            var pos = boxCollider.transform.TransformPoint(boxCollider.center);
                            var halfExtens = boxCollider.size * 0.5f;
                            var rotation = boxCollider.transform.rotation;
                            var hitColliders = new RaycastHit[10000];
                            var hitsCount = collider.gameObject.scene.GetPhysicsScene()
                                .BoxCast(pos, halfExtens, direction, hitColliders, rotation);
                            for (var k = 0; k < hitsCount; k++)
                            {
                                var hit = hitColliders[k];
                                var hitCollider = hit.collider;
                                if (_colliders.Contains(hitCollider))
                                {
                                    continue;
                                }

                                if (hitCollider.gameObject.GetComponent<HookPart>() != null)
                                {
                                    return false;
                                }
                            }

                            break;
                    }
                }

                return true;
            }
        }

        return false;
    }

    public List<List<HookController>> GetNextHooks()
    {
        var handles = new List<List<HookController>>();
        for (var i = 0; i < _hookHandles.Length; i++)
        {
            var hookHandle = _hookHandles[i];
            var direction = hookHandle.transform.up;
            var handleHooks = new List<HookController>();
            handles.Add(handleHooks);
            for (var j = 0; j < _colliders.Length; j++)
            {
                var collider = _colliders[j];
                switch (collider)
                {
                    case BoxCollider boxCollider:
                        var pos = boxCollider.transform.TransformPoint(boxCollider.center);
                        var halfExtens = boxCollider.size * 0.5f;
                        var rotation = boxCollider.transform.rotation;
                        var hitColliders = new RaycastHit[10000];
                        var hitsCount = collider.gameObject.scene.GetPhysicsScene()
                            .BoxCast(pos, halfExtens, direction, hitColliders, rotation);
                        for (var k = 0; k < hitsCount; k++)
                        {
                            var hit = hitColliders[k];
                            var hitCollider = hit.collider;
                            if (_colliders.Contains(hitCollider))
                            {
                                continue;
                            }

                            if (!hitCollider.gameObject.TryGetComponent<HookPart>(out var hookPart))
                            {
                                continue;
                            }

                            var hookController = hookPart.Controller;
                            if (hookController &&
                                hookController != this &&
                                !handleHooks.Contains(hookController))
                            {
                                handleHooks.Add(hookController);
                            }
                        }

                        break;
                }
            }
        }

        return handles;
    }

    public bool MoveOut(Vector3 direction, Vector3 rightVector, Vector3 forwardVector, HookHandle handle,
        TrailRenderer trail, HookMoveOutRaycaster raycaster)
    {
        if (_moving || IsReturning || _isBurned)
        {
            return false;
        }

        _pinFxController.TutorialDisable();
        _ghostPinsTriggered.Clear();

        if (_frozen || _blockers.Count > 0 || _blockedByFrozenPins.Any(a => a.IsActive))
        {
            _pinOutLevel.OnPreMove();
            Touches(Vector3.zero);
            LevelsController.Instance.CurrentGameController.CheckMoves();
            return true;
        }

        if (_isFire)
        {
            var fireParticle = ParticlesSpawner.Instance.GetFireTrigger(transform, transform.position,
                Quaternion.LookRotation(direction, transform.forward));
            fireParticle.SetScaleAndParent(this, _meshRenderers.Select(a => a.transform).ToList());
            _fireTrigger = fireParticle;
        }

        _lastDirection = direction;
        _lastHandle = handle;


        if (_lastHandle)
        {
            _lastHandle.IncreaseColliderSize();
        }

        VibrationController.Instance.Play(EVibrationType.LightImpact);
        _hookScaler ??= GetComponent<IHookScaler>();

        _movingVector = direction;
        _moving = true;
        var time = GameConfig.RemoteConfig.timeToPinOut;
        _pinFxController.TrailFXActivate(trail);

        _visualMoveOutSeq?.Kill();
        _visual.DOKill();
        _visual.localPosition = Vector3.zero;

        const float scaleY = 1.3f;
        var scaleXZ = 1f + (1f - scaleY);
        var visualFinishScale = Vector3.one +
                                _visual.InverseTransformDirection(direction).Abs() * (scaleY - 1f) -
                                _visual.InverseTransformDirection(rightVector + forwardVector).Abs() * (1f - scaleXZ);

        _visualMoveOutSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(_visual.transform.DOScale(visualFinishScale, time * 2f));

        var hitPos = Vector3.zero;

        var canMoveOut = raycaster != null && raycaster.CanMoveOut(this, out hitPos);

        if (GameConfig.RemoteConfig.newPinMoveOutScaleAnimation)
        {
            _visualMoveOutSeq.Append(_visual.DOScale(Vector3.one, time * 3f).SetEase(Ease.OutQuad));
        }

        if (canMoveOut)
        {
            _rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Removed = true;
            MoveOutAnimation(direction, false);
            _rigidBody.isKinematic = true;
            OnMoveOutInvoke(this, false);

            if (TargetSwitchers.Count > 0)
            {
                if (_isTarget)
                {
                    TargetSwitchers.ForEach(a =>
                    {
                        a.TargetPin.SetLayer(6);
                        a.DefaultAppear();
                    });
                }
                else
                {
                    TargetSwitchers.ForEach(a =>
                    {
                        var particle = ParticlesSpawner.Instance.SpawnParticle(ParticleId.rightPinClick,
                            a.transform.position, Quaternion.identity, true);
                    });
                }
            }

            OnPostMoveOutInvoke(this);

            _pinOutLevel.OnPreMove();
            return true;
        }

        var hookScale = _hookScaler == null ? 15f : _hookScaler.GetPinLenght();
        var flyposition = _rigidBody.position + direction * hookScale;

        var speed = flyposition.magnitude / time;

        if (speed > 150)
        {
            time = flyposition.magnitude / 150;
        }

        if (raycaster)
        {
            var distance = Vector3.Distance(hitPos, _rigidBody.position);
            flyposition = _rigidBody.position + direction * (distance + hookScale + 3f);
        }


        _pinOutLevel.OnPreMove();
        _rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        var moveTween =
            _rigidBody.DOMove(flyposition, time)
                .SetEase(Ease.Linear)
                .SetUpdate(UpdateType.Fixed)
                .OnComplete(() =>
                {
                    PinRemoved();
                    Removed = true;
                    MoveOutAnimation(direction, false);
                    _rigidBody.isKinematic = true;
                    OnMoveOutInvoke(this, false);

                    if (TargetSwitchers.Count > 0)
                    {
                        if (_isTarget)
                        {
                            TargetSwitchers.ForEach(a =>
                            {
                                a.TargetPin.SetLayer(6);
                                a.DefaultAppear();
                            });
                        }
                        else
                        {
                            TargetSwitchers.ForEach(a =>
                            {
                                ParticlesSpawner.Instance.SpawnParticle(ParticleId.rightPinClick, a.transform.position,
                                    Quaternion.identity, true);
                            });
                        }
                    }

                    OnPostMoveOutInvoke(this);
                });

        if (_pinType == PinType.pinBolt)
        {
            moveTween.OnUpdate(() =>
            {
                _visual.Rotate(Vector3.up, -900f * Time.deltaTime, Space.Self);
            });
        }

        return true;
    }

    public class GhostPinTriggeredInfo
    {
        public HookController _ghost;
        public Vector3 _touchedDistance;
        public Vector3 _touchPoint;
    }
}

public enum PinType
{
    loop = 0,
    single = 1,
    quad = 2,
    fork = 3,
    loopDouble = 4,
    snake = 5,
    chain = 6,
    corner = 7,
    T = 9,

    pinBolt = 40,

    partHalfLoop = 50,
    partLine = 51
}
