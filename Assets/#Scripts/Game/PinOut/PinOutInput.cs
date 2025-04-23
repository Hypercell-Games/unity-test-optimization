using System.Linq;
using UnityEngine;

namespace Unpuzzle
{
    public class PinOutInput : MonoBehaviour
    {
        [SerializeField] private MobileInputController _mobileInputController;
        private Camera _camera;

        private HookHandle _hookHandle;
        private bool _inputBlocked;

        private HookController _onlyInteractivePin;
        private PinBolt _pinBolt;

        public static PinOutInput Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _camera = CameraManager.Instance.GetCameraItem(ECameraType.GAME).Camera;
        }

        private void OnEnable()
        {
            _mobileInputController.onPointerClick += Click;
            _mobileInputController.onPointerDown += OnPress;
            _mobileInputController.onPointerUp += OnUp;
            _mobileInputController.onClickInterrupt += OnInteruptClick;
        }

        private void OnDisable()
        {
            _mobileInputController.onPointerClick -= Click;
            _mobileInputController.onPointerDown -= OnPress;
            _mobileInputController.onPointerUp -= OnUp;
            _mobileInputController.onClickInterrupt -= OnInteruptClick;
        }

        public void SetTutorialPin(HookController hook)
        {
            _onlyInteractivePin = hook;
        }

        public void SetInputBlocked(bool blocked)
        {
            _inputBlocked = blocked;
        }

        public void ClearTutorialPin()
        {
            _onlyInteractivePin = null;
        }

        private void OnPress(Vector2 pos)
        {
            if (_inputBlocked)
            {
                return;
            }

            var level = LevelsController.Instance.CurrentGameController;

            if (level == null || level.GetCurrentStage() == null || level.StartAnimationShowing ||
                level.GetCurrentStage().Level.IsControllBlocked)
            {
                return;
            }

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var hits = Physics.RaycastAll(ray);
            HookHandle pinHandle = null;
            PinBolt pinBolt = null;

            if (TryGetHandle(hits, out pinHandle, out pinBolt))
            {
                if (pinBolt != null)
                {
                    _pinBolt = pinBolt;
                    _pinBolt.Press();
                    return;
                }

                if (pinHandle.HookController == null || pinHandle.HookController.IsGhost ||
                    pinHandle.HookController.Removed)
                {
                    return;
                }

                _hookHandle = pinHandle;
                _hookHandle.Press();
                return;
            }

            hits = Physics.SphereCastAll(ray, GameConfig.RemoteConfig.secondStepHookCastRadius);

            if (TryGetHandle(hits, out pinHandle, out pinBolt))
            {
                if (pinBolt != null)
                {
                    _pinBolt = pinBolt;
                    _pinBolt.Press();
                    return;
                }

                if (pinHandle.HookController == null || pinHandle.HookController.IsGhost ||
                    pinHandle.HookController.Removed)
                {
                    return;
                }

                _hookHandle = pinHandle;
                _hookHandle.Press();
            }
        }

        public bool TryGetHandle(RaycastHit[] hits, out HookHandle handle, out PinBolt pinBolt)
        {
            handle = null;
            pinBolt = null;
            if (hits == null || hits.Length == 0)
            {
                return false;
            }

            hits = hits.OrderBy(a => a.distance).ToArray();

            for (var i = 0; i < hits.Length; i++)
            {
                var collider = hits[i].collider;
                if (collider == null)
                {
                    continue;
                }

                var pinBoltFinded = collider.GetComponent<PinBolt>();

                if (pinBoltFinded != null)
                {
                    pinBolt = pinBoltFinded;
                    return true;
                }

                var pinHandle = collider.GetComponent<HookHandle>();

                if (_onlyInteractivePin != null)
                {
                    if (pinHandle == null || _onlyInteractivePin != pinHandle.HookController)
                    {
                        continue;
                    }
                }
                else
                {
                    if (pinHandle == null)
                    {
                        break;
                    }

                    if (pinHandle.HookController == null)
                    {
                        break;
                    }

                    if (pinHandle.HookController.IsGhost)
                    {
                        continue;
                    }
                }

                handle = pinHandle;
                return true;
            }

            return false;
        }

        private void Click(Vector2 pos)
        {
            if (_hookHandle != null)
            {
                _hookHandle.Release();
                _hookHandle.TryToRemoveHoook();
                _hookHandle = null;
                return;
            }

            if (_pinBolt != null)
            {
                _pinBolt.Up();
                _pinBolt.BoltDestroy();
            }
        }

        private void OnInteruptClick()
        {
            OnUp(Vector2.zero);
        }

        private void OnUp(Vector2 pos)
        {
            if (_hookHandle != null)
            {
                _hookHandle.Release();
            }

            if (_pinBolt != null)
            {
                _pinBolt.Up();
            }

            _pinBolt = null;
            _hookHandle = null;
        }
    }
}
