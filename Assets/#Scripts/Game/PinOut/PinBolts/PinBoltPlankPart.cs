using UnityEngine;

namespace Unpuzzle
{
    public class PinBoltPlankPart : MonoBehaviour, ICollisionElement
    {
        [SerializeField] private PinBoltPlank pinBoltPlank;

        private void OnTriggerEnter(Collider other)
        {
            var hookPart = other.GetComponent<HookPart>();

            if (hookPart == null || hookPart.Controller == null)
            {
                return;
            }

            if (hookPart.Controller.IsReturning || hookPart.Controller.Removed)
            {
                return;
            }

            hookPart.Controller.OnTouchStaticObject(this);
        }

        public void StartFire()
        {
            pinBoltPlank.DestroyPlank(null);
        }

        public void TouchFeedack()
        {
            pinBoltPlank.PlankTouched();
        }
    }
}
