using UnityEngine;

namespace Unpuzzle
{
    public abstract class AbstrackIceBlockScale : MonoBehaviour
    {
        public abstract void SetSize(Vector3 size);

        public abstract Vector3 GetSize();
    }
}
