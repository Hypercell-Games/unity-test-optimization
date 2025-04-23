using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class KeysLayoutController : MonoBehaviour
{
    [SerializeField] private IntVariable keysVariable;

    [SerializeField] private List<KeyController> keys;

    [SerializeField] private Image keyCollectPrefab;

    [SerializeField] private CanvasGroup _canvasGroup;

    private void Start()
    {
        _canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        keysVariable.onChange.action += SetFilledCount;
        SetFilledCount(keysVariable.Value, true);
    }

    private void OnDisable()
    {
        keysVariable.onChange.action -= SetFilledCount;
    }

    private void SetFilledCount(int count)
    {
        SetFilledCount(count, false);
    }

    public void Show()
    {
        _canvasGroup.alpha = 1f;
    }

    public void Hide()
    {
        _canvasGroup.DOFade(0f, 1f)
            .SetDelay(0.5f);
    }

    public void SetFilledCount(int count, bool instant)
    {
        for (var i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            var wasFilled = key.IsFilled;
            var shouldFill = i < count;

            key.SetFilled(shouldFill);

            if (!instant && !wasFilled && shouldFill)
            {
                DOTween.Sequence()
                    .Append(key.transform.DOScale(0.9f, 0.05f).SetEase(Ease.OutSine))
                    .Append(key.transform.DOScale(1f, 0.2f).SetEase(Ease.InOutSine));
            }
        }
    }

    public KeyController GetNextToFill()
    {
        return keys[Mathf.Min(keys.Count - 1, keysVariable.Value)];
    }

    public YieldInstruction TweenCollectKeyFrom(Transform key)
    {
        var target = GetNextToFill();

        var keyPosition = CameraManager.Instance.GetCameraItem(ECameraType.GAME).Camera
            .WorldToScreenPoint(key.position);

        keyPosition = CameraManager.Instance.GetCameraItem(ECameraType.UI).Camera.ScreenToWorldPoint(keyPosition);

        var img = Instantiate(keyCollectPrefab, keyPosition, Quaternion.identity);
        var imgTransform = img.transform;
        imgTransform.localEulerAngles = new Vector3(0, 0, 80);
        imgTransform.SetParent(target.transform, true);
        imgTransform.localScale = Vector3.one;

        var scale = 1f;

        key.transform.DOScale(0f, 0.3f)
            .OnComplete(() => { Destroy(key.gameObject); });

        var sequence = DOTween.Sequence()
            .Append(DOTween.Sequence()
                .AppendCallback(Show)
                .Append(imgTransform.DOScale(scale * 1.5f, 0.3f).SetEase(Ease.OutSine))
                .Join(imgTransform.DOShakeRotation(0.3f, Vector3.forward * 45f, 18))
            )
            .Join(img.DOColor(Color.white, 0.4f).SetEase(Ease.InOutSine))
            .Append(imgTransform
                .DOPath(GetKeyPath(imgTransform.position, target.transform.position), 0.7f, PathType.CatmullRom).From()
                .SetEase(Ease.InSine)
                .SetDelay(0.1f))
            .Join(imgTransform.DOScale(scale, 0.7f).SetEase(Ease.InCubic))
            .Join(imgTransform.DOLocalRotate(Vector3.zero, 0.7f).SetEase(Ease.InCubic))
            .AppendCallback(Hide)
            .OnComplete(() => Destroy(img.gameObject));

        return sequence.WaitForCompletion();
    }

    private Vector3[] GetKeyPath(Vector3 fromPos, Vector3 toPos)
    {
        var middlePoint = Vector3.Lerp(fromPos, toPos, 0.5f);
        middlePoint.x += -fromPos.x * 0.3f;

        return new[] { middlePoint, toPos };
    }
}
