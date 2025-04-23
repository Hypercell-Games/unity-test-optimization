using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LoaderScreen : MonoBehaviour
{
    private static LoaderScreen _instance;
    [SerializeField] private GameObject _canvasGameObject;
    [SerializeField] private Image _fillImage;
    private Sequence _showSeq;

    public static LoaderScreen Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LoaderScreen>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null)
        {
            if (_instance != this)
            {
                Destroy(gameObject);
            }

            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        _canvasGameObject.SetActive(false);
    }

    public void Hide()
    {
        StartCoroutine(Co_Hide());
        _showSeq?.Kill();
    }

    private IEnumerator Co_Hide()
    {
        yield return null;
        _canvasGameObject.SetActive(false);
    }

    public void Show()
    {
        _canvasGameObject.SetActive(true);
        _fillImage.rectTransform.anchorMax = new Vector2(0.06f, 1f);
        _showSeq?.Kill();
    }

    public void SetProgress(float progress)
    {
        progress = Mathf.InverseLerp(0f, 0.9f, progress);
        progress = Mathf.Lerp(0.06f, 1f, progress);
        _fillImage.rectTransform.anchorMax = _fillImage.rectTransform.anchorMax.SetX(progress);
    }
}
