using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
public class ShowOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        var showOnlyAttribute = attribute as ShowOnlyAttribute;

        if (showOnlyAttribute.onlyRuntime && Application.isPlaying == false)
        {
            DrawProperty(position, prop, label);
        }
        else
        {
            GUI.enabled = false;
            DrawProperty(position, prop, label);
            GUI.enabled = true;
        }
    }

    private void DrawProperty(Rect position, SerializedProperty prop, GUIContent label)
    {
        if (!prop.hasVisibleChildren)
        {
            EditorGUI.PropertyField(position, prop, label);
        }
        else
        {
            EditorPropertyHelper.DrawProperty(position, prop);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorPropertyHelper.GetPropertyHeight(property);
    }
}
