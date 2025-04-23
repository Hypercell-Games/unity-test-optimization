using UnityEngine;

public class TextColorThemeSwitcher : ColorThemeSwitcherBase
{
    [SerializeField] private ColorThemeTextComponent[] _texts;

    protected override void ChangeTheme(ColorThemeType colorThemeType)
    {
        if (_texts != null)
        {
            _texts.For(t => t?.SetColorTheme(colorThemeType));
        }
    }
}
