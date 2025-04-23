using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationSetting", menuName = "Animations/New AnimationSetting")]
public class AnimationSetting : ScriptableObject
{
    [SerializeField] private bool _customCurve;
    [SerializeField] private Ease _ease = Ease.OutQuad;
    [SerializeField] private AnimationCurve _curve = new(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    [SerializeField] [Range(0f, 3f)] private float _duration = 0.3f;

    public float Duration => _duration;

    public float Evulate(float t)
    {
        if (_customCurve)
        {
            return DOVirtual.EasedValue(0f, 1f, t, _curve);
        }

        return DOVirtual.EasedValue(0f, 1f, t, _ease);
    }

    public float EvulateClamp01(float t)
    {
        return Mathf.Clamp01(Evulate(t));
    }

    public EaseFunction GetEaseFunction()
    {
        float easeFunction(float time, float duration, float overshootOrAmplitude, float period)
        {
            time /= duration;
            return Evulate(time);
        }

        return easeFunction;
    }
}
