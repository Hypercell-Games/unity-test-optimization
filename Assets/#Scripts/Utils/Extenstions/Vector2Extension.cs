using UnityEngine;

public static class Vector2Extension
{
    public static float Angle(this Vector2 v)
    {
        var angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        if (angle < 0)
        {
            angle += 360;
        }

        return angle;
    }

    public static Vector2 SetAngle(this Vector2 v, float degrees)
    {
        return SetAngleRad(v, degrees * Mathf.Deg2Rad);
    }

    public static Vector2 SetAngleRad(this Vector2 v, float radians)
    {
        v.x = v.magnitude;
        v.y = 0f;
        v = v.RotateRad(radians);

        return new Vector2(v.x, v.y);
    }

    public static Vector2 RotateRad(this Vector2 v, float radians)
    {
        var cos = Mathf.Cos(radians);
        var sin = Mathf.Sin(radians);

        var newX = v.x * cos - v.y * sin;
        var newY = v.x * sin + v.y * cos;

        v.x = newX;
        v.y = newY;

        return new Vector2(v.x, v.y);
    }

    public static Vector2 SetLength(this Vector2 v, float len)
    {
        return SetLength2(v, len * len);
    }

    public static Vector2 SetLength2(this Vector2 v, float len2)
    {
        var oldLen2 = v.sqrMagnitude;
        var output = new Vector2(v.x, v.y);

        if (oldLen2 == 0 || oldLen2 == len2)
        {
            return output;
        }

        output = output.Scl(Mathf.Sqrt(len2 / oldLen2));
        return output;
    }

    public static Vector2 Scl(this Vector2 v, float scalar)
    {
        var output = new Vector2(v.x, v.y);

        output.x *= scalar;
        output.y *= scalar;

        return output;
    }

    public static float Angle(this Vector2 v, Vector2 reference)
    {
        return Mathf.Atan2(v.Crs(reference), v.Dot(reference)) * Mathf.Rad2Deg;
    }

    public static float Crs(this Vector2 v, Vector2 other)
    {
        return v.x * other.y - v.y * other.x;
    }

    public static float Dot(this Vector2 v, Vector2 other)
    {
        return v.x * other.x + v.y * other.y;
    }

    public static Vector2 AddX(this Vector2 v, float value)
    {
        v.x += value;
        return v;
    }

    public static Vector2 AddY(this Vector2 v, float value)
    {
        v.y += value;
        return v;
    }

    public static Vector2 SetX(this Vector2 v, float value)
    {
        v.x = value;
        return v;
    }

    public static Vector2 SetY(this Vector2 v, float value)
    {
        v.y = value;
        return v;
    }
}
