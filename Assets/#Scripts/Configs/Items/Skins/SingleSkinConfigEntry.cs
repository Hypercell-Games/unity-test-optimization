using UnityEngine;

public abstract class SingleSkinConfigEntry : BaseSkinConfigEntry
{
    [Header("Tile Color")] [SerializeField]
    protected ETileColor _tileColor;

    [Header("Effects")] [SerializeField] private Color _effectsColor = Color.white;

    [Space(10)] [SerializeField] private TrailRenderer _trailEffect;

    [SerializeField] private ParticleSystem _dieEffect;
    [SerializeField] private ParticleSystem _pathEffect;

    [Space(10)] [Tooltip("Force path effect ignoring remote config")] [SerializeField]
    private bool _forcePathEffect;

    public Color EffectsColor => _effectsColor;
    public TrailRenderer TrailEffect => _trailEffect;
    public ParticleSystem DieEffect => _dieEffect;
    public ParticleSystem PathEffect => _pathEffect;
    public bool ForcePathEffect => _forcePathEffect;

    public ETileColor TileColor { get => _tileColor; set => _tileColor = value; }

    private Color GetTileColor()
    {
        return TileColors.GetColor(TileColor);
    }

    public abstract TileSkinType GetSkinType();

    public override SingleSkinConfigEntry ProduceSingleSkinConfig(int level, int stage, ETileColor color)
    {
        return this;
    }
}
