using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unpuzzle
{
    public class ActivatorParticels : MonoBehaviour
    {
        [SerializeField] Transform startPoint;
        [SerializeField] List<Transform> endPoints;

        [SerializeField] GameObject particlePrefab;
        [SerializeField] GameObject BurstParticelsPrefab;

        [SerializeField] float particleSpeed = 50;

        void Start()
        {
            print("Start");
            StartCoroutine(launcher());
        }

        void Update()
        {
        }

        private void LaunchParticles()
        {
            for (var i = 0; i < endPoints.Count; i++)
            {
                var particle = Instantiate(particlePrefab, startPoint, false);
                particle.name = "ActivationParticle" + i;

                var distance = Vector3.Magnitude(startPoint.position - endPoints[i].position);

                var ptB = startPoint.position + Vector3.ProjectOnPlane(endPoints[i].position, Vector3.up);
                var ptC = endPoints[i].position + Vector3.ProjectOnPlane(endPoints[i].position, Vector3.up) * 0.5f;

                StartCoroutine(fly(particle, i, Time.time, distance, ptB, ptC));
                print(particle.name);
            }
        }

        private IEnumerator fly(GameObject p, int i, float t, float d, Vector3 b, Vector3 c)
        {
            yield return new WaitForEndOfFrame();
            var delta = (Time.time - t) * particleSpeed / d;

            if (delta <= 1)
            {
                p.transform.position = Mathf.Pow(1 - delta, 3) * startPoint.position +
                                       3 * Mathf.Pow(1 - delta, 2) * delta * b +
                                       3 * (1 - delta) * Mathf.Pow(delta, 2) * c +
                                       Mathf.Pow(delta, 3) * endPoints[i].position;

                StartCoroutine(fly(p, i, t, d, b, c));
            }
            else
            {
                Instantiate(BurstParticelsPrefab, endPoints[i], false);
            }
        }

        private IEnumerator launcher()
        {
            yield return new WaitForSeconds(2);
            print("go");
            if (endPoints.Count != 0)
            {
                LaunchParticles();
            }

            StartCoroutine(launcher());
        }
    }
}
