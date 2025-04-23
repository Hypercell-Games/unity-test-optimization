using UnityEngine;

public static class PlayerPrefsController
{
    #region STRING

    public static void SetString(EPlayerPrefsKey key, string postfix, string value, bool autoSave = true)
    {
        var stringKey = GetKey(key, postfix);
        SetString(stringKey, value, autoSave);
    }

    public static void SetString(EPlayerPrefsKey key, string value, bool autoSave = true)
    {
        SetString(key, string.Empty, value, autoSave);
    }

    public static string GetString(EPlayerPrefsKey key, string postfix = null, string defaultValue = default)
    {
        var stringKey = GetKey(key, postfix);
        var value = GetString(stringKey, defaultValue);
        return value;
    }

    private static void SetString(string key, string value, bool autoSave = true)
    {
        PlayerPrefs.SetString(key, value);

        if (autoSave)
        {
            Save();
        }
    }

    private static string GetString(string key, string defaultValue = default)
    {
        var value = PlayerPrefs.GetString(key, defaultValue);
        return value;
    }

    #endregion STRING

    #region INT

    public static void SetInt(EPlayerPrefsKey key, int value, bool autoSave = true)
    {
        SetInt(key, null, value, autoSave);
    }

    public static int GetInt(EPlayerPrefsKey key, int defaultValue = default)
    {
        var value = GetInt(key, null, defaultValue);
        return value;
    }

    public static void SetInt(EPlayerPrefsKey key, string postfix, int value, bool autoSave = true)
    {
        var stringKey = GetKey(key, postfix);
        SetInt(stringKey, value, autoSave);
    }

    public static int GetInt(EPlayerPrefsKey key, string postfix, int defaultValue = default)
    {
        var stringKey = GetKey(key, postfix);
        var value = GetInt(stringKey, defaultValue);
        return value;
    }

    private static void SetInt(string key, int value, bool autoSave = true)
    {
        PlayerPrefs.SetInt(key, value);

        if (autoSave)
        {
            Save();
        }
    }

    private static int GetInt(string key, int defaultValue = default)
    {
        var value = PlayerPrefs.GetInt(key, defaultValue);
        return value;
    }

    #endregion INT

    #region FLOAT

    public static void SetFloat(EPlayerPrefsKey key, float value, bool autoSave = true)
    {
        SetFloat(key, null, value, autoSave);
    }

    public static float GetFloat(EPlayerPrefsKey key, float defaultValue = default)
    {
        var value = GetFloat(key, null, defaultValue);
        return value;
    }

    public static void SetFloat(EPlayerPrefsKey key, string postfix, float value, bool autoSave = true)
    {
        var stringKey = GetKey(key, postfix);
        SetFloat(stringKey, value, autoSave);
    }

    public static float GetFloat(EPlayerPrefsKey key, string postfix, float defaultValue = default)
    {
        var stringKey = GetKey(key, postfix);
        var value = GetFloat(stringKey, defaultValue);
        return value;
    }

    private static void SetFloat(string key, float value, bool autoSave = true)
    {
        PlayerPrefs.SetFloat(key, value);

        if (autoSave)
        {
            Save();
        }
    }

    private static float GetFloat(string key, float defaultValue = default)
    {
        var value = PlayerPrefs.GetFloat(key, defaultValue);
        return value;
    }

    #endregion FLOAT

    #region BOOL

    public static void SetBool(EPlayerPrefsKey key, bool value, bool autoSave = true)
    {
        SetBool(key, null, value, autoSave);
    }

    public static bool GetBool(EPlayerPrefsKey key, bool defaultValue = default)
    {
        var value = GetBool(key, null, defaultValue);
        return value;
    }

    public static void SetBool(EPlayerPrefsKey key, string postfix, bool value, bool autoSave = true)
    {
        var stringKey = GetKey(key, postfix);
        SetBool(stringKey, value, autoSave);
    }

    public static bool GetBool(EPlayerPrefsKey key, string postfix, bool defaultValue = default)
    {
        var stringKey = GetKey(key, postfix);
        var value = GetBool(stringKey, defaultValue);
        return value;
    }

    private static void SetBool(string key, bool value, bool autoSave = true)
    {
        var intValue = value ? 1 : 0;
        SetInt(key, intValue, autoSave);
    }

    private static bool GetBool(string key, bool defaultValue = default)
    {
        var intDefaultValue = defaultValue ? 1 : 0;
        var intValue = GetInt(key, intDefaultValue);
        var value = intValue != 0;
        return value;
    }

    #endregion BOOL

    #region CONTROL

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static void Clear(bool autoSave = true)
    {
        PlayerPrefs.DeleteAll();

        if (autoSave)
        {
            Save();
        }
    }

    #endregion CONTROL

    #region KEYS

    public static bool HasKey(EPlayerPrefsKey key, string postfix = null)
    {
        var stringKey = GetKey(key, postfix);
        var result = PlayerPrefs.HasKey(stringKey);
        return result;
    }

    public static void DeleteKey(EPlayerPrefsKey key, string postfix = null, bool autoSave = true)
    {
        var stringKey = GetKey(key, postfix);
        PlayerPrefs.DeleteKey(stringKey);

        if (autoSave)
        {
            Save();
        }
    }

    public static string GetKey(EPlayerPrefsKey playerPrefsKey, string postfix = null)
    {
        if (!string.IsNullOrEmpty(postfix))
        {
            return $"{playerPrefsKey.ToString()}_{postfix}";
        }

        return playerPrefsKey.ToString();
    }

    #endregion KEYS
}
