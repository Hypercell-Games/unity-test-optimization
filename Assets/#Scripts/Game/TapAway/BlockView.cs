using DG.Tweening;
using UnityEngine;

public class BlockView : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;

    [SerializeField] private BlockColorsScheme _blockColorScheme;
    private BlockMaterials _blockMaterials;

    private Sequence _bounceSeq;
    private Sequence _colorSeq;
    private Vector3 _preBouncePos;

    private void Start()
    {
        _blockMaterials = new BlockMaterials(_meshRenderer.material);
    }

    public void SetColor(BlockMaterials bm)
    {
        _blockMaterials = bm;
        _meshRenderer.material = bm.Material;
    }

    public void Bounce(Vector3 dir)
    {
        StopBounce();
        var p0 = transform.localPosition;
        var p1 = transform.localPosition + dir * 0.1f;
        _preBouncePos = p0;
        const float duration0 = 0.07f;
        const float duration1 = 0.1f;
        _bounceSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(transform.DOLocalMove(p1, duration0).SetEase(Ease.OutQuad))
            .Append(transform.DOLocalMove(p0, duration1).SetEase(Ease.OutQuad))
            .OnKill(() => transform.localPosition = p0);
    }

    public void StopBounce()
    {
        if (_bounceSeq == null)
        {
            return;
        }

        _bounceSeq.Kill();
        _bounceSeq = null;
        transform.localPosition = _preBouncePos;
    }

    public void TapEffect()
    {
        if (!TapAwayDebugUI.TapEffect || _blockColorScheme.GetTapMaterials() == null ||
            _blockColorScheme.GetTapMaterials().Count == 0)
        {
            return;
        }

        var blockTapMaterials = _blockColorScheme.GetTapMaterials()[TapAwayDebugUI.TapEffectColorInd];
        var mat0 = _blockMaterials.Material;

        var _material0 = blockTapMaterials.Material;

        _meshRenderer.material = _material0;

        _colorSeq?.Kill();
        _colorSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .SetDelay(0.05f)
            .Append(DOTween.To(() => 0f, t =>
            {
                _meshRenderer.material.Lerp(_material0, mat0, t);
            }, 1f, 0.4f).SetEase(Ease.Linear));
    }
}
