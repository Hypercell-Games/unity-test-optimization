using Core.Messenger;

public class ColorThemeChangedMessage : IMessage
{
    public ColorThemeChangedMessage(ColorThemeType colorThemeType)
    {
        ColorThemeType = colorThemeType;
    }

    public ColorThemeType ColorThemeType { get; private set; }
}

public enum ColorThemeType
{
    Light,
    Dark
}
