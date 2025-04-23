using UnityEngine;

public class ImageTextColorThemeSwitcher : ColorThemeSwitcherBase
{
    [SerializeField] private ColorThemeImageComponent[] _images;

    [SerializeField] private ColorThemeTextComponent[] _texts;

    protected override void ChangeTheme(ColorThemeType colorThemeType)
    {
        if (
            _images != null)
        {
            _images.For(t => t?.SetColorTheme(colorThemeType));
        }

        if (
            _texts != null)
        {
            _texts.For(t => t?.SetColorTheme(colorThemeType));
        }
    }
}
