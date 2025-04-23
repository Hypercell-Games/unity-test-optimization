using UnityEngine;

public static class BoardElementUtils
{
    public static void ChangeComponentsSortOrder(Component component, int sortOrder)
    {
        if (component == null)
        {
            return;
        }

        void SortOrderInternal(Transform t)
        {
            if (t.TryGetComponent<ParticleSystemRenderer>(out var particle))
            {
                particle.sortingOrder = sortOrder;
            }

            if (t.TryGetComponent<TrailRenderer>(out var trail))
            {
                trail.sortingOrder = sortOrder;
            }

            for (var i = 0; i < t.childCount; i++)
            {
                SortOrderInternal(t.GetChild(i));
            }
        }

        SortOrderInternal(component.transform);
    }
}
