using System;
using UnityEngine;

public class GameBoardInteraction : MonoSingleton<GameBoardInteraction>
{
    [SerializeField] private MobileInputController _mobileInputController;

    private CameraManager _cameraManager;
    private Vector2 _pointerDownPosition;

    private void Start()
    {
        _cameraManager = CameraManager.Instance;
    }

    private void OnEnable()
    {
        _mobileInputController.onPointerDown += OnPointerDown;
        _mobileInputController.onPointerUp += OnPointerUp;
        _mobileInputController.onPointerClick += OnPointerClick;
        _mobileInputController.onPointerDrag += OnPointerDrag;
    }

    private void OnDisable()
    {
        _mobileInputController.onPointerDown -= OnPointerDown;
        _mobileInputController.onPointerUp -= OnPointerUp;
        _mobileInputController.onPointerClick -= OnPointerClick;
        _mobileInputController.onPointerDrag -= OnPointerDrag;
    }

    public event Action<Vector2> onPointerClick;
    public event Action<Vector2> onPointerUp;
    public event Action<Vector2> onPointerDown;
    public event Action<Vector2> onPointerDrag;

    private void OnPointerDown(Vector2 position)
    {
        var worldInputPosition = ConvertScreenToWorldPosition(position);

        _pointerDownPosition = worldInputPosition;

        onPointerDown?.Invoke(worldInputPosition);
    }

    private void OnPointerUp(Vector2 position)
    {
        var worldInputPosition = ConvertScreenToWorldPosition(position);


        onPointerClick?.Invoke(worldInputPosition);

        onPointerUp?.Invoke(worldInputPosition);
    }

    private void OnPointerDrag(Vector2 position, Vector2 delta)
    {
        var worldInputPosition = ConvertScreenToWorldPosition(position);
        onPointerDrag?.Invoke(worldInputPosition);
    }

    private Vector2 ConvertScreenToWorldPosition(Vector2 screenPosition)
    {
        var worldInputPosition = _cameraManager.GetCameraItem(ECameraType.UI).Camera.ScreenToWorldPoint(screenPosition);
        worldInputPosition.z = 0f;

        return worldInputPosition;
    }

    private void OnPointerClick(Vector2 position)
    {
    }
}
