using UnityEngine;

[CreateAssetMenu(fileName = "3DSkin", menuName = MenuPath, order = MenuOrder)]
public class ModelSkinConfigEntry : SingleSkinConfigEntry
{
    private const string MenuPath = "Configs/3DSkin";
    private const int MenuOrder = int.MinValue + 110;

    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material[] _materials;

    public Mesh Mesh => _mesh;
    public Material[] Materials => _materials;

    public override TileSkinType GetSkinType()
    {
        return TileSkinType.Model;
    }
}
