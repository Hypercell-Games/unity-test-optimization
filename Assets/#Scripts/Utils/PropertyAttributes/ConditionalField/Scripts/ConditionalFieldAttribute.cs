using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class ConditionalFieldAttribute : PropertyAttribute
{
    public object CompareValue;
    public string PropertyToCheck;

    public ConditionalFieldAttribute(string propertyToCheck, object compareValue = null)
    {
        PropertyToCheck = propertyToCheck;
        CompareValue = compareValue;
    }
}
