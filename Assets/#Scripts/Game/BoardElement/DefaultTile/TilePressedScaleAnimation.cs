using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "TilePressedScaleAnimationSetting")]
public class TilePressedScaleAnimation : ScriptableObject
{
    [SerializeField] [Range(0f, 1f)] private float _pressedDuration = 0.1f;
    [SerializeField] private Ease _pressedEase = Ease.OutQuad;

    [SerializeField] [Range(0f, 1f)] private float _unPressedDuration = 0.3f;
    [SerializeField] private Ease _unPressedEase = Ease.OutBack;

    public float PressedDuration => _pressedDuration;
    public Ease PressedEase => _pressedEase;
    public float UnPressedDuration => _unPressedDuration;
    public Ease UnPressedEase => _unPressedEase;
}
