using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TapAwayDebugUI : MonoBehaviour
{
    private static bool _enableCreativeHand;
    private static int _creativeHandImageInd;
    private static float _creativeHandAlpha;
    private static float _creativeHandScale = 1f;

    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _openBtn;
    [SerializeField] private Button _prevBtn;
    [SerializeField] private Button _nextBtn;
    [SerializeField] private TMP_Text _touchesText;
    [SerializeField] private Button _autoPlayBtn;
    [SerializeField] private Toggle _tapEffectToggle;
    [SerializeField] private Slider _tapEffectColorSlider;
    [SerializeField] private BlockColorsScheme _blockColorsScheme;

    [Header("Hand")] [SerializeField] private Toggle _creativeHandToggle;

    [SerializeField] private Slider _creativeHandImageSlider;
    [SerializeField] private Slider _creativeHandAlphaSlider;
    [SerializeField] private Slider _creativeHandScaleSlider;
    [SerializeField] private RectTransform _creativeHandTransform;
    [SerializeField] private RectTransform _creativeHandScaleTransform;
    [SerializeField] private CanvasGroup _creativeHandImageCanvasGroup;
    [SerializeField] private Image[] _creativeHandImages;
    private Camera _camera;
    private Sequence _showHandSeq;
    public static TapAwayDebugUI Instance { get; private set; }
    public static bool TapEffect { get; private set; }
    public static int TapEffectColorInd { get; private set; }

    private void Awake()
    {
        Instance = this;
        _openBtn.onClick.AddListener(() =>
        {
            _panel.SetActive(!_panel.activeSelf);
        });
        _prevBtn.onClick.AddListener(() =>
        {
            FindObjectOfType<Map>()?.LoadPrev();
        });
        _nextBtn.onClick.AddListener(() =>
        {
            FindObjectOfType<Map>()?.LoadNext();
        });
        _autoPlayBtn.onClick.AddListener(() =>
        {
            FindObjectOfType<LevelTapAway>()?.AutoPlay();
            _panel.SetActive(false);
        });

        _creativeHandToggle.isOn = _enableCreativeHand;
        _creativeHandToggle.onValueChanged.AddListener(value =>
        {
            _enableCreativeHand = value;
            _creativeHandTransform.gameObject.SetActive(value);
        });
        _creativeHandTransform.gameObject.SetActive(_enableCreativeHand);

        _creativeHandImageSlider.value = _creativeHandImageInd;
        _creativeHandImageSlider.minValue = 0;
        _creativeHandImageSlider.maxValue = _creativeHandImages.Length - 1;
        ChangeHandImage(_creativeHandImageInd);

        void ChangeHandImageF(float ind)
        {
            ChangeHandImage(Mathf.RoundToInt(ind));
        }

        void ChangeHandImage(int ind)
        {
            _creativeHandImageInd = ind;
            _creativeHandImages.For((image, i) => image.gameObject.SetActive(i == _creativeHandImageInd));
        }

        _creativeHandImageSlider.onValueChanged.AddListener(ChangeHandImageF);

        _creativeHandAlphaSlider.value = _creativeHandAlpha;
        _creativeHandAlphaSlider.onValueChanged.AddListener(value =>
        {
            _creativeHandAlpha = value;
            _creativeHandImageCanvasGroup.alpha = value;
        });
        _creativeHandTransform.localScale = Vector3.one * 1.2f;

        _creativeHandScaleSlider.minValue = 0.1f;
        _creativeHandScaleSlider.maxValue = 1;
        _creativeHandScaleSlider.value = _creativeHandScale;
        _creativeHandScaleSlider.onValueChanged.AddListener(value =>
        {
            _creativeHandScale = value;
            _creativeHandScaleTransform.localScale = Vector3.one * _creativeHandScale;
        });
        _creativeHandScaleTransform.localScale = Vector3.one * _creativeHandScale;

        _tapEffectToggle.isOn = TapEffect;
        _tapEffectToggle.onValueChanged.AddListener(value =>
        {
            TapEffect = value;
        });

        _tapEffectColorSlider.minValue = 0;
        _tapEffectColorSlider.maxValue = _blockColorsScheme.GetTapMaterials().Count - 1;
        _tapEffectColorSlider.wholeNumbers = true;
        _tapEffectColorSlider.value = TapEffectColorInd;
        _tapEffectColorSlider.onValueChanged.AddListener(value =>
        {
            TapEffectColorInd = Mathf.RoundToInt(value);
        });

        if (!Gamestoty.debugDevice)
        {
            gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        _camera = CameraManager.Instance.GetCameraItem(ECameraType.UI).Camera;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _showHandSeq?.Kill();
            _showHandSeq = DOTween.Sequence()
                .SetLink(gameObject)
                .Append(_creativeHandImageCanvasGroup.DOFade(1f, 0.1f))
                .Join(_creativeHandTransform.DOScale(1f, 0.1f));
        }

        if (Input.GetMouseButtonUp(0))
        {
            _showHandSeq?.Kill();
            _showHandSeq = DOTween.Sequence()
                .SetLink(gameObject)
                .Append(_creativeHandImageCanvasGroup.DOFade(_creativeHandAlpha, 0.2f))
                .Join(_creativeHandTransform.DOScale(1.2f, 0.2f))
                .OnComplete(() => _creativeHandImageCanvasGroup.alpha = _creativeHandAlpha);
        }

        RectTransformUtility.ScreenPointToWorldPointInRectangle(_rectTransform, Input.mousePosition, _camera,
            out var worldPos);
        _creativeHandTransform.position = worldPos;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void UdateTouches(int count)
    {
        _touchesText.text = count.ToString();
    }
}
