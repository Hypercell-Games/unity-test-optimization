using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadeScreen : MonoBehaviour
{
    private static FadeScreen _instance;

    [SerializeField] private CanvasGroup _overlay;
    [SerializeField] private Image _iconImage;
    private Action _hideCallback;

    private bool _shown;

    private Sequence _showSeq;

    public static FadeScreen Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<FadeScreen>();
            }

            return _instance;
        }
        set => _instance = value;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void Show(Action hideCallback)
    {
        if (_shown)
        {
            return;
        }

        _shown = true;
        _hideCallback = hideCallback;
        _overlay.blocksRaycasts = true;
        var adsFadeIn = GameConfig.RemoteConfig.adsFadeInDuration;
        var adsFadeOut = GameConfig.RemoteConfig.adsFadeOutDuration;
        _iconImage.gameObject.SetActive(GameConfig.RemoteConfig.adsFadeIcon);
        var enableFade = adsFadeIn > 0f || adsFadeOut > 0f;

        void OnEnableScreen()
        {
            DOTween.Sequence()
                .SetLink(gameObject)
                .InsertCallback(GameConfig.RemoteConfig.adsFadeScreenCloseModeDelay, () =>
                {
                    Hide();
                });
        }

        if (adsFadeIn <= 0f)
        {
            _overlay.alpha = enableFade ? 1f : 0f;
            OnEnableScreen();
            return;
        }

        _showSeq?.Kill();
        _showSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(_overlay.DOFade(1f, adsFadeIn).SetEase(Ease.Linear))
            .OnComplete(() =>
            {
                OnEnableScreen();
            });
    }

    public void Hide(Action callback = null)
    {
        if (!_shown)
        {
            return;
        }

        _shown = false;
        var hideCallback = _hideCallback;
        _hideCallback = null;

        var adsFadeOut = GameConfig.RemoteConfig.adsFadeOutDuration;

        if (adsFadeOut <= 0f)
        {
            _showSeq?.Kill();
            _showSeq = DOTween.Sequence()
                .SetLink(gameObject)
                .AppendInterval(0.1f)
                .AppendCallback(() =>
                {
                    hideCallback?.Invoke();
                    _overlay.alpha = 0f;
                    callback?.Invoke();
                    _overlay.blocksRaycasts = false;
                });
            return;
        }

        _showSeq?.Kill();
        _showSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .AppendInterval(0.1f)
            .AppendCallback(() =>
            {
                hideCallback?.Invoke();
            })
            .Append(_overlay.DOFade(0f, adsFadeOut).SetEase(Ease.Linear))
            .OnComplete(() =>
            {
                callback?.Invoke();
                _overlay.blocksRaycasts = false;
            });
    }
}
