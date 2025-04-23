using UnityEngine;

[CreateAssetMenu(fileName = "String Variable", menuName = "Utils/Variables/String Variable")]
public class StringVariable : ScriptableObject
{
#if UNITY_EDITOR
    [Multiline] public string DeveloperDescription = "";
#endif
    public string DefaultValue;

    [SerializeField] [ShowOnly] private string m_CurrentValue;

    public StringEvent onChange;

    [Header("Persistence")] public bool isPersistent;

    [ConditionalField("isPersistent")] public string prefsName;
    public string Value { get; private set; }

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

    public void SetValue(string value)
    {
        Value = value;

        RiseOnChange();

        if (isPersistent)
        {
            Save();
        }
    }

    public void SetValue(StringVariable value)
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
        PlayerPrefs.SetString(prefsName, Value);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        Value = PlayerPrefs.GetString(prefsName, DefaultValue);

        m_CurrentValue = Value;
    }

    public void Clear()
    {
        PlayerPrefs.DeleteKey(prefsName);
        PlayerPrefs.Save();
    }

    public void RiseOnChange()
    {
        onChange?.Raise(Value);

        m_CurrentValue = Value;
    }
}
