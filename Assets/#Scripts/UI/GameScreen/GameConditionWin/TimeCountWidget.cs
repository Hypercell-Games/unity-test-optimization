using Core.Messenger;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeCountWidget : MonoBehaviour,
    IMessageListener<ColorThemeChangedMessage>
{
    private static Color _redColor = new Color32(228, 101, 101, 255);
    private static Color _normalColor = new Color32(51, 59, 102, 255);
    [SerializeField] private Image _timeImage;
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private Image _redFrameImage;
    [SerializeField] private bool _switchColor;
    [SerializeField] private ColorThemeTextScriptableObject _normalColorThemeText;
    [SerializeField] private Image _clockLightImage;
    [SerializeField] private Image _clockDarkImage;
    private ColorThemeType? _themeType;

    private Sequence _timeSeq;

    private void Awake()
    {
        if (!_switchColor)
        {
            return;
        }

        Messenger.Subscribe(this);
        UpdateColor(ColorThemeLogicUtil.CurrentColorTheme);
    }

    private void OnDestroy()
    {
        if (!_switchColor)
        {
            return;
        }

        Messenger.Unsubscribe(this);
    }

    public void OnMessage(ColorThemeChangedMessage message)
    {
        UpdateColor(message.ColorThemeType);
    }

    public void UpdateTime(int count, int original, bool tweenValue)
    {
        _timeImage.fillAmount = (float)count / original;
        var timeStr = GetTimeStr(count);
        if (timeStr == _timeText.text)
        {
            return;
        }

        var timeRedEffect = GameConfig.RemoteConfig.timeRedEffect;
        if (count <= timeRedEffect)
        {
            _redFrameImage.SetAlpha(1f);
            _timeText.color = _redColor;
            _timeText.transform.localScale = Vector3.one * 1.1f;
            _timeSeq?.Kill();
            _timeSeq = DOTween.Sequence()
                .Append(_timeText.transform.DOScale(1f, 1f).SetEase(Ease.Linear))
                .Join(_redFrameImage.DOFade(0.5f, 1f).SetEase(Ease.Linear));
        }
        else
        {
            _timeText.color = _normalColor;
            _timeSeq?.Kill();
            _timeText.transform.localScale = Vector3.one;
            _redFrameImage.SetAlpha(0f);
        }

        _timeText.text = timeStr;
    }

    public static string GetTimeStr(int count)
    {
        var seconds = count % 60;
        var minutes = count / 60;
        return $"{minutes:00}:{seconds:00}";
    }

    public void SetActive(bool isEnabled)
    {
        gameObject.SetActive(isEnabled);
        _redFrameImage.gameObject.SetActive(isEnabled);
        _redFrameImage.SetAlpha(0f);
    }

    private void UpdateColor(ColorThemeType colorThemeType)
    {
        if (!_switchColor && _themeType == colorThemeType)
        {
            return;
        }

        _themeType = colorThemeType;

        _clockLightImage.gameObject.SetActive(colorThemeType == ColorThemeType.Light);
        _clockDarkImage.gameObject.SetActive(colorThemeType == ColorThemeType.Dark);
        _normalColor = _normalColorThemeText.GetColorTheme(colorThemeType).color;
        switch (colorThemeType)
        {
            case ColorThemeType.Light:
                _redColor = new Color32(228, 101, 101, 255);
                break;
            case ColorThemeType.Dark:
                _redColor = new Color32(228, 101, 101, 255);
                break;
        }
    }
}
