using UnityEngine;

public interface ICameraItem
{
    Camera Camera { get; }
    ECameraType CameraType { get; }

    void Scale(float scale);
    void Scale(float scale, Vector2 centerPointViewport);
    void Move(Vector2 move);
    void SetMaxZoom(float value);
}
