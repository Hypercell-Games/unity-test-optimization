using UnityEngine;

namespace Unpuzzle
{
    public class MinMaxRange : PropertyAttribute
    {
        public float max;
        public float min;

        public MinMaxRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
