using UnityEngine;

[CreateAssetMenu(fileName = "Bool Variable", menuName = "Utils/Variables/Bool Variable")]
public class BoolVariable : ScriptableObject
{
#if UNITY_EDITOR
    [Multiline] public string DeveloperDescription = "";
#endif
    public bool DefaultValue;

    [SerializeField] [ShowOnly] private bool m_CurrentValue;

    public BoolEvent onChange;

    [Header("Persistance")] public bool isPersistent;

    [ConditionalField("isPersistent")] public string prefsName;
    public bool Value { get; private set; }

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

    public void SetValue(bool value)
    {
        Value = value;

        RiseOnChange();

        if (isPersistent)
        {
            Save();
        }
    }

    public void SetValue(BoolVariable value)
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
        PlayerPrefs.SetInt(prefsName, Value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        Value = PlayerPrefs.GetInt(prefsName, DefaultValue ? 1 : 0) == 1 ? true : false;

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
