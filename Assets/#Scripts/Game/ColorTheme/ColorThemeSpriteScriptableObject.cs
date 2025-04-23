using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ColorThemeSpriteSwitcher", menuName = "ColorTheme/New ColorThemeSpriteSwitcher")]
public class ColorThemeSpriteScriptableObject : ScriptableObject
{
    [SerializeField] private ColorThemeSprite[] _spriteThemes;

    public void SetColorTheme(Image image, ColorThemeType colorThemeType)
    {
        if (image == null || _spriteThemes == null)
        {
            return;
        }

        _spriteThemes.For(t =>
        {
            if (t == null)
            {
                return;
            }

            if (t.colorThemeType == colorThemeType)
            {
                image.sprite = t.sprite;
                image.color = t.color;
            }
        });
    }
}
