using UnityEngine.UI;

public static class MaskableGraphicExtenstions
{
    public static void SetFade(this MaskableGraphic graphic, float fade)
    {
        var color = graphic.color;
        color.a = fade;

        graphic.color = color;
    }
}
