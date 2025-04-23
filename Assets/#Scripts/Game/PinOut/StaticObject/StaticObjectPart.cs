using UnityEngine;

namespace Unpuzzle
{
    public class StaticObjectPart : MonoBehaviour, ICollisionElement
    {
        [SerializeField] private StaticObjectTouchFeedback _touchFeedback;

        public void OnTriggerEnter(Collider other)
        {
            var hookPart = other.GetComponent<HookPart>();

            if (hookPart == null)
            {
                return;
            }

            if (hookPart.Controller.Removed)
            {
                return;
            }

            hookPart.Controller.OnTouchStaticObject(this);
            _touchFeedback?.PlayFeedBack();
        }

        public void StartFire()
        {
        }

        public void TouchFeedack()
        {
        }
    }
}
