using UnityEngine;

public static class UnityExtensions
{
    public static void SetLayer(this GameObject gameObject, string layerName, bool includeChildren = false)
    {
        var layer = LayerMask.NameToLayer(layerName);
        SetLayer(gameObject, layer, includeChildren);
    }

    public static void SetLayer(this GameObject gameObject, int layer, bool includeChildren = false)
    {
        if (!gameObject)
        {
            return;
        }

        if (!includeChildren)
        {
            gameObject.layer = layer;
            return;
        }

        foreach (var child in gameObject.GetComponentsInChildren(typeof(Transform), true))
        {
            child.gameObject.layer = layer;
        }
    }
}
