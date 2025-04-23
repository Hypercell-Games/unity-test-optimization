using UnityEngine;

public class BlockTrail : MonoBehaviour
{
    private const float _baseWidth = 1f;
    [SerializeField] private TrailRenderer _trailRenderer;
    [SerializeField] private ParticleSystemRenderer _particleSystem;

    public void Init(float scale, BlockMaterials blockMaterials)
    {
        SetScale(scale);
        var color = blockMaterials.Material.GetColor("_Diffuse");
        _trailRenderer.startColor = new Color(color.r, color.g, color.b, _trailRenderer.startColor.a);
        _trailRenderer.endColor = new Color(color.r, color.g, color.b, _trailRenderer.endColor.a);
        _particleSystem.material = blockMaterials.Material;
    }

    public void SetScale(float scale)
    {
        _trailRenderer.startWidth = _baseWidth * scale;
        _trailRenderer.endWidth = _baseWidth * scale;
    }
}
