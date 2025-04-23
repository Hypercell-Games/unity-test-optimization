using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoolVariable))]
public class BoolVariableEditor : Editor
{
    string valueToSet = "";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var variable = (BoolVariable)target;

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        valueToSet = GUILayout.TextField(valueToSet, 50);
        if (GUILayout.Button("Set value"))
        {
            variable.SetValue(bool.Parse(valueToSet));
            variable.Save();
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(20);

        if (variable.isPersistent)
        {
            if (GUILayout.Button("Load pref"))
            {
                variable.Load();
            }

            if (GUILayout.Button("Reset pref"))
            {
                variable.Clear();
            }

            if (GUILayout.Button("Set default"))
            {
                variable.SetValue(variable.DefaultValue);
                variable.Save();
            }

            if (GUILayout.Button("Reset all prefs"))
            {
                PlayerPrefs.DeleteAll();
            }
        }
    }
}
