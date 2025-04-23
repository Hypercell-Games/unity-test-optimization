using System;
using UnityEditor;
using UnityEngine;

public static class EditorPropertyHelper
{
    public static float GetPropertyHeight(SerializedProperty element)
    {
        var depth = element.depth;

        if (element.hasVisibleChildren)
        {
            if (element.isExpanded)
            {
                var height = 0f;

                element.NextVisible(true);
                do
                {
                    if (element.depth == depth)
                    {
                        break;
                    }

                    if (element.hasVisibleChildren)
                    {
                        if (element.isExpanded)
                        {
                            height += (element.Copy().CountInProperty() + 1) * EditorGUIUtility.singleLineHeight;
                        }
                        else
                        {
                            height += element.Copy().CountInProperty() * EditorGUIUtility.singleLineHeight;
                        }
                    }
                    else
                    {
                        height += element.Copy().CountInProperty() * EditorGUIUtility.singleLineHeight;
                    }
                } while (element.NextVisible(false));

                return height + EditorGUIUtility.singleLineHeight;
            }
        }

        return EditorGUIUtility.singleLineHeight;
    }

    public static void DrawProperty(Rect rect, SerializedProperty element, GUIContent label = null)
    {
        rect.height = EditorGUIUtility.singleLineHeight;
        var depth = element.depth;

        if (element.hasVisibleChildren)
        {
            rect.x += 15;
            rect.width -= 15;

            var cpy = element.Copy();
            string displayName;

            cpy.NextVisible(true);
            if (cpy.displayName == "Name" && !string.IsNullOrEmpty(cpy.stringValue))
            {
                displayName = cpy.stringValue;
            }
            else
            {
                displayName = element.displayName;
            }

            element.isExpanded = EditorGUI.Foldout(rect, element.isExpanded, displayName);

            if (element.isExpanded)
            {
                rect.x += 15;
                rect.width -= 15;
                rect.y += EditorGUIUtility.singleLineHeight;

                if (element.NextVisible(true))
                {
                    do
                    {
                        if (element.depth <= depth)
                        {
                            break;
                        }

                        if (element.hasVisibleChildren)
                        {
                            EditorGUI.PropertyField(rect, element, true);
                            if (element.isExpanded)
                            {
                                rect.y += (element.Copy().CountInProperty() + 1) * EditorGUIUtility.singleLineHeight;
                            }
                            else
                            {
                                rect.y += element.Copy().CountInProperty() * EditorGUIUtility.singleLineHeight;
                            }
                        }
                        else
                        {
                            EditorGUI.PropertyField(rect, element, false);
                            rect.y += element.Copy().CountInProperty() * EditorGUIUtility.singleLineHeight;
                        }
                    } while (element.NextVisible(false));
                }
            }
        }
        else
        {
            if (label == null)
            {
                EditorGUI.PropertyField(rect, element);
            }
            else
            {
                EditorGUI.PropertyField(rect, element, label);
            }
        }
    }

    public static string AsStringValue(this SerializedProperty property)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.String:
                return property.stringValue;

            case SerializedPropertyType.Character:
            case SerializedPropertyType.Integer:
                if (property.type == "char")
                {
                    return Convert.ToChar(property.intValue).ToString();
                }

                return property.intValue.ToString();

            case SerializedPropertyType.ObjectReference:
                return property.objectReferenceValue != null ? property.objectReferenceValue.ToString() : "null";

            case SerializedPropertyType.Boolean:
                return property.boolValue.ToString();

            case SerializedPropertyType.Enum:
                return property.enumNames[property.enumValueIndex];

            default:
                return string.Empty;
        }
    }
}
