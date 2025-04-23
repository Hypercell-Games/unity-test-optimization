using System;
using UnityEngine;

public class UILevelFailedScreen : BaseScreen
{
    [Header("Buttons")] [SerializeField] private BaseButtonController _restartButton;

    [SerializeField] private BaseButtonController _skipLevelButton;

    [Header("SpriteChanger")] [SerializeField]
    private FinalSpriteChanger _finalSpriteChanger;

    [SerializeField] private GameObject _cryEmoji;

    private void Awake()
    {
        Hide(true);
    }

    private void OnEnable()
    {
        AddListeners();
    }

    private void OnDisable()
    {
        RemoveListeners();
    }

    public event Action<ELevelCompleteReason> onRestart;
    public event Action onSkipLevel;

    public override void Show(bool force = false, Action callback = null)
    {
        _cryEmoji.SetActive(true);
        _finalSpriteChanger.SetRandomSprite();

        base.Show(force, delegate { callback?.Invoke(); });
    }

    public override void Hide(bool force = false, Action callback = null)
    {
        _cryEmoji.SetActive(false);

        base.Hide(force, callback);
    }

    private void AddListeners()
    {
        _restartButton.onButtonClicked += RestartLevel;
        _skipLevelButton.onButtonClicked += SkipLevel;
    }

    private void RemoveListeners()
    {
        _restartButton.onButtonClicked -= RestartLevel;
        _skipLevelButton.onButtonClicked -= SkipLevel;
    }

    private void SkipLevel()
    {
        HyperKit.Ads.ShowRewardedAd("ad_rewarded_level_skip",
            result =>
            {
                if (!result)
                {
                    return;
                }

                Hide();
                onRestart?.Invoke(ELevelCompleteReason.SKIP);
            });
    }

    private void RestartLevel()
    {
        VibrationController.Instance.Play(EVibrationType.LightImpact);

        Hide();

        onRestart?.Invoke(ELevelCompleteReason.LOSE);
    }
}
