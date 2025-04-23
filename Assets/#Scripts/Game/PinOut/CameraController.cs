using DG.Tweening;
using UnityEngine;

namespace Unpuzzle
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float _minZoom;
        [SerializeField] private float _maxZoom;
        [SerializeField] private float _maxLevelSize;
        [SerializeField] private float _defaultAngeleY;
        [SerializeField] private float _defaultAngleX;
        [SerializeField] private LevelsController _levelController;
        [SerializeField] private MobileInputController _mobileInputController;
        [SerializeField] private Transform _cameraRoot;
        [SerializeField] private Transform _horizontalHandle;
        [SerializeField] private Transform _verticalHandle;
        [SerializeField] private Transform _camera;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Camera _tutorialCamera;

        private Vector2 _clampPos;

        public float MaxZoom => _maxZoom;

        public Camera MainCamera => _mainCamera;

        public Transform Handle => _horizontalHandle;

        private float MinNearClipPlane => GameConfig.RemoteConfig.CameraMinNearClipPlane;
        private float HalfFrustumDepth => GameConfig.RemoteConfig.CameraHalfFrustumDepth;

        private void Awake()
        {
        }

        private void OnEnable()
        {
            _mobileInputController.onPointerDrag += Drag;
            _mobileInputController.onZoom += OnZoom;
            _mobileInputController.onZoomNew += OnZoomNew;
            _mobileInputController.onMove += Move;
        }

        private void OnDisable()
        {
            _mobileInputController.onPointerDrag -= Drag;
            _mobileInputController.onZoom -= OnZoom;
            _mobileInputController.onZoomNew -= OnZoomNew;
            _mobileInputController.onMove -= Move;
        }

        private float GetDistance()
        {
            return Mathf.Abs(_camera.localPosition.z);
        }

        private void OffsetCameeraToVisualCenter(PinOutLevel level)
        {
            var offset = level.CenterPivot != null
                ? level.transform.InverseTransformPoint(level.CenterPivot.position)
                : level.VisualCenter;
            _cameraRoot.transform.position = offset;
            UpdateBorders();
        }

        public void CameraOffsetLerp(Vector3 offset, float time)
        {
            _cameraRoot.DOMove(offset, time).OnComplete(UpdateBorders).SetLink(_cameraRoot.gameObject);
        }

        public void SetCameraOffset(Vector3 offset)
        {
            _cameraRoot.position = offset;
            UpdateBorders();
        }

        public void ResetRotation()
        {
            var rotation = _horizontalHandle.localRotation.eulerAngles;
            rotation.y = 45f;
            rotation.x = 15f;
            rotation.z = 0f;

            _horizontalHandle.localRotation = Quaternion.Euler(rotation);
        }

        public void ResetRotationLerp(float time)
        {
            var rotation = _horizontalHandle.localRotation.eulerAngles;
            rotation.y = 45f;
            rotation.x = 15f;
            rotation.z = 0f;

            _horizontalHandle.DOLocalRotateQuaternion(Quaternion.Euler(rotation), time);
        }

        public void LerpZoom(float zoom, float time, Ease ease)
        {
            var currentZoom = zoom;

            _maxZoom = GameConfig.RemoteConfig.maxZoom;

            if (_maxZoom < currentZoom)
            {
                _maxZoom = currentZoom;
            }

            var pos = -_camera.localPosition.z;

            DOTween.To(x =>
                {
                    _camera.localPosition = Vector3.back * x;
                    _mainCamera.nearClipPlane = Mathf.Max(x - HalfFrustumDepth, MinNearClipPlane);
                    _mainCamera.farClipPlane = x + HalfFrustumDepth;

                    _tutorialCamera.nearClipPlane = _mainCamera.nearClipPlane;
                    _tutorialCamera.farClipPlane = _mainCamera.farClipPlane;

                    RenderSettings.fogStartDistance =
                        -_camera.localPosition.z + GameConfig.RemoteConfig.fogDistanceStart;
                    RenderSettings.fogEndDistance =
                        RenderSettings.fogStartDistance + GameConfig.RemoteConfig.fogDistanceEnd;
                }, pos, currentZoom, time)
                .SetLink(gameObject)
                .SetEase(ease);
        }

        public float GetZoom(Vector2 offset)
        {
            var widthOnScreen = offset.x;
            var haightScreen = offset.y;

            var screenResolution = _mainCamera.pixelWidth - _mainCamera.pixelWidth * 0.2f;
            var screenResoulutionHeight = _mainCamera.pixelHeight - _mainCamera.pixelHeight * 0.4f;

            var multiplier = Mathf.Max(widthOnScreen / screenResolution, haightScreen / screenResoulutionHeight);

            var currentZoom = Mathf.Abs(_camera.localPosition.z) * multiplier;

            currentZoom = Mathf.Max(currentZoom, _minZoom + 100f);

            return currentZoom;
        }

        private void UpdateBorders()
        {
            var cameraLocalPos = _camera.localPosition;
            _camera.localPosition = Vector3.back * _maxZoom;
            var plane = new Plane(-_camera.forward, _cameraRoot.position * 1f);
            var ray = _mainCamera.ViewportPointToRay(Vector2.one);
            if (plane.Raycast(ray, out var enter))
            {
                var pos1 = ray.GetPoint(enter);
                _clampPos = _camera.InverseTransformPoint(pos1);


                var ray0 = _mainCamera.ViewportPointToRay(Vector2.zero);
                if (plane.Raycast(ray0, out var enter0))
                {
                    var pos0 = ray0.GetPoint(enter0);
                }
            }

            _camera.localPosition = cameraLocalPos;
        }

        public void SetZoom(float zoom)
        {
            var currentZoom = zoom;

            _maxZoom = GameConfig.RemoteConfig.maxZoom;

            if (_maxZoom < currentZoom)
            {
                _maxZoom = currentZoom;
            }


            var pos = Mathf.Clamp(currentZoom, _minZoom + 100f, _maxZoom);
            _camera.localPosition = Vector3.back * pos;
            _mainCamera.nearClipPlane = Mathf.Max(pos - HalfFrustumDepth, MinNearClipPlane);
            _mainCamera.farClipPlane = pos + HalfFrustumDepth;

            _tutorialCamera.nearClipPlane = _mainCamera.nearClipPlane;
            _tutorialCamera.farClipPlane = _mainCamera.farClipPlane;

            RenderSettings.fog = false;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = -_camera.localPosition.z + GameConfig.RemoteConfig.fogDistanceStart;
            RenderSettings.fogEndDistance = RenderSettings.fogStartDistance + GameConfig.RemoteConfig.fogDistanceEnd;
        }

        private void Drag(Vector2 position, Vector2 input)
        {
            var level = LevelsController.Instance.CurrentGameController;

            if (level == null || level.StartAnimationShowing)
            {
                return;
            }

            if (level.GetCurrentStage() == null || level.GetCurrentStage().Level.IsControllBlocked)
            {
                return;
            }

            if (DebugPanel.controlLockX)
            {
                input.x = 0f;
            }

            if (DebugPanel.controlLockY)
            {
                input.y = 0f;
            }

            if (GameConfig.RemoteConfig.cameraRotationRalativeLevel && !DebugPanel.isAlternativeControl)
            {
                var rotation = _horizontalHandle.rotation.eulerAngles;
                var verticalRotation = rotation.x;
                if (verticalRotation > 180f)
                {
                    verticalRotation -= 360f;
                }

                rotation.x = Mathf.Clamp(verticalRotation - 0.1f * input.y, -75f, 75f);
                rotation.y += 0.1f * input.x;

                _horizontalHandle.rotation = Quaternion.Euler(rotation);
            }
            else
            {
                _horizontalHandle.Rotate(_camera.up, 0.1f * input.x, Space.World);
                _horizontalHandle.Rotate(_camera.right, -0.1f * input.y, Space.World);
            }
        }

        private void OnZoom(float delta)
        {
            var level = LevelsController.Instance.CurrentGameController;

            if (level == null || level.StartAnimationShowing)
            {
                return;
            }

            if (level.GetCurrentStage() == null || level.GetCurrentStage().Level.IsControllBlocked)
            {
                return;
            }

            var pos = _camera.localPosition.z + delta * 0.01f;
            pos = Mathf.Clamp(pos, -_maxZoom, -_minZoom);
            _mainCamera.nearClipPlane = Mathf.Max(-pos - HalfFrustumDepth, MinNearClipPlane);
            _mainCamera.farClipPlane = -pos + HalfFrustumDepth;
            _tutorialCamera.nearClipPlane = _mainCamera.nearClipPlane;
            _tutorialCamera.farClipPlane = _mainCamera.farClipPlane;
            _camera.localPosition = Vector3.forward * pos;
        }

        private void OnZoomNew(float scale, Vector2 centerPointViewport)
        {
            var level = LevelsController.Instance.CurrentGameController;

            if (level == null || level.StartAnimationShowing)
            {
                return;
            }

            if (level.GetCurrentStage() == null || level.GetCurrentStage().Level.IsControllBlocked)
            {
                return;
            }

            if (centerPointViewport.x < 0 || centerPointViewport.x > 1 ||
                centerPointViewport.y < 0 || centerPointViewport.y > 1)
            {
                return;
            }

            if (scale == 0f)
            {
                return;
            }

            var cameraRootPosition = _cameraRoot.position;
            _cameraRoot.position = Vector3.zero;
            var plane = new Plane(-_camera.forward, _cameraRoot.position);
            var ray0 = _mainCamera.ViewportPointToRay(centerPointViewport);
            var wp0 = Vector3.zero;
            if (plane.Raycast(ray0, out var enter0))
            {
                wp0 = ray0.GetPoint(enter0);
            }

            Scale(scale);
            var ray1 = _mainCamera.ViewportPointToRay(centerPointViewport);
            var wp1 = Vector3.zero;
            if (plane.Raycast(ray1, out var enter1))
            {
                wp1 = ray1.GetPoint(enter1);
            }

            var offset = wp0 - wp1;
            offset = _camera.InverseTransformVector(offset);
            offset.z = 0f;
            _camera.localPosition += offset;
            CheckBorders();
            _cameraRoot.position = cameraRootPosition;
        }

        public void Scale(float scaleStep)
        {
            scaleStep += 1f;
            if (scaleStep == 1f || float.IsNaN(scaleStep))
            {
                return;
            }

            scaleStep = Mathf.Max(0.01f, scaleStep);
            var dist = Mathf.Clamp(GetDistance() / scaleStep, _minZoom, _maxZoom);
            _mainCamera.nearClipPlane = Mathf.Max(dist - HalfFrustumDepth, MinNearClipPlane);
            _mainCamera.farClipPlane = dist + HalfFrustumDepth;

            _tutorialCamera.nearClipPlane = _mainCamera.nearClipPlane;
            _tutorialCamera.farClipPlane = _mainCamera.farClipPlane;

            _camera.localPosition = Vector3.back * dist;
        }

        private void CheckBorders()
        {
            var cameraRootPosition = _cameraRoot.position;
            _cameraRoot.position = Vector3.zero;
            var plane = new Plane(-_camera.forward, _cameraRoot.position);
            var ray = _mainCamera.ViewportPointToRay(Vector2.one);
            if (plane.Raycast(ray, out var enter0))
            {
                var pos = ray.GetPoint(enter0);
                pos = _camera.InverseTransformVector(pos);

                var dPos = (Vector2)pos - _clampPos;
                dPos.x = Mathf.Max(0f, dPos.x);
                dPos.y = Mathf.Max(0f, dPos.y);
                _camera.localPosition -= (Vector3)dPos;
            }

            ray = _mainCamera.ViewportPointToRay(Vector2.zero);
            if (plane.Raycast(ray, out var enter1))
            {
                var pos = ray.GetPoint(enter1);
                pos = _camera.InverseTransformVector(pos);

                var dPos = -(Vector2)pos - _clampPos;
                dPos.x = Mathf.Max(0f, dPos.x);
                dPos.y = Mathf.Max(0f, dPos.y);
                _camera.localPosition += (Vector3)dPos;
            }

            _cameraRoot.position = cameraRootPosition;
        }

        private void Move(Vector3 prevMoveMousePos, Vector3 mousePos)
        {
            var plane = new Plane(-_camera.forward, _cameraRoot.position);
            var ray0 = _mainCamera.ScreenPointToRay(prevMoveMousePos);
            if (plane.Raycast(ray0, out var enter0))
            {
                var pos0 = ray0.GetPoint(enter0);
                pos0 = _verticalHandle.InverseTransformPoint(pos0);
                var ray1 = _mainCamera.ScreenPointToRay(mousePos);
                if (plane.Raycast(ray1, out var enter1))
                {
                    var pos1 = ray1.GetPoint(enter1);
                    pos1 = _verticalHandle.InverseTransformPoint(pos1);
                    var delta = pos1 - pos0;
                    delta.z = 0f;
                    _camera.localPosition -= delta;
                    CheckBorders();
                }
            }
        }
    }
}
