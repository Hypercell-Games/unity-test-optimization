using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unpuzzle;
using Zenject;
using Debug = UnityEngine.Debug;

public class DebugPanel : MonoBehaviour
{
    private static bool _enableFps;
    private static bool _enableCreativeHand;
    private static float _creativeHandAlpha;
    public static bool isAlternativeControl;
    public static bool controlLockX, controlLockY;
    public static bool isMoveOverride;
    public static int moveCount = 50;
    public static bool isEyeHandleEnabled;
    public static bool isEyeLookAtCursor;

    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private SkinsConfig _skinsConfig;
    [SerializeField] private Button _openButton;
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _configText;
    [SerializeField] private GameObject _statsPanel;
    [SerializeField] private TMP_Text _fpsText;
    [SerializeField] private Slider _loadLevelSlider;
    [SerializeField] private TMP_InputField _loadLevelInputField;
    [SerializeField] private Button _loadLevelButton;
    [SerializeField] private Button _loadPrevLevelButton;
    [SerializeField] private Button _loadNextLevelButton;
    [SerializeField] private TMP_Text _loadLevelText;
    [SerializeField] private Toggle _fpsToggle;
    [SerializeField] private Toggle _uiToggle;
    [SerializeField] private Toggle _controlToggle;
    [SerializeField] private Toggle _controlXToggle;
    [SerializeField] private Toggle _controlYToggle;
    [SerializeField] private Toggle _moveOverrideToggle;
    [SerializeField] private Toggle _eyesEnabled;
    [SerializeField] private Toggle _eyesLookAtCursor;
    [SerializeField] private TMP_InputField _moveCountText;
    [SerializeField] private TMP_Text _levelNameText;
    [SerializeField] private TMP_Text _solutionTimeText;
    [SerializeField] private TMP_Dropdown _materialSelect;

    [SerializeField] private Slider _skinSlider;
    [SerializeField] private TextMeshProUGUI _skinIndexLabel;
    [SerializeField] private Button _add3KeysButton;
    [SerializeField] private Button _addSoftCcyButton;
    [SerializeField] private IntVariable _softCcyVariable;

    private Camera _camera;
    private int _loadLevel = 1;
    private Sequence _showHandSeq;

    [Space(10)] [Inject] private UIGameScreen _uIGameScreen;

    public static DebugPanel Instance { get; private set; }

    public static bool EnableGameplayUI { get; private set; } = true;

    private void Awake()
    {
        if (!Gamestoty.debugDevice)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _openButton.onClick.AddListener(() =>
        {
            _panel.SetActive(!_panel.activeSelf);
        });

        _controlToggle.isOn = isAlternativeControl;
        _controlToggle.onValueChanged.AddListener(value => { isAlternativeControl = value; });

        _controlXToggle.isOn = controlLockX;
        _controlYToggle.isOn = controlLockY;

        _moveOverrideToggle.isOn = isMoveOverride;
        _moveOverrideToggle.onValueChanged.AddListener(value =>
        {
            isMoveOverride = value;

            Debug.Log(_moveCountText.text);
            if (isMoveOverride)
            {
                moveCount = int.Parse(_moveCountText.text, NumberStyles.Integer);
            }
        });

        _controlXToggle.onValueChanged.AddListener(value => { controlLockX = value; });
        _controlYToggle.onValueChanged.AddListener(value => { controlLockY = value; });

        _eyesEnabled.isOn = isEyeHandleEnabled;
        _eyesLookAtCursor.isOn = isEyeLookAtCursor;

        _eyesEnabled.onValueChanged.AddListener(value => { isEyeHandleEnabled = value; });
        _eyesLookAtCursor.onValueChanged.AddListener(value => { isEyeLookAtCursor = value; });

        _loadLevelSlider.wholeNumbers = true;
        _loadLevelSlider.minValue = 1;
        _loadLevelSlider.maxValue = GameConfig.RemoteConfig.LevelsFirstQueue.Length +
                                    GameConfig.RemoteConfig.LevelsSecondQueue.Length;
        _loadLevelSlider.onValueChanged.AddListener(value =>
        {
            _loadLevelText.text = $"Load: {(int)value}";
        });

        _loadLevelInputField.text = _loadLevel.ToString();
        _loadLevelInputField.onValueChanged.AddListener(value =>
        {
            if (int.TryParse(value, out _loadLevel))
            {
                _loadLevelText.text = $"Load: {_loadLevel}";
            }
        });

        _loadLevelButton.onClick.AddListener(() =>
        {
            LevelsController.Instance.ForceLoadLevel(_loadLevel - 1);
        });

        _loadPrevLevelButton.onClick.AddListener(() =>
        {
            LevelsController.Instance.ForceLoadLevel(LevelsController.Instance.CurrentLevelNumber - 1);
        });

        _loadNextLevelButton.onClick.AddListener(() =>
        {
            LevelsController.Instance.ForceLoadLevel(LevelsController.Instance.CurrentLevelNumber + 1);
        });

        _add3KeysButton.onClick.AddListener(() =>
        {
            GlobalData.Instance.GetGameData().KeysAmount.SetValue(3);
        });

        _addSoftCcyButton.onClick.AddListener(() =>
        {
            _softCcyVariable.ApplyChange(999999);
        });

        var mateials = Enum.GetNames(typeof(PinMaterialId)).ToList();

        _materialSelect.options.Add(new TMP_Dropdown.OptionData("default"));

        mateials.ForEach(a => _materialSelect.options.Add(new TMP_Dropdown.OptionData(a)));

        _materialSelect.onValueChanged.AddListener(selectedIndex =>
        {
            if (selectedIndex == 0)
            {
                ColorHolder._isMaterialOverride = false;
            }
            else
            {
                ColorHolder._isMaterialOverride = true;
                ColorHolder._overridedMaterial = (PinMaterialId)selectedIndex - 1;
            }

            LevelsController.Instance.RestartLevel(ELevelStartType.RESTART);
        });

        _statsPanel.gameObject.SetActive(_enableFps);
        _fpsToggle.isOn = _enableFps;
        _fpsToggle.onValueChanged.AddListener(value =>
        {
            _enableFps = value;
            _statsPanel.gameObject.SetActive(_enableFps);
        });

        _uiToggle.isOn = EnableGameplayUI;
        _uiToggle.onValueChanged.AddListener(value =>
        {
            EnableGameplayUI = value;
            _uIGameScreen.UpdateVisual();
        });

        SetupSkinSlider();
    }

    private void Start()
    {
        StartCoroutine(Co_FPS());

        UpdateLoadLevelSlider();

        _configText.text = HyperKit.Data.CurrentConfigVersion;
        _camera = CameraManager.Instance.GetCameraItem(ECameraType.UI).Camera;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void SetupSkinSlider()
    {
    }

    public void UpdateLoadLevelSlider()
    {
        _loadLevelSlider.value = LevelsController.Instance.CurrentLevelNumber + 1;
    }

    public void UpdateLevelName(string name)
    {
        _levelNameText.text = name;
    }

    public void ShowFindSolutionTime(Stopwatch sw, bool hasSolution)
    {
        if (hasSolution)
        {
            _solutionTimeText.text = $"solution found:\n{sw.Elapsed.TotalSeconds} sec.";
            _solutionTimeText.color = new Color(0f, 0.7f, 0f);
        }
        else
        {
            _solutionTimeText.text = $"solution NOT found:\n{sw.Elapsed.TotalSeconds} sec.";
            _solutionTimeText.color = Color.red;
        }
    }

    public void HideSolutionTime()
    {
        _solutionTimeText.text = "";
    }

    public void ShowFindSolutionInProcess()
    {
        _solutionTimeText.color = _fpsText.color;
        _solutionTimeText.text = "finding a solution in the process: 0.0 sec.";
    }

    public void ShowFindSolutionInProcess(float sec)
    {
        _solutionTimeText.text = $"finding a solution in the process: {sec:0.0} sec.";
    }

    private IEnumerator Co_FPS()
    {
        var en = new CultureInfo("en-US");

        void UpdateText(float min, float avg, float max, float current)
        {
            _fpsText.text =
                $"Min: {min.ToString("F1", en)}\n" +
                $"Avg: {avg.ToString("F1", en)}\n" +
                $"Max: {max.ToString("F1", en)}\n" +
                $"Current: {current.ToString("F1", en)}";
        }

        UpdateText(0f, 0f, 0f, 0f);
        while (true)
        {
            var frames = 0;
            var time = 0f;
            var minFps = float.MaxValue;
            var maxFps = float.MinValue;
            var current = 0f;
            while (time < 1f)
            {
                yield return null;
                var deltaTime = Time.unscaledDeltaTime;
                frames++;
                time += deltaTime;
                var fps = 1f / deltaTime;
                minFps = Mathf.Min(minFps, fps);
                maxFps = Mathf.Max(maxFps, fps);
                current = fps;
            }

            UpdateText(minFps, frames / time, maxFps, current);
        }
    }
}
