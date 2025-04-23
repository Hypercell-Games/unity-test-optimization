using System;
using UnityEngine;

public abstract class JsonVariable<T> : JsonVariable where T : struct
{
    [Header("Json data")] public T DefaultValue;

    [SerializeField] private T m_CurrentValue;

    public Action<T> onChange;

    public T Value { get; private set; }

    private void OnEnable()
    {
        if (isPersistent)
        {
            Load();
        }
        else
        {
            Value = DefaultValue;
            m_CurrentValue = Value;
        }
    }

    public void SetValue(T value)
    {
        Value = value;

        RiseOnChange();

        if (isPersistent)
        {
            Save();
        }
    }

    public void SetValue(JsonVariable<T> value)
    {
        Value = value.Value;

        RiseOnChange();

        if (isPersistent)
        {
            Save();
        }
    }

    public void Save()
    {
        var strValue = JsonUtility.ToJson(Value);

        PlayerPrefs.SetString(prefsName, strValue);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        var strVal = PlayerPrefs.GetString(prefsName, null);

        Value = !string.IsNullOrEmpty(strVal) ? JsonUtility.FromJson<T>(strVal) : DefaultValue;
        m_CurrentValue = Value;
    }

    public void Clear()
    {
        PlayerPrefs.DeleteKey(prefsName);
        PlayerPrefs.Save();
    }

    public void RiseOnChange()
    {
        onChange?.Invoke(Value);
        m_CurrentValue = Value;
    }
}

public abstract class JsonVariable : ScriptableObject
{
    [Header("Persistence")] public bool isPersistent;

    [ConditionalField(nameof(isPersistent))]
    public string prefsName;
}
