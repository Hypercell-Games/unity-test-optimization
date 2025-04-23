using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Unpuzzle
{
    public class StaticObjectTouchFeedback : MonoBehaviour
    {
        [SerializeField] private List<MeshRenderer> _renderers;

        [SerializeField] private Shader _outlineShader;
        private Shader _defaultShader;

        private Tween _feedback;

        public void PlayFeedBack()
        {
            if (_renderers.Count == 0)
            {
                return;
            }

            if (_defaultShader == null)
            {
                _defaultShader = _renderers[0].sharedMaterial.shader;
            }

            _feedback?.Kill(true);

            _feedback = DOTween.To(x =>
                {
                    var lerpAmount = x;

                    foreach (var mr in _renderers)
                    {
                        var materaial = mr.material;
                        materaial.shader = _outlineShader;
                        materaial.SetColor("_OutlineColor", Color.red);
                        materaial.SetFloat("_OutlineThickness", 0);
                        materaial.SetFloat("_Red", lerpAmount);
                        mr.material = materaial;
                    }
                }, 1f, 0f, 0.5f)
                .OnComplete(() =>
                {
                    foreach (var mr in _renderers)
                    {
                        var materaial = mr.material;
                        materaial.shader = _defaultShader;
                        materaial.SetColor("_OutlineColor", Color.red);
                        materaial.SetFloat("_OutlineThickness", 0);
                        materaial.SetFloat("_Red", 0);
                        mr.material = materaial;
                    }
                });
        }
    }
}
