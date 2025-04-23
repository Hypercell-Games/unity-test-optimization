using Core.Messenger;

public static class ColorThemeLogicUtil
{
    public static ColorThemeType CurrentColorTheme { get; private set; } = ColorThemeType.Light;

    public static void SetColorTheme(ColorThemeType colorThemeType)
    {
        CurrentColorTheme = colorThemeType;
        Messenger.Send(new ColorThemeChangedMessage(colorThemeType));
    }
}
