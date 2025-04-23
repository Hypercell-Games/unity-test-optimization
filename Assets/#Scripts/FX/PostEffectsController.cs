using UnityEngine;

[ExecuteInEditMode]
public class PostEffectsController : MonoBehaviour
{
    [SerializeField] public Shader postShader;
    [SerializeField] public Camera distortionCamera;
    [SerializeField] public float intensity;

    Material postEffectMaterial;

    private void Awake()
    {
        postEffectMaterial = new Material(postShader);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        var width = source.width;
        var height = source.height;

        var startRT = RenderTexture.GetTemporary(width, height, -1, source.format);
        var distortionRT = RenderTexture.GetTemporary(width, height, -1, source.format);
        OnDistort(distortionRT);

        postEffectMaterial.SetTexture("_DistortionTexture", distortionRT);
        postEffectMaterial.SetFloat("_DistortionIntensity", intensity);

        Graphics.Blit(source, startRT, postEffectMaterial, 0);
        Graphics.Blit(startRT, destination);
        RenderTexture.ReleaseTemporary(startRT);
        RenderTexture.ReleaseTemporary(distortionRT);
    }

    void OnDistort(RenderTexture rt)
    {
        if (distortionCamera == null)
        {
            return;
        }

        distortionCamera.targetTexture = rt;
        distortionCamera.Render();
    }
}
