using UnityEngine;

namespace Unpuzzle
{
    [CreateAssetMenu(fileName = "HoleScaleConfig", menuName = "HookConfig/New HoleScaleConfig")]
    public class HoleScaleConfig : ScriptableObject
    {
        [SerializeField] [Range(0.5f, 2f)] private float _xz = 1f;

        public void SetScale(Transform transform)
        {
            transform.localScale = new Vector3(_xz, 1f, _xz);
        }
    }
}
