using System;
using DG.Tweening;
using UnityEngine;

namespace Unpuzzle
{
    public class PinFXController : MonoBehaviour
    {
        [SerializeField] private float blockFXTime = 1;
        [SerializeField] private float blockFXOutlineThickness = 0.3f;

        [SerializeField] private ParticleSystem trailParticleSystem;

        [SerializeField] private TrailRenderer[] trails;

        private Shader _shaderEffect;
        private HookController hookController;
        private Tween outLineTween;

        private void Start()
        {
            hookController = GetComponent<HookController>();
            _shaderEffect = hookController.OutlineShader;
            Array.ForEach(trails, a => a.enabled = false);
        }

        public void BlockFXActivate(Color defaultColor)
        {
            if (hookController.IsInActive)
            {
                return;
            }

            if (outLineTween != null)
            {
                outLineTween.Kill(true);
            }

            if (hookController.MeshRenderers.Count == 0)
            {
                return;
            }

            var propertyBlock = new MaterialPropertyBlock();
            hookController.MeshRenderers.Find(a => a != null).GetPropertyBlock(propertyBlock);

            var pinMaterial = new Material(hookController.MeshRenderers[0].material);
            var defaultShader = pinMaterial.shader;
            var touchColor = ColorHolder.Instance.GetTouchColor();

            outLineTween = DOTween.To(x =>
                {
                    var lerpAmount = x;
                    var color = Color.Lerp(defaultColor, touchColor, x);
                    foreach (var mr in hookController.MeshRenderers)
                    {
                        var material = mr.material;
                        material.color = color;
                        material.shader = _shaderEffect;
                        propertyBlock.SetColor("_OutlineColor", Color.red);
                        propertyBlock.SetFloat("_OutlineThickness", lerpAmount * blockFXOutlineThickness);
                        propertyBlock.SetFloat("_Red", lerpAmount);
                        mr.material = material;
                        mr.SetPropertyBlock(propertyBlock);
                    }
                }, 1f, 0f, blockFXTime)
                .OnKill(() =>
                {
                    hookController.UpdateMaterial();
                })
                .OnComplete(() =>
                {
                    hookController.UpdateMaterial();
                });
        }

        public void TutorialDisable()
        {
            if (hookController.IsInActive)
            {
                return;
            }

            outLineTween?.Kill(true);
            hookController.UpdateMaterial();
        }

        private void StartHint(bool isBoosterUsed = false)
        {
            if (hookController.IsInActive)
            {
                return;
            }

            outLineTween?.Kill(true);
            var outlineColor = Color.white;
            var layer = LayerMask.NameToLayer("TutorialPin");


            if (hookController.MeshRenderers.Count == 0)
            {
                return;
            }


            outLineTween = DOTween.To(x =>
                {
                    var lerpAmount = x;


                    foreach (var mr in hookController.MeshRenderers)
                    {
                        if (isBoosterUsed)
                        {
                            mr.gameObject.layer = layer;
                        }

                        var pinMaterial = mr.material;
                        pinMaterial.shader = _shaderEffect;
                        pinMaterial.SetColor("_OutlineColor", outlineColor);
                        pinMaterial.SetFloat("_OutlineThickness", lerpAmount * blockFXOutlineThickness);
                        pinMaterial.SetFloat("_Red", 0);
                        mr.material = pinMaterial;
                    }
                }, 0f, 1f, blockFXTime * 2f)
                .SetLink(hookController.gameObject)
                .SetLoops(-1, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    hookController.UpdateMaterial();
                });
        }

        public void TrailFXActivate(TrailRenderer trail)
        {
            if (trailParticleSystem != null)
            {
                trailParticleSystem.Play();
            }

            for (var i = 0; i < trails.Length; i++)
            {
                trails[i].enabled = trail == trails[i];
            }
        }

        public void SetTrailColor(Color color)
        {
            for (var i = 0; i < trails.Length; i++)
            {
                color.a = 0.5f;
                trails[i].startColor = color;
                color.a = 0f;
                trails[i].endColor = color;
            }
        }

        public void OnPress()
        {
            if (hookController.IsInActive)
            {
                return;
            }

            outLineTween?.Kill(true);

            PlayJuice();
            if (hookController.MeshRenderers.Count == 0)
            {
                return;
            }


            if (hookController.IsFire)
            {
                return;
            }

            outLineTween = DOTween.To(x =>
            {
                var lerpAmount = x;

                foreach (var mr in hookController.MeshRenderers)
                {
                    var pinMaterial = mr.material;
                    pinMaterial.shader = _shaderEffect;
                    pinMaterial.SetFloat("_OutlineThickness",
                        Mathf.Lerp(blockFXOutlineThickness, blockFXOutlineThickness * 2f, lerpAmount));
                }
            }, 0f, 1f, 0.5f);
        }

        private void PlayJuice()
        {
            if (!GameConfig.RemoteConfig.juiceEffectEnable || hookController.pinType == PinType.pinBolt)
            {
                return;
            }

            var juiceFx = ParticlesSpawner.Instance.SpawnParticle(ParticleId.pinJuice,
                hookController.transform.position, hookController.transform.rotation);

            var main = juiceFx.ParticleSystem.main;
            main.startColor = hookController.Color;

            var shape = juiceFx.ParticleSystem.shape;
            var scale = hookController.GetBounds() * 2f;
            scale.x = Mathf.Max(6f, scale.x);
            scale.z = Mathf.Max(1f, scale.z);
            shape.scale = scale;

            juiceFx.StartPlaying();
        }

        public void DisableFx()
        {
            outLineTween?.Kill(true);
        }

        public void OnUp()
        {
            PlayJuice();

            if (hookController.IsFire || hookController.MeshRenderers.Count == 0)
            {
                return;
            }

            outLineTween?.Kill(true);
            outLineTween = DOTween.To(x =>
                {
                    var lerpAmount = x;


                    foreach (var mr in hookController.MeshRenderers)
                    {
                        var pinMaterial = mr.material;
                        pinMaterial.shader = _shaderEffect;
                        pinMaterial.SetFloat("_OutlineThickness",
                            Mathf.Lerp(blockFXOutlineThickness, blockFXOutlineThickness * 2f, lerpAmount));
                    }
                }, 1f, 0f, 0.5f)
                .OnComplete(() =>
                {
                    hookController.UpdateMaterial();
                });
        }
    }
}
