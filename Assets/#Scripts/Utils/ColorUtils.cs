using UnityEngine;
using UnityEngine.UI;

public static class ColorUtils
{
    public static void SetAlpha(this Graphic graphic, float alpha)
    {
        graphic.color = graphic.color.SetAlpha(alpha);
    }

    public static Color SetAlpha(this Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
}
