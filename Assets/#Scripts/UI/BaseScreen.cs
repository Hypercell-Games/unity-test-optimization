using System;
using DG.Tweening;
using UnityEngine;

public class BaseScreen : MonoBehaviour, IScreen
{
    [SerializeField] protected GameObject _canvasObject;
    [SerializeField] protected CanvasGroup _canvasGroup;

    [Header("Settings")] [SerializeField] protected float _hideDuration = 0.5f;

    [SerializeField] protected float _showDuration = 0.5f;
    public event Action onBeforeShow;
    public event Action onAfterShow;
    public event Action onBeforeHide;
    public event Action onAfterHide;

    public virtual void Hide(bool force = false, Action callback = null)
    {
        onBeforeHide?.Invoke();

        if (force)
        {
            _canvasGroup.alpha = 0f;
            OnHide();
        }
        else
        {
            _canvasGroup.DOFade(0f, _hideDuration).OnComplete(OnHide).SetLink(_canvasGroup.gameObject);
        }

        void OnHide()
        {
            _canvasObject.SetActive(false);
            onAfterHide?.Invoke();
            callback?.Invoke();
        }
    }

    public virtual void Show(bool force = false, Action callback = null)
    {
        onBeforeShow?.Invoke();

        _canvasObject.SetActive(true);

        if (force)
        {
            _canvasGroup.alpha = 1f;
            OnShow();
        }
        else
        {
            _canvasGroup.DOFade(1f, _showDuration).OnComplete(OnShow).SetLink(_canvasGroup.gameObject);
            ;
        }

        void OnShow()
        {
            onAfterShow?.Invoke();
            callback?.Invoke();
        }
    }
}
