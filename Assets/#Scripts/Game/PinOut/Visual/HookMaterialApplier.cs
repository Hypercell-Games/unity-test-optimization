using System.Collections.Generic;
using UnityEngine;

namespace Unpuzzle
{
    public class HookMaterialApplier : MonoBehaviour
    {
        [SerializeField] private List<MeshRenderer> _mesh;
        [SerializeField] private Transform _scaledMesh;
        [SerializeField] private ScaleSourceAxis _scaleSourceAxis = ScaleSourceAxis.y;

        public void ApplyMaterial(Material material)
        {
            return;
            var tilledMaterial = new Material(material);
            var tile = material.GetTextureScale("_MainTex");
            tile.y *= (_scaleSourceAxis == ScaleSourceAxis.y ? _scaledMesh.localScale.y : _scaledMesh.localScale.x) *
                      0.5f;
            tilledMaterial.SetTextureScale("_MainTex", tile);
            _mesh.ForEach(a => a.material = tilledMaterial);
        }

        private enum ScaleSourceAxis
        {
            y = 0,
            x = 1
        }
    }
}
