using UnityEngine;

namespace Unpuzzle
{
    [ExecuteInEditMode]
    public class IceBlockEditor : MonoBehaviour
    {
        private HookController _hookController;
        private bool _iceBlockEnabled;

        private void Start()
        {
            if (Application.isPlaying)
            {
                return;
            }

            _hookController = GetComponent<HookController>();

            IceBlockUpdate();
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (_hookController == null)
            {
                return;
            }

            if (_iceBlockEnabled != _hookController.IsFrozen)
            {
                IceBlockUpdate();
            }
        }

        private void IceBlockUpdate()
        {
            _iceBlockEnabled = _hookController.IsFrozen;

            if (_hookController.IceBlock == null)
            {
                return;
            }

            _hookController.IceBlock.gameObject.SetActive(_iceBlockEnabled);
        }
    }
}
