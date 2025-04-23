using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MobileInputController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private Camera _camera;

    private float _dpiMultiplier;

    private bool _dragOffsetBlocked;
    private bool _dragRotationBlocked;

    private Vector2 _inputVector;

    private bool _isClick;
    private float _lastSize;
    private bool _moveBtnPressed;
    private Vector3 _prevMoveMousePos;

    private Touch[] _prevTouches = new Touch[0];
    private bool _zoomBlocked;
    private bool _zoomOnlyZoomIn;

    private void Awake()
    {
        _dpiMultiplier = Screen.dpi / GameConfig.RemoteConfig.defaultDpi;
        _camera = Camera.main;
    }

    private void Update()
    {
        var zoom = Input.GetAxis("Mouse ScrollWheel");

        if (!GameConfig.RemoteConfig.zoomNewEnabled)
        {
            onZoom(zoom * 100000f);
        }
        else
        {
            var mousePosition = Input.mousePosition;
            var mouseViewportPositon = _camera.ScreenToViewportPoint(mousePosition);

            OnZoomInvoke(zoom, mouseViewportPositon);
            MultitouchUpdate();
            MoveByMouseUpdate();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!GameConfig.RemoteConfig.zoomNewEnabled)
        {
            if (Input.touchCount == 2)
            {
                var size = Vector2.Distance(Input.touches[0].position, Input.touches[1].position) * _dpiMultiplier *
                           GameConfig.RemoteConfig.zoomSensivity;
                onZoom(size - _lastSize);
                return;
            }
        }
        else
        {
            if (Input.touchCount > 1)
            {
                return;
            }

            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
        }

        if (_isClick && Vector3.Distance(_inputVector, eventData.position) >
            GameConfig.RemoteConfig.tapMaxZone * _dpiMultiplier)
        {
            _isClick = false;
            onClickInterrupt.Invoke();
        }

        OnRotateInvoke(eventData.position, eventData.delta * _dpiMultiplier * GameConfig.RemoteConfig.swipeSensivity);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!GameConfig.RemoteConfig.zoomNewEnabled)
        {
            if (Input.touchCount == 2)
            {
                onClickInterrupt.Invoke();
                _isClick = false;
                _lastSize = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                return;
            }
        }
        else
        {
            if (Input.touchCount > 1)
            {
                onClickInterrupt.Invoke();
                _isClick = false;
                return;
            }
        }

        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        _inputVector = eventData.position;
        _isClick = true;

        onPointerDown?.Invoke(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Input.touchCount > 1)
        {
            return;
        }

        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (_isClick)
        {
            onPointerClick?.Invoke(_inputVector);
        }

        onPointerUp?.Invoke(eventData.position);
    }

    public event Action<Vector2> onPointerDown;
    public event Action<Vector2> onPointerUp;
    public event Action<Vector2> onPointerClick;
    public event Action onClickInterrupt = () => { };
    public event Action<Vector2, Vector2> onPointerDrag;
    public event Action<float> onZoom;
    public event Action<float, Vector2> onZoomNew;
    public event Action<Vector3, Vector3> onMove;

    public void SetBlockInput(bool rotationBlocked, bool dragBlocked, bool zoomBlocked, bool onlyZoomIn)
    {
        _dragRotationBlocked = rotationBlocked;
        _zoomBlocked = zoomBlocked;
        _dragOffsetBlocked = dragBlocked;
        _zoomOnlyZoomIn = onlyZoomIn;
    }

    private void MoveByMouseUpdate()
    {
        var mousePos = Input.mousePosition;
        if (!_moveBtnPressed)
        {
            _prevMoveMousePos = mousePos;
        }

        _moveBtnPressed = Input.GetMouseButton(1) && !Input.GetMouseButton(0) && Input.touchCount < 2;
        if (_moveBtnPressed)
        {
            OnMoveInvoke(_prevMoveMousePos, mousePos);
        }

        _prevMoveMousePos = mousePos;
    }

    private Vector2 GetPointerViewPos(Vector3 pointerScreenPos)
    {
        var pos = _camera.ScreenToViewportPoint(pointerScreenPos);
        pos.x *= _camera.aspect;
        return pos;
    }

    private void OnZoomInvoke(float zoom, Vector2 position)
    {
        if (_zoomBlocked)
        {
            return;
        }

        if (_zoomOnlyZoomIn && zoom <= 0)
        {
            return;
        }

        onZoomNew(zoom, position);
    }

    private void OnMoveInvoke(Vector3 prePos, Vector3 currentPos)
    {
        if (_dragOffsetBlocked)
        {
            return;
        }

        onMove(prePos, currentPos);
    }

    private void OnRotateInvoke(Vector2 position, Vector2 input)
    {
        if (_dragRotationBlocked)
        {
            return;
        }

        onPointerDrag?.Invoke(position, input);
    }

    private void MultitouchUpdate()
    {
        var touchesCount = Input.touchCount;
        var touches = Input.touches;
        if (touchesCount > 1)
        {
            var centerPoint = Vector2.zero;
            var centerPointViewport = Vector3.zero;
            var draggedTouchesCount = 0;
            for (var i = 0; i < touchesCount; i++)
            {
                var touch = touches[i];
                for (var j = 0; j < _prevTouches.Length; j++)
                {
                    var prevTouch = _prevTouches[j];
                    if (touch.fingerId == prevTouch.fingerId)
                    {
                        centerPointViewport += _camera.ScreenToViewportPoint(touch.position);
                        var touchPos = GetPointerViewPos(touch.position);
                        var touchPrevPos = GetPointerViewPos(prevTouch.position);

                        centerPoint += touchPos;
                        draggedTouchesCount++;
                        break;
                    }
                }
            }

            if (draggedTouchesCount > 1)
            {
                centerPoint /= draggedTouchesCount;
                centerPointViewport /= draggedTouchesCount;
                var touchScreenPrevPos = Vector2.zero;
                var touchScreenPos = Vector2.zero;
                var scale = 0f;
                for (var i = 0; i < touchesCount; i++)
                {
                    var touch = touches[i];
                    for (var j = 0; j < _prevTouches.Length; j++)
                    {
                        var prevTouch = _prevTouches[j];
                        if (touch.fingerId == prevTouch.fingerId)
                        {
                            touchScreenPrevPos += prevTouch.position;
                            touchScreenPos += touch.position;
                            var touchPos = GetPointerViewPos(touch.position);
                            var touchPrevPos = GetPointerViewPos(prevTouch.position);
                            var distNow = Vector2.Distance(touchPos, centerPoint);
                            var distPrev = Vector2.Distance(touchPrevPos, centerPoint);
                            scale += distPrev / distNow;
                            break;
                        }
                    }
                }

                scale /= draggedTouchesCount;
                touchScreenPrevPos /= draggedTouchesCount;
                touchScreenPos /= draggedTouchesCount;
                var zoom = 1f - scale;
                OnZoomInvoke(zoom, centerPointViewport);
                OnMoveInvoke(touchScreenPrevPos, touchScreenPos);
            }
        }

        _prevTouches = touches;
    }
}
