using System.Collections.Generic;
using UnityEngine;

namespace Unpuzzle
{
    public class PinFactory : MonoBehaviour
    {
        [SerializeField] private List<HookController> _pins;
        [SerializeField] private List<IceBlockController> _iceBLockController;
        [SerializeField] private PinBoltPlank _pinBoltPlank;

        public HookController GetPin(PinType pinType)
        {
            var pin = _pins.Find(a => a.pinType == pinType);

            if (pin == null)
            {
                return null;
            }

            pin = Instantiate(pin);

            pin.CacheRefs();

            return pin;
        }

        public PinBoltPlank GetPinBolt()
        {
            var bolt = Instantiate(_pinBoltPlank);
            return bolt;
        }

        public void ReturnPin(HookController pin)
        {
            Destroy(pin.gameObject);
        }

        public IceBlockController GetIceBlockController(IceBlockType type)
        {
            var iceBLock = _iceBLockController.Find(a => a.IceBlockType == type);
            if (iceBLock == null)
            {
                return null;
            }

            iceBLock = Instantiate(iceBLock);

            return iceBLock;
        }
    }
}
