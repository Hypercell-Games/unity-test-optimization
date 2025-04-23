using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class ShowOnlyAttribute : PropertyAttribute
{
    public readonly bool onlyRuntime;

    public ShowOnlyAttribute()
    {
        onlyRuntime = false;
    }

    public ShowOnlyAttribute(bool isOnlyRuntime)
    {
        onlyRuntime = isOnlyRuntime;
    }
}
