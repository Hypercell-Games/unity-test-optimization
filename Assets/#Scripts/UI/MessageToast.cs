using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageToast : MonoBehaviour
{
    public enum ToastIcon
    {
        None,
        NoInternet
    }

    [Header("Configuration")] [SerializeField]
    private List<IconSpriteEntry> _iconSprites = new();

    [Header("Internal references")] [SerializeField]
    private Image _overlayImage;

    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _text;

    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _messagePanel;

    private Sequence _showSeq;
    public static MessageToast Instance { get; private set; }

    private void Awake()
    {
        if (Instance)
        {
            return;
        }

        Instance = this;
        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Hide()
    {
        _showSeq?.Kill();
        _showSeq = null;
        _canvasGroup.gameObject.SetActive(false);
    }

    public void Show(string text, bool autoHide = true, ToastIcon icon = ToastIcon.None, bool showOverlay = true)
    {
        _iconImage.gameObject.SetActive(icon != ToastIcon.None);
        _iconImage.sprite = GetIconSprite(icon);
        _iconImage.SetNativeSize();
        _overlayImage.gameObject.SetActive(showOverlay);

        _text.text = text;

        _showSeq?.Kill();
        _canvasGroup.gameObject.SetActive(true);
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = true;
        _messagePanel.localScale = Vector3.one * 0.5f;
        _showSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(_canvasGroup.DOFade(1f, 0.2f).SetEase(Ease.Linear))
            .Join(_messagePanel.DOScale(1f, 0.2f).SetEase(Ease.OutBack))
            .AppendInterval(0.5f)
            .OnComplete(() =>
            {
                if (autoHide)
                {
                    _showSeq = DOTween.Sequence()
                        .SetLink(gameObject)
                        .Append(_canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.Linear))
                        .Join(_messagePanel.DOScale(0.5f, 0.2f).SetEase(Ease.InBack))
                        .OnComplete(() =>
                        {
                            _canvasGroup.gameObject.SetActive(false);
                        });
                }
            });
    }

    private Sprite GetIconSprite(ToastIcon icon)
    {
        return _iconSprites.FirstOrDefault(r => r.icon == icon)?.sprite;
    }

    [Serializable]
    public class IconSpriteEntry
    {
        public ToastIcon icon;
        public Sprite sprite;
    }
}
