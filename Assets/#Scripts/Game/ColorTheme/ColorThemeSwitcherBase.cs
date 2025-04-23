using System;
using Core.Messenger;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class ColorThemeSwitcherBase : MonoBehaviour,
    IMessageListener<ColorThemeChangedMessage>
{
    [SerializeField] protected bool _switch;

    private ColorThemeType? _themeType;

    protected void Awake()
    {
        if (!_switch)
        {
            return;
        }

        Messenger.Subscribe(this);
        TryChangeTheme(ColorThemeLogicUtil.CurrentColorTheme);
    }

    protected void OnDestroy()
    {
        if (!_switch)
        {
            return;
        }

        Messenger.Unsubscribe(this);
    }

    public void OnMessage(ColorThemeChangedMessage message)
    {
        TryChangeTheme(message.ColorThemeType);
    }

    private void TryChangeTheme(ColorThemeType colorThemeType)
    {
        if (!_switch)
        {
            return;
        }

        if (_themeType == colorThemeType)
        {
            return;
        }

        _themeType = colorThemeType;
        ChangeTheme(colorThemeType);
    }

    protected abstract void ChangeTheme(ColorThemeType colorThemeType);
}

public interface ISetColorTheme
{
    void SetColorTheme(ColorThemeType colorThemeType);
}

[Serializable]
public class ColorThemeImageComponent : ISetColorTheme
{
    [SerializeField] private Image image;
    [SerializeField] private ColorThemeSpriteScriptableObject spriteThemes;

    public void SetColorTheme(ColorThemeType colorThemeType)
    {
        spriteThemes.SetColorTheme(image, colorThemeType);
    }
}

[Serializable]
public class ColorThemeTextComponent : ISetColorTheme
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private ColorThemeTextScriptableObject textThemes;

    public void SetColorTheme(ColorThemeType colorThemeType)
    {
        textThemes.SetColorTheme(text, colorThemeType);
    }
}

[Serializable]
public class ColorThemeSprite
{
    public ColorThemeType colorThemeType;
    public Sprite sprite;
    public Color color = Color.white;
}

[Serializable]
public class ColorThemeText
{
    public ColorThemeType colorThemeType;
    public Color color = Color.white;
}
