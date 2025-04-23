using UnityEngine;

namespace Unpuzzle
{
    public class MeshSizeCalculator : MonoBehaviour
    {
        [ContextMenu("Print Mesh Size")]
        void PrintMeshSize()
        {
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                var mesh = meshFilter.mesh;
                mesh.RecalculateBounds();
                var size = mesh.bounds.size;
                Debug.Log($"Mesh Size - Width: {size.x}, Height: {size.y}, Length: {size.z}");
            }
            else
            {
                Debug.Log("MeshFilter component is not attached to the object.");
            }
        }
    }
}
