using UnityEngine;

namespace Unpuzzle
{
    public class HookMoveOutRaycaster : MonoBehaviour
    {
        [SerializeField] private Transform _raycastPointer;
        [SerializeField] private Transform _rightBound;
        [SerializeField] private Transform _upBound;

        void OnDrawGizmosSelected()
        {
            if (_raycastPointer == null)
            {
                return;
            }

            if (_rightBound == null)
            {
                return;
            }

            if (_upBound == null)
            {
                return;
            }

            var matrix = Matrix4x4.TRS(_raycastPointer.position, _raycastPointer.rotation, Vector3.one);

            var halfWidth = _raycastPointer.InverseTransformPoint(_rightBound.position).x;
            var halfHeight = _raycastPointer.InverseTransformPoint(_upBound.position).y;


            Gizmos.color = Color.yellow;
            Gizmos.matrix = matrix;
            Gizmos.DrawWireCube(Vector3.zero + Vector3.forward * halfWidth * 2f,
                new Vector3(halfWidth * 2f, halfHeight * 2, halfWidth * 4f));
        }

        public bool CanMoveOut(HookController exceptHook, out Vector3 touchPos)
        {
            var halfWidth = _raycastPointer.InverseTransformPoint(_rightBound.position).x;
            var halfHeight = _raycastPointer.InverseTransformPoint(_upBound.position).y;
            var hits = Physics.BoxCastAll(_raycastPointer.position, new Vector3(halfWidth, halfHeight),
                _raycastPointer.forward, _raycastPointer.rotation);
            touchPos = Vector3.zero;

            if (hits.Length == 0)
            {
                return true;
            }


            foreach (var hit in hits)
            {
                var pinPart = hit.collider.GetComponent<HookPart>();

                if (pinPart == null)
                {
                    var staticObject = hit.collider.GetComponent<ICollisionElement>();
                    if (staticObject != null)
                    {
                        touchPos = hit.point;
                        return false;
                    }

                    continue;
                }

                if (pinPart.Controller.IsGhost || pinPart.Controller == exceptHook)
                {
                    continue;
                }

                if (pinPart.Controller.Removed)
                {
                    continue;
                }

                touchPos = hit.point;
                return false;
            }

            return true;
        }
    }
}
