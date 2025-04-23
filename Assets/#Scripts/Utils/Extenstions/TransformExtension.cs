#region

using UnityEngine;

#endregion

public static class TransformExtension
{
    public static void ClearChildren(this Transform that)
    {
        for (var i = that.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(that.GetChild(i).gameObject);
#else
            GameObject.Destroy(that.GetChild(i).gameObject);
#endif
        }
    }
}
