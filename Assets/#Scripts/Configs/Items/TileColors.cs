using UnityEngine;

public static class TileColors
{
    public static Color GetColor(ETileColor tileColor)
    {
        return tileColor switch
        {
            ETileColor.None => Color.white,
            ETileColor.Red => Color.red,
            ETileColor.Orange => new Color32(255, 165, 0, 255),
            ETileColor.Yellow => Color.yellow,
            ETileColor.Green => Color.green,
            ETileColor.Mint => new Color32(170, 240, 209, 255),
            ETileColor.LightBlue => new Color32(173, 216, 230, 255),
            ETileColor.Blue => Color.blue,
            ETileColor.Magenta => Color.magenta,
            ETileColor.Black => Color.black,
            ETileColor.Purple => new Color32(128, 0, 128, 255),
            ETileColor.Pink => new Color32(255, 192, 203, 255),
            ETileColor.Brown => new Color32(165, 42, 42, 255),
            ETileColor.Baige => new Color32(245, 245, 220, 255),
            _ => Color.white
        };
    }
}

public enum ETileColor
{
    None = 0,
    Red = 16711680,
    Orange = 16753920,
    Yellow = 16776960,
    Green = 32768,
    Mint = 11202769,
    LightBlue = 11393254,
    Blue = 255,
    Magenta = 16711935,
    Black = 1,
    Purple = 8388736,
    Pink = 16761035,
    Brown = 10824234,
    Baige = 16119260
}
