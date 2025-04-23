using UnityEngine;

public class HookPart : MonoBehaviour
{
    [SerializeField] private HookController _hookController;
    [SerializeField] private bool _isIce;

    public bool IsIce => _isIce;

    public HookController Controller
    {
        get => _hookController;
        set => _hookController = value;
    }

    public void OnTriggerEnter(Collider other)
    {
        var hookPart = other.GetComponent<HookPart>();

        if (hookPart == null ||
            hookPart.Controller == _hookController)
        {
            return;
        }

        if (hookPart.Controller._pinOutLevel != Controller._pinOutLevel)
        {
            return;
        }

        if (hookPart.Controller.Removed)
        {
            return;
        }

        var collisionPoint = other.ClosestPoint(transform.position);
        _hookController.OnPartCollision(hookPart.Controller, collisionPoint);
    }
}
