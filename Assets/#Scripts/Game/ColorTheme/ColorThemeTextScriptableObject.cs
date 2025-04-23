using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorThemeTextSwitcher", menuName = "ColorTheme/New ColorThemeTextSwitcher")]
public class ColorThemeTextScriptableObject : ScriptableObject
{
    [SerializeField] private ColorThemeText[] _textThemes;

    public void SetColorTheme(TMP_Text text, ColorThemeType colorThemeType)
    {
        if (text == null || _textThemes == null)
        {
            return;
        }

        _textThemes.For(t =>
        {
            if (t == null)
            {
                return;
            }

            if (t.colorThemeType == colorThemeType)
            {
                text.color = t.color;
            }
        });
    }

    public ColorThemeText GetColorTheme(ColorThemeType colorThemeType)
    {
        if (_textThemes == null)
        {
            return null;
        }

        for (var i = 0; i < _textThemes.Length; i++)
        {
            var t = _textThemes[i];
            if (t == null)
            {
                continue;
            }

            if (t.colorThemeType == colorThemeType)
            {
                return t;
            }
        }

        return null;
    }
}
