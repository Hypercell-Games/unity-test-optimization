using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TileModelController : MonoBehaviour
{
    [SerializeField] private SkinsConfig _skinConfig;
    [SerializeField] private TilePressedScaleAnimation _tilePressedScaleAnimationSetting;

    [Space(10)] [SerializeField] private List<SpriteRenderer> _spritesForSorting;

    [SerializeField] private List<WrongDefaultGroup> _wrongDefaultGroups;

    [Space(10)] [SerializeField] private TileBaseSkinController _skinController;

    [Space(10)] [SerializeField] private Transform _effectsHolder;

    [SerializeField] private TrailRenderer _trailEffect;
    [SerializeField] private ParticleSystem _pathEffect;
    [SerializeField] private ParticleSystem _dieEffect;
    [SerializeField] private GameObject _keyEffect;

    [field: Space(10)]
    [field: SerializeField]
    public GameObject Key { get; private set; }

    [Header("Lock data")] [SerializeField] private GameObject _lockGroup;

    [SerializeField] private TextMeshPro _lockCounterLabel;
    [SerializeField] private SpriteRenderer _lockSpriteRenderer;
    [SerializeField] private ParticleSystem _jellyClickEffect;

    private SingleSkinConfigEntry _currentSkin;

    private EMoveDirection _moveDirection;
    private Vector3 _moveDirectionEulerAngles;
    private Sequence _pressSeq;

    private Transform _tileTransform;

    private void Awake()
    {
        SetTrailState(false);
    }

    public void Init(Transform tileTransform)
    {
        _tileTransform = tileTransform;
        _trailEffect.transform.SetParent(tileTransform, true);
    }

    public void ChangeRotation(EMoveDirection moveDirection, Vector3 eulerAngles)
    {
        _moveDirection = moveDirection;
        _moveDirectionEulerAngles = eulerAngles;

        RefreshRotation();
    }

    private void RefreshRotation()
    {
        _skinController.ChangeRotation(_moveDirection, _moveDirectionEulerAngles);
        _trailEffect.transform.eulerAngles = _moveDirectionEulerAngles;
        if (HasPathEffect())
        {
            _pathEffect.transform.eulerAngles = _moveDirectionEulerAngles;
        }
    }

    public void ChangeSortOrder(int sortOrder)
    {
        _lockCounterLabel.sortingOrder = sortOrder;
        _skinController.OnChangeSortOrder(sortOrder);

        BoardElementUtils.ChangeComponentsSortOrder(_trailEffect, sortOrder - 500);
        BoardElementUtils.ChangeComponentsSortOrder(_pathEffect, sortOrder - 500);
        BoardElementUtils.ChangeComponentsSortOrder(_dieEffect, sortOrder + 1000);
        BoardElementUtils.ChangeComponentsSortOrder(_jellyClickEffect, sortOrder + 1000);
    }

    public void SetTrailState(bool state)
    {
        _trailEffect.gameObject.SetActive(state);
        if (state)
        {
            var tps = _trailEffect.GetComponentInChildren<ParticleSystem>();
            if (tps)
            {
                tps.Stop();
                tps.Play();
            }
        }

        SetPathEffectState(state);
    }

    private void SetPathEffectState(bool state)
    {
        if (!HasPathEffect())
        {
            return;
        }

        var force = _currentSkin != null && _currentSkin.ForcePathEffect;

        if (GameConfig.RemoteConfig.tilePathEffectVersion == 0 && !force)
        {
            _pathEffect.gameObject.SetActive(false);
            return;
        }

        _pathEffect.gameObject.SetActive(state);
        if (state)
        {
            _pathEffect.Stop();
            _pathEffect.Play();
        }
    }

    private bool HasPathEffect()
    {
        return _pathEffect != null;
    }

    public void EnableTileEffect(ETileEffect tileEffect)
    {
        switch (tileEffect)
        {
            case ETileEffect.DIE:
                _dieEffect.transform.SetParent(null);
                _dieEffect.Play();
                break;

            case ETileEffect.JELLY:
                var effect = Instantiate(_jellyClickEffect, _lockGroup.transform);

                var shape = effect.shape;
                shape.scale = transform.localScale;

                effect.gameObject.SetActive(true);

                var anim = _lockSpriteRenderer.GetComponent<Animation>();
                anim.Play();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(tileEffect), tileEffect, null);
        }
    }

    public void UpdateActionsLock(int charges, bool forceEnabled = false)
    {
        _lockCounterLabel.text = charges.ToString();
        _lockGroup.SetActive(charges > 0 || forceEnabled);
    }

    public void FadeOutActionsLock()
    {
        _lockCounterLabel.gameObject.SetActive(false);
        _lockSpriteRenderer.DOFade(0f, 0.4f).OnComplete(() => UpdateActionsLock(0));
    }

    public void SetupTileEffect(float size)
    {
        var targetSize = size * 0.9f;

        _trailEffect.startWidth = targetSize;
        _trailEffect.endWidth = targetSize;
    }

    public void SetKeyEffect(bool isActive)
    {
        _keyEffect.SetActive(isActive);
    }

    private void SetEffectsColor(Color trailColor)
    {
        _trailEffect.startColor = trailColor;
        _trailEffect.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0.0f);

        var a = _dieEffect.main;
        a.startColor = Color.Lerp(trailColor, Color.white, 0.5f);

        if (HasPathEffect())
        {
            a = _pathEffect.main;
            a.startColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0.75f);
        }
    }

    public void SetSkin(SingleSkinConfigEntry skin, float cellSize)
    {
        _currentSkin = skin;

        SetEffects(skin);
        SetEffectsColor(skin.EffectsColor);
        SetupTileEffect(cellSize);
        InstantiateSkin(skin);

        SetTrailState(false);
    }

    private void InstantiateSkin(SingleSkinConfigEntry skin)
    {
    }

    private void SetEffects(SingleSkinConfigEntry skin)
    {
        _effectsHolder?.ClearChildren();

        if (skin.TrailEffect != null)
        {
            if (_trailEffect)
            {
                Destroy(_trailEffect.gameObject);
            }

            _trailEffect = Instantiate(skin.TrailEffect, _effectsHolder);
            _trailEffect.transform.localPosition = new Vector3(0, 0, 10f);
            _trailEffect.startWidth *= transform.lossyScale.x;
            _trailEffect.transform.SetParent(_tileTransform, true);
        }

        if (skin.PathEffect != null)
        {
            _pathEffect = Instantiate(skin.PathEffect, _effectsHolder);
            _pathEffect.transform.localPosition = Vector3.zero;
        }

        if (skin.DieEffect != null)
        {
            _dieEffect = Instantiate(skin.DieEffect, _effectsHolder);
            _dieEffect.transform.localPosition = Vector3.zero;
        }
    }

    public void OnTilePressed()
    {
        _skinController.OnTilePressed();
        _pressSeq?.Kill();
        _pressSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(_skinController.transform
                .DOScale(GameConfig.RemoteConfig.tilePressedScale, _tilePressedScaleAnimationSetting.PressedDuration)
                .SetEase(_tilePressedScaleAnimationSetting.PressedEase));
    }

    public void OnTileUnPressed()
    {
        _pressSeq?.Kill();
        _pressSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(_skinController.transform.DOScale(1f, _tilePressedScaleAnimationSetting.UnPressedDuration)
                .SetEase(_tilePressedScaleAnimationSetting.UnPressedEase));
        _skinController.OnTileUnPressed();
    }

    public List<SpriteRenderer> GetSpritesForSorting()
    {
        var list = new List<SpriteRenderer>(_skinController.SpritesForSorting);
        list.AddRange(_spritesForSorting);
        return list;
    }

    public List<WrongDefaultGroup> GetWrongDefaultGroups()
    {
        var list = new List<WrongDefaultGroup>(_skinController.WrongGroups);
        list.AddRange(_wrongDefaultGroups);

        return list;
    }

    public void OnBeforeDestroy()
    {
        _trailEffect.transform.SetParent(null, true);
        Destroy(_trailEffect.gameObject, 3);
    }

    public void OnStageLayerUpdate(bool isLocked)
    {
        var keyTransform = _keyEffect.transform;

        if (isLocked)
        {
            keyTransform.localScale = Vector3.zero;
            return;
        }

        if (keyTransform.localScale == Vector3.zero)
        {
            keyTransform.DOScale(1, 0.2f).SetEase(Ease.OutSine);
        }
    }
}
