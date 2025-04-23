using UnityEngine;

namespace Unpuzzle
{
    public class Scaler : MonoBehaviour
    {
        [SerializeField] private HoleScaleConfig _holeScaleConfig;

        private void Start()
        {
            UpdateScale();
        }

#if UNITY_EDITOR
        private void Update()
        {
            UpdateScale();
        }
#endif

        private void UpdateScale()
        {
            _holeScaleConfig.SetScale(transform);
        }
    }
}
