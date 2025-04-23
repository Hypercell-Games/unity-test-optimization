using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoSingleton<CameraManager>
{
    [SerializeField] private List<CameraItem> _cameraItems = new();

    public List<CameraItem> CameraItems => _cameraItems;

    public ICameraItem GetCameraItem(ECameraType cameraType)
    {
        return CameraItems.Find(item => item.CameraType == cameraType);
    }
}
