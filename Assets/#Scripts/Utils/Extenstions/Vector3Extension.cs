using UnityEngine;

public static class Vector3Extension
{
    public static Vector3 Abs(this Vector3 vector3)
    {
        vector3.x = Mathf.Abs(vector3.x);
        vector3.y = Mathf.Abs(vector3.y);
        vector3.z = Mathf.Abs(vector3.z);
        return vector3;
    }
}
