using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
public class ConditionalFieldAttributeDrawer : PropertyDrawer
{
    private ConditionalFieldAttribute _attribute;
    private bool _toShow = true;

    private ConditionalFieldAttribute Attribute => _attribute ?? (_attribute = attribute as ConditionalFieldAttribute);

    private string PropertyToCheck => Attribute != null ? _attribute.PropertyToCheck : null;

    private object CompareValue => Attribute != null ? _attribute.CompareValue : null;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return _toShow ? EditorGUI.GetPropertyHeight(property) : 0;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!string.IsNullOrEmpty(PropertyToCheck))
        {
            var conditionProperty = FindPropertyRelative(property, PropertyToCheck);
            if (conditionProperty != null)
            {
                var isBoolMatch = conditionProperty.propertyType == SerializedPropertyType.Boolean &&
                                  conditionProperty.boolValue;

                var compareStringValue = CompareValue != null ? CompareValue.ToString().ToUpper() : "NULL";
                if (isBoolMatch && compareStringValue == "FALSE")
                {
                    isBoolMatch = false;
                }

                var conditionPropertyStringValue = conditionProperty.ToString().ToUpper();
                var objectMatch = compareStringValue == conditionPropertyStringValue;

                if (!isBoolMatch && !objectMatch)
                {
                    _toShow = false;
                    return;
                }
            }
        }

        _toShow = true;
        EditorGUI.PropertyField(position, property, label, true);
    }

    private SerializedProperty FindPropertyRelative(SerializedProperty property, string toGet)
    {
        if (property.depth == 0)
        {
            return property.serializedObject.FindProperty(toGet);
        }

        var path = property.propertyPath.Replace("Array.data[", "[");
        var elements = path.Split('.');
        SerializedProperty parent = null;
        for (var i = 0; i < elements.Length - 1; i++)
        {
            var element = elements[i];
            var index = -1;
            if (element.Contains("["))
            {
                index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal))
                    .Replace("[", "").Replace("]", ""));
                element = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
            }

            parent = i == 0 ? property.serializedObject.FindProperty(element) : parent.FindPropertyRelative(element);

            if (index >= 0)
            {
                parent = parent.GetArrayElementAtIndex(index);
            }
        }

        var lol = parent.propertyPath;
        return parent.FindPropertyRelative(toGet);
    }
}
