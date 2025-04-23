using UnityEditor;
using UnityEngine;

public static class KillPrefsTool
{
    [MenuItem("Tools/Kill Prefs")]
    public static void KillPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
