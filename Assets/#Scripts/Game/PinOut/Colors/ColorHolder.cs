using System;
using System.Collections.Generic;
using UnityEngine;
using Unpuzzle;

namespace Unpuzzle
{
    public class ColorHolder : MonoSingleton<ColorHolder>
    {
        public static bool _isMaterialOverride;
        public static PinMaterialId _overridedMaterial;
        [SerializeField] private Color _touchColor;
        [SerializeField] private List<PinColor> _pinColors;
        [SerializeField] private PinColor _pinHandleColor;
        [SerializeField] private PinColor _starHandleColor;
        [SerializeField] private List<PinMaterial> _pinMaterials;
        [SerializeField] private Material _outlinePinHandleMaterial;
        [SerializeField] private Material _outlineStarMaterial;
        [SerializeField] private Material _defaultPinMaterial;
        [SerializeField] private Material _defaultStarMaterial;
        [SerializeField] private Material _activatorHandleMain;
        [SerializeField] private Material _activatorHandleSecond;

        [SerializeField] private Material _voidStateMaterial;
        [SerializeField] private Material _transparentMaterial;
        [SerializeField] private Material _boltPinMaterial;
        [SerializeField] private Material _fireMaterial;
        [SerializeField] private Shader _fireShader;

        private PinMaterialId _currentMaterialId;

        private int _handlePinPeriod;

        private List<PinMaterialPropertyBlockList> _propertyBlocksDatas;

        private int colorIndex;

        public Material OutlinePinHandleMaterial => _outlinePinHandleMaterial;
        public Material OutlineStarMaterial => _outlineStarMaterial;
        public Material DefaultPinMaterial => _defaultPinMaterial;
        public Material DefaultStarMaterial => _defaultStarMaterial;

        public Material VoidStateMaterial => _voidStateMaterial;

        public Material TransparentMaterial => _transparentMaterial;

        public Material ActivatorHandleMain => _activatorHandleMain;
        public Material ActivatorHandleSecond => _activatorHandleSecond;

        public Material BoltPinMaterial => _boltPinMaterial;

        public Material FireMaterial => _fireMaterial;
        public Shader FireShader => _fireShader;

        public Material GetDefaultHandlePinMaterial()
        {
            return _defaultPinMaterial;
        }

        protected override void OnInstanced()
        {
            _propertyBlocksDatas = new List<PinMaterialPropertyBlockList>();
            CreateHandlePropertiesBlocks();
        }

        private void CreateHandlePropertiesBlocks()
        {
        }

        public PinColor GetRandomColor()
        {
            colorIndex++;
            return _pinColors[colorIndex % _pinColors.Count];
        }

        public PinColor GetRandomColor(PinMaterial material)
        {
            var pinColor = GetRandomColor();
            var overridedPinColor = material.TryGetOverridedColor(pinColor.ColorId);

            if (overridedPinColor != null)
            {
                return overridedPinColor;
            }

            return pinColor;
        }

        public PinColor GetColor(int index)
        {
            return _pinColors[index % _pinColors.Count];
        }

        public PinColor GetColor(PinColorID colorId)
        {
            return _pinColors.Find(a => a.ColorId == colorId);
        }

        public PinColor GetColor(PinMaterial material, PinColorID colorId)
        {
            var pinColor = material.TryGetOverridedColor(colorId);

            if (pinColor != null)
            {
                return pinColor;
            }

            return _pinColors.Find(a => a.ColorId == colorId);
        }

        public Color GetTouchColor()
        {
            return _touchColor;
        }

        public PinMaterial GetMaterial(PinMaterialId materialId)
        {
            return _pinMaterials.Find(a => a.MaterialId == materialId);
        }

        public void SetMaterial(PinMaterialId pinMaterialId)
        {
            if (_isMaterialOverride)
            {
                _currentMaterialId = _overridedMaterial;
                return;
            }

            _currentMaterialId = pinMaterialId;
        }

        public PinMaterial GetMaterial()
        {
            return GetMaterial(_currentMaterialId);
        }

        private PinMaterialPropertyBlockList FindOrCreaterPinMaterialPropertyBlock(PinMaterialId materialId)
        {
            var pinMaterialData = _propertyBlocksDatas.Find(a => a.PinMaterialId == materialId);

            if (pinMaterialData == null)
            {
                pinMaterialData = new PinMaterialPropertyBlockList(materialId, GetMaterial(materialId));
                _propertyBlocksDatas.Add(pinMaterialData);
            }

            return pinMaterialData;
        }

        public PinMaterialPropertyBlock GetPropertyBlockData(PinMaterialId pinMaterialId, PinColorID colorId)
        {
            var materialPropertyDatas = FindOrCreaterPinMaterialPropertyBlock(pinMaterialId);

            return materialPropertyDatas.GetProperyBlock(colorId);
        }
    }
}

[Serializable]
public class PinColor
{
    [SerializeField] private PinColorID _colorId;
    [SerializeField] private Color _primaryColor;
    [SerializeField] private Color _selfShadowColor;
    [SerializeField] private Color _FalloffColor;
    [SerializeField] private Color _outlineColor;
    [SerializeField] private Color _secondaryColor;

    public Color PrimaryColor => _primaryColor;
    public Color OutlineColor => _outlineColor;
    public Color SecondaryColor => _secondaryColor;
    public Color SelfShadowColor => _selfShadowColor;
    public Color FalloffColor => _FalloffColor;

    public PinColorID ColorId => _colorId;
}

[Serializable]
public class PinMaterial
{
    [SerializeField] private PinMaterialId _materialId;
    [SerializeField] private Material _material;
    [SerializeField] private float _alpha = 1f;
    [SerializeField] private List<PinColor> _pinColors;
    [SerializeField] private bool _needUvCorrection;
    [SerializeField] private bool _secondaryColorEnabled;
    [SerializeField] private Shader _outlineShader;
    [SerializeField] private Shader _outlineVariant;
    [SerializeField] private Material _inActiveMaterial;

    public bool NeedUvCorrection => _needUvCorrection;
    public bool SecondaryColorEnabled => _secondaryColorEnabled;

    public Material InActiveMaterial => _inActiveMaterial;

    public Shader OutlineVariant => _outlineVariant;

    public PinMaterialId MaterialId => _materialId;
    public Material Material => _material;

    public Shader OutlineShader => _outlineShader;

    public float Alpha => _alpha;

    public PinColor TryGetOverridedColor(PinColorID color)
    {
        if (_pinColors == null || _pinColors.Count == 0)
        {
            return null;
        }

        return _pinColors.Find(a => a.ColorId == color);
    }
}

public class PinMaterialPropertyBlockList
{
    private readonly List<PinMaterialPropertyBlock> _pinColorBlocks;
    private readonly PinMaterial _pinMaterialData;
    private readonly Shader _shader;
    private int period;

    public PinMaterialPropertyBlockList(PinMaterialId pinMaterialId, PinMaterial pinMaterialData)
    {
        PinMaterialId = pinMaterialId;
        _pinMaterialData = pinMaterialData;
        Material = new Material(pinMaterialData.Material);
        _shader = GameConfig.RemoteConfig.outlineVariant && _pinMaterialData.OutlineVariant != null
            ? _pinMaterialData.OutlineVariant
            : _pinMaterialData.Material.shader;
        _pinColorBlocks = new List<PinMaterialPropertyBlock>();
        Material.shader = _shader;
    }

    public PinMaterialId PinMaterialId { get; }

    public Material Material { get; private set; }

    public Material GetMaterial()
    {
        if (period >= GameConfig.RemoteConfig.batchingMPBPeriod)
        {
            period = 0;
            Material = new Material(Material);
        }

        period++;
        return Material;
    }

    private PinMaterialPropertyBlock FindOrCreateColorPropertyBlockId(PinColorID colorId)
    {
        var propertyBlockData = _pinColorBlocks.Find(a => a.ColorId == colorId);

        if (propertyBlockData == null)
        {
            propertyBlockData = new PinMaterialPropertyBlock(colorId, _pinMaterialData, this);
            _pinColorBlocks.Add(propertyBlockData);
        }

        return propertyBlockData;
    }

    public PinMaterialPropertyBlock GetProperyBlock(PinColorID colorId)
    {
        var pinMaterialBlocInfo = FindOrCreateColorPropertyBlockId(colorId);
        return pinMaterialBlocInfo;
    }
}

public class PinMaterialPropertyBlock
{
    private PinColor _pinColor;

    private PinMaterial _pinMaterial;
    private int period;

    public PinMaterialPropertyBlock(PinColorID colorId, PinMaterial pinMaterial,
        PinMaterialPropertyBlockList pinMaterialPropertyBlockList)
    {
        ColorId = colorId;
        var pinColor = ColorHolder.Instance.GetColor(pinMaterial, ColorId);
        Color = pinColor.PrimaryColor;
        _pinMaterial = pinMaterial;
        _pinColor = pinColor;
        GetMaterial = new Material(pinMaterial.Material);

        GetMaterial.SetColor("_Color", pinColor.PrimaryColor);
        GetMaterial.SetColor("_OutlineColor", pinColor.OutlineColor);
        GetMaterial.SetColor("_Color01", pinColor.SecondaryColor);
        GetMaterial.SetColor("_SelfShadowColor", pinColor.SelfShadowColor);
        GetMaterial.SetColor("_FalloffColor", pinColor.FalloffColor);

        GetMaterial.SetFloat("_OutlineThickness", 0.27f);
        PinMaterialPropertyBlockList = pinMaterialPropertyBlockList;
    }

    public PinColorID ColorId { get; }

    public Color Color { get; }

    public Material GetMaterial { get; }

    public PinMaterialPropertyBlockList PinMaterialPropertyBlockList { get; }
}

public enum PinColorID
{
    color1 = 0,
    color2 = 1,
    color3 = 2,
    color4 = 3,
    color5 = 4,
    color6 = 5,
    color7 = 6,
    color8 = 7
}

public enum PinMaterialId
{
    defaultMat = 0,
    glass = 1,
    wood = 2,
    candy = 3,
    pixel = 4,
    cheetah = 5,
    snake = 6,
    zebra = 7
}
