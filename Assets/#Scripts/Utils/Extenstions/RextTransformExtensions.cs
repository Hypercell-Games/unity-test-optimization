using UnityEngine;

namespace Utils.Extensions
{
    public static class RextTransformExtensions
    {
        public static void SetPivot(this RectTransform rectTransform, Vector2 pivot)
        {
            Vector3 deltaPosition = rectTransform.pivot - pivot;
            deltaPosition.Scale(rectTransform.rect.size);
            deltaPosition.Scale(rectTransform.localScale);
            deltaPosition = rectTransform.rotation * deltaPosition;

            rectTransform.pivot = pivot;
            rectTransform.localPosition -= deltaPosition;
        }
    }
}
