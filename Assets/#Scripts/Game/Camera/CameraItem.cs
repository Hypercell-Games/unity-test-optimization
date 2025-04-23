using UnityEngine;

public class CameraItem : MonoBehaviour, ICameraItem
{
    private const float _fovMax = 30f;
    [SerializeField] private ECameraType _cameraType = ECameraType.NONE;
    [SerializeField] private Camera _camera;
    private float _cameraHeightMax;

    private Vector2 _clampPos;
    private float _scaleMax;
    private float _zoom = 1f;

    private void Awake()
    {
        var plane = new Plane(Vector3.forward, Vector3.zero);
        var ray = _camera.ViewportPointToRay(Vector2.one);
        if (plane.Raycast(ray, out var enter))
        {
            _clampPos = ray.GetPoint(enter);
        }

        _cameraHeightMax = Mathf.Tan(_fovMax * 0.5f * Mathf.Deg2Rad) * 2f;
    }

    public ECameraType CameraType => _cameraType;
    public Camera Camera => _camera;

    public void SetMaxZoom(float value)
    {
        _scaleMax = value;
    }

    public void Scale(float scaleStep)
    {
        if (scaleStep == 1f || float.IsNaN(scaleStep))
        {
            return;
        }

        scaleStep = Mathf.Max(0.01f, scaleStep);
        _zoom = Mathf.Clamp(_zoom / scaleStep, 1f, _scaleMax);

        var N1 = _cameraHeightMax / _zoom;
        var fov1 = Mathf.Atan(N1 * 0.5f) * Mathf.Rad2Deg * 2f;

        _camera.fieldOfView = fov1;
        CheckBorders();
    }

    public void Scale(float scale, Vector2 centerPointViewport)
    {
        var plane = new Plane(Vector3.forward, Vector3.zero);
        var ray0 = _camera.ViewportPointToRay(centerPointViewport);
        var wp0 = Vector3.zero;
        if (plane.Raycast(ray0, out var enter0))
        {
            wp0 = ray0.GetPoint(enter0);
        }

        Scale(scale);
        var ray1 = _camera.ViewportPointToRay(centerPointViewport);
        var wp1 = Vector3.zero;
        if (plane.Raycast(ray1, out var enter1))
        {
            wp1 = ray1.GetPoint(enter1);
        }

        var offset = wp0 - wp1;
        offset.z = 0f;
        transform.position += offset;
        CheckBorders();
    }

    public void Move(Vector2 move)
    {
        move = move / _zoom * _cameraHeightMax * 28f;
        transform.position += (Vector3)move;
        CheckBorders();
    }

    private void CheckBorders()
    {
        var plane = new Plane(Vector3.forward, Vector3.zero);
        var ray = _camera.ViewportPointToRay(Vector2.one);
        if (plane.Raycast(ray, out var enter0))
        {
            var pos = (Vector2)ray.GetPoint(enter0);
            var dPos = pos - _clampPos;
            dPos.x = Mathf.Max(0f, dPos.x);
            dPos.y = Mathf.Max(0f, dPos.y);
            transform.position -= (Vector3)dPos;
        }

        ray = _camera.ViewportPointToRay(Vector2.zero);
        if (plane.Raycast(ray, out var enter1))
        {
            var pos = (Vector2)ray.GetPoint(enter1);
            var dPos = -pos - _clampPos;
            dPos.x = Mathf.Max(0f, dPos.x);
            dPos.y = Mathf.Max(0f, dPos.y);
            transform.position += (Vector3)dPos;
        }
    }
}
