using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class UITapAwayTutorial : MonoBehaviour
{
    private static UITapAwayTutorial _instance;

    [SerializeField] private RectTransform _tutorialHandTransform;
    [SerializeField] private Image _tutorialHandImage;

    [Header("Drag")] [SerializeField] private GameObject _dragContent;

    [SerializeField] private RectTransform _dragHandPoint;
    [SerializeField] private RectTransform _dragHandPointBorder;
    [SerializeField] private TMP_Text _dragTutorialText;
    [SerializeField] private TMP_Text _dragTutorialText1;
    [SerializeField] private UILineRenderer _dragLineRenderer;
    [SerializeField] private UILineRenderer _dragLineRenderer1;

    private Block _tapBlock;
    private bool _tapTutorShow;
    private Sequence _tutorDragTextSeq;

    private Sequence _tutorHandSeq;

    public static UITapAwayTutorial Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UITapAwayTutorial>();
            }

            return _instance;
        }
        private set => _instance = value;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void ShowDragTutor()
    {
        _dragContent.SetActive(true);
        _tutorialHandTransform.gameObject.SetActive(true);

        var pivot = _dragHandPoint.position;
        var width = _dragHandPointBorder.position.x - pivot.x;
        var height = _dragHandPointBorder.position.y - pivot.y;

        _tutorialHandTransform.localScale = Vector3.one;
        _tutorialHandTransform.position = pivot;

        _tutorHandSeq?.Kill();
        _tutorHandSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .SetLoops(-1)
            .Append(_tutorialHandTransform.DOScale(0.8f, 0.3f))
            .Append(DOTween.To(() => 0f, t =>
            {
                var angle = t * Mathf.PI * 2f;
                var x = -Mathf.Sin(angle);
                var y = Mathf.Sin(angle * 2f);
                _tutorialHandTransform.position = pivot + new Vector3(x * width, y * height, 0f);
            }, 1f, 1f + 1.7f).SetEase(Ease.InOutSine))
            .Append(_tutorialHandTransform.DOScale(1f, 0.4f));

        _dragTutorialText.transform.localScale = Vector3.one;
        _dragTutorialText1.transform.localScale = Vector3.one;
        _tutorDragTextSeq?.Kill();
        _tutorDragTextSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .InsertCallback(0.3f + 0.7f, () =>
            {
                _tutorDragTextSeq = DOTween.Sequence()
                    .SetLink(gameObject)
                    .SetLoops(-1)
                    .Append(_dragTutorialText.rectTransform.DOScale(new Vector3(0.9f, 1.1f, 1f), 0.3f))
                    .Append(_dragTutorialText.rectTransform.DOScale(new Vector3(1.1f, 0.9f, 1f), 0.3f))
                    .Append(_dragTutorialText.rectTransform.DOScale(new Vector3(0.9f, 1.1f, 1f), 0.3f))
                    .Append(_dragTutorialText.rectTransform.DOScale(new Vector3(1.1f, 0.9f, 1f), 0.3f))
                    .Append(_dragTutorialText.rectTransform.DOScale(Vector3.one, 0.5f + 1.7f).SetEase(Ease.OutExpo))
                    .OnUpdate(() =>
                        _dragTutorialText1.rectTransform.localScale = _dragTutorialText.rectTransform.localScale);
            });
    }

    public void HideTutorial()
    {
        _tutorHandSeq?.Kill();
        _tutorDragTextSeq?.Kill();
        _dragContent.SetActive(false);
        _tutorialHandTransform.gameObject.SetActive(false);
        _tapTutorShow = false;
    }

    public void ShowTapTutor(Block block)
    {
        _tapBlock = block;
        if (_tapTutorShow)
        {
            return;
        }

        _tapTutorShow = true;
        _tutorialHandTransform.gameObject.SetActive(true);
        var cameraGAME = CameraManager.Instance.GetCameraItem(ECameraType.GAME).Camera;
        var cameraUI = CameraManager.Instance.GetCameraItem(ECameraType.UI).Camera;
        const float handScale = 0.7f;
        _tutorialHandTransform.localScale = Vector3.one * handScale;
        _tutorHandSeq?.Kill();
        _tutorHandSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .SetLoops(-1)
            .Append(_tutorialHandTransform.DOScale(handScale * 0.8f, 0.2f))
            .Append(_tutorialHandTransform.DOScale(handScale, 0.2f))
            .Append(_tutorialHandTransform.DOScale(handScale * 0.8f, 0.2f))
            .Append(_tutorialHandTransform.DOScale(handScale, 0.8f).SetEase(Ease.OutExpo))
            .OnUpdate(() =>
            {
                if (_tapBlock)
                {
                    var pos = RectTransformUtility.WorldToScreenPoint(cameraGAME, _tapBlock.transform.position);
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            _tutorialHandTransform.parent as RectTransform, pos, cameraUI, out var localPoint))
                    {
                        _tutorialHandTransform.localPosition = localPoint;
                    }
                }
            });
    }
}
