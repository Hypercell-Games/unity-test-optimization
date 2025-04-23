using UnityEngine;

namespace Unpuzzle
{
    public class EyeRotator : MonoBehaviour
    {
        [Range(0, 0.94f)] [SerializeField] private float rayLength = 0.94f;

        public float RayLenght
        {
            set => rayLength = value;
            get => rayLength;
        }

        void Update()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            var newRot = Quaternion.LookRotation(ray.origin +
                                                 ray.direction *
                                                 (Vector3.Magnitude(Camera.main.transform.position -
                                                                    transform.position) * rayLength) -
                                                 transform.position);


            transform.rotation = newRot;
        }
    }
}
