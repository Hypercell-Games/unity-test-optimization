using UnityEngine;

namespace Unpuzzle
{
    public class ScaleIceBlockScale : AbstrackIceBlockScale
    {
        public override Vector3 GetSize()
        {
            return transform.localScale;
        }

        public override void SetSize(Vector3 size)
        {
            transform.localScale = size;
        }
    }
}
