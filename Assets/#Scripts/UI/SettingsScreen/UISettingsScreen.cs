using System;
using System.Text;
using Game;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Zenject;

public class UISettingsScreen : BaseScreen
{
    [SerializeField] private TMP_Text _configVersionText;

    [Header("SwitchersSpriteRenderers")] [SerializeField]
    private Image _musicSwitcherImage;

    [SerializeField] private Image _soundsSwitcherImage;
    [SerializeField] private Image _vibrationSwitcherImage;
    [SerializeField] private Image _exitBackgroundImage;

    [Header("Buttons")] [SerializeField] private Button _musicButton;

    [SerializeField] private Button _soundsButton;
    [SerializeField] private Button _vibrationButton;
    [SerializeField] private Button _rateUsButton;
    [SerializeField] private TextButtonController _restorePurchasesButton;

    [Header("Rules")] [SerializeField] private Button _privacyPolicyButton;

    [SerializeField] private Button _termsOfUseButton;

    [Header("Exit")] [SerializeField] private BaseButtonController _exitButton;

    [SerializeField] private Button _exitBackgroundButton;
    [SerializeField] private Button _exitBottomButton;

    [Header("Sprites")] [SerializeField] private Sprite _switcherOnSprite;

    [SerializeField] private Sprite _switcherOffSprite;

    [Inject] private SettingsController _settingsController;

    private void Awake()
    {
        Hide(true);
    }

    private void OnEnable()
    {
        AddListeners();
        _configVersionText.text = $"v{HyperKit.Data.CurrentConfigVersion}";
    }

    private void OnDisable()
    {
        RemoveListeners();
    }

    public override void Show(bool force = false, Action callback = null)
    {
        if (gameObject.scene.name == Scenes.GAME)
        {
            PauseUtil.BossLevelPaused();
        }

        _exitBackgroundImage.raycastTarget = true;


        SetSwitchers();

        base.Show(force, delegate
        {
            callback?.Invoke();
            EnableRaycast();
        });

        void EnableRaycast()
        {
        }
    }

    private void AddListeners()
    {
        _vibrationButton?.onClick.AddListener(OnClickVibrationButton);
        _exitBackgroundButton?.onClick.AddListener(CloseSettings);
        _exitBottomButton?.onClick.AddListener(CloseSettings);
        _exitButton.onButtonClicked += CloseSettings;

        _restorePurchasesButton.SetOnButtonClick(() =>
        {
            MessageToast.Instance.Show("Restore requested.");
            if (HyperKit.IAP.IsReady())
            {
                HyperKit.IAP.RestorePurchases();
            }
        });
    }

    private void RemoveListeners()
    {
        _vibrationButton?.onClick.RemoveListener(OnClickVibrationButton);
        _exitBackgroundButton?.onClick.RemoveListener(CloseSettings);
        _exitBottomButton?.onClick.RemoveListener(CloseSettings);
        _exitButton.onButtonClicked -= CloseSettings;
    }

    private void CloseSettings()
    {
        _exitBackgroundImage.raycastTarget = false;

        Hide(false, () =>
        {
            if (gameObject.scene.name == Scenes.GAME)
            {
                PauseUtil.BossLevelUnpaused();
            }
        });
    }

    private void OnClickVibrationButton()
    {
        _settingsController.SetVibrationEnable(!_settingsController.IsVibrationEnable);

        SetSwitcherState(_vibrationSwitcherImage, _settingsController.IsVibrationEnable);
    }

    private void SetSwitchers()
    {
        SetSwitcherState(_vibrationSwitcherImage, _settingsController.IsVibrationEnable);
    }

    private void SetSwitcherState(Image switcherImage, bool state)
    {
        if (state)
        {
            switcherImage.sprite = _switcherOnSprite;
        }
        else
        {
            switcherImage.sprite = _switcherOffSprite;
        }
    }
}
