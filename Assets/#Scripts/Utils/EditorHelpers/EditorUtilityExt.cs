#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

public class EditorUtilityExt
{
#if UNITY_EDITOR
    public static T[] GetAllInstances<T>(T except = null) where T : ScriptableObject
    {
        var exceptPath = except != null ? AssetDatabase.GetAssetPath(except) : null;
        var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        var instances = new T[guids.Length];

        for (var i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (exceptPath?.Equals(path) ?? false)
            {
                continue;
            }

            instances[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        }

        return instances;
    }
#endif
}
