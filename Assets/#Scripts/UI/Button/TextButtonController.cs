using TMPro;
using UnityEngine;

public class TextButtonController : BaseButtonController
{
    [Header("Transform")] [SerializeField] private RectTransform _textTransform;

    [Header("Length")] [SerializeField] private float _yMovePosition = 100;

    [SerializeField] private TextMeshProUGUI _label;

    private Vector2 _startTextPosition;

    public CanvasGroup CanvasGroup => _canvasGroup;

    protected override void OnButtonPress()
    {
        base.OnButtonPress();

        _startTextPosition = _textTransform.localPosition;

        _textTransform.localPosition += Vector3.down * _yMovePosition;
    }

    protected override void OnButtonUnPress()
    {
        _textTransform.localPosition = _startTextPosition;

        base.OnButtonUnPress();
    }

    public void SetText(string text)
    {
        _label.SetText(text);
    }

    public TMP_Text GetText()
    {
        return _label;
    }
}
