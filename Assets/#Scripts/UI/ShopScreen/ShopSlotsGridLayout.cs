using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Unpuzzle;
using Random = UnityEngine.Random;

public class ShopSlotsGridLayout : MonoBehaviour
{
    public static readonly int SLOTS_PER_ROW = 3;
    public static readonly int SLOTS_ROWS = 3;
    public static readonly int SLOTS_PER_PAGE = SLOTS_PER_ROW * SLOTS_ROWS;

    [ReadOnly] [SerializeField] private List<ShopSlotsRowLayout> rows = new();

    public int RowCount => rows.Count;

    public int SlotCount => rows.Count * SLOTS_PER_ROW;

    private void OnValidate()
    {
        rows = GetComponentsInChildren<ShopSlotsRowLayout>().ToList();
    }

    public void SetupContents(List<BgSkinConfig> galleryItems, Action<BgSkinConfig> onItemRequested,
        Action<BgSkinConfig> onUnlockByAd)
    {
        for (var i = 0; i < galleryItems.Count; i++)
        {
            var slot = GetSlot(i);
            slot.SetContent(galleryItems[i]);
            slot.OnSelectionRequest += onItemRequested.Invoke;
            slot.OnGetByAdRequest += onUnlockByAd.Invoke;
        }
    }

    public void SetUseGrayScaleForDisabled(bool useGrayScale)
    {
        Array.ForEach(SlotsEnumerator().ToArray(), r => r.UseGrayScaleForDisabled = useGrayScale);
    }

    public ShopSlotWidget GetSlot(int index)
    {
        var rowIndex = index / SLOTS_PER_ROW;
        var slotInRowIndex = index - rowIndex * SLOTS_PER_ROW;

        return rows[rowIndex].GetSlot(slotInRowIndex);
    }

    public int CountLockedItems()
    {
        return SlotsEnumerator().Count(r => r.IsLocked());
    }

    private IEnumerable<ShopSlotWidget> SlotsEnumerator()
    {
        for (var i = 0; i < SlotCount; i++)
        {
            yield return GetSlot(i);
        }
    }

    public IEnumerator CO_UnlockRandomWithAnimation(BgSkinConfig preferredSkin, Action<BgSkinConfig> onResult = null)
    {
        var lockedSlots = SlotsEnumerator()
            .Where(r => r.IsLocked() && !r.IsHidden())
            .ToList();

        if (lockedSlots.Count == 0)
        {
            yield break;
        }

        var randomSteps = 18 + Random.Range(0, 5);
        var targetSlot = lockedSlots.Random();
        var preferredSlot = lockedSlots.FirstOrDefault(r => r.CurrentItem == preferredSkin);

        var maxWaitTime = 0.05f + 35 * 0.01f;

        if (lockedSlots.Count > 1)
        {
            for (var i = 0; i < randomSteps; i++)
            {
                var waitTime = Mathf.Lerp(0, maxWaitTime, i / (float)(randomSteps - 1));
                var lastStep = i + 1 == randomSteps;

                yield return new WaitForSecondsRealtime(waitTime);

                var newTarget = lockedSlots.Random();
                while (newTarget == targetSlot)
                {
                    newTarget = lockedSlots.Random();
                }

                if (preferredSlot != null && lastStep)
                {
                    newTarget = preferredSlot;
                }

                SetLockedSelected(targetSlot = newTarget);
            }
        }

        SetLockedSelected(null);

        targetSlot.UnlockItem();

        onResult?.Invoke(targetSlot.CurrentItem);
    }

    private void SetLockedSelected(ShopSlotWidget targetSlot)
    {
        foreach (var slot in SlotsEnumerator().Where(r => r.IsLocked()))
        {
            var isTarget = slot == targetSlot;

            slot.SetState(isTarget ? ShopSlotWidget.State.LockedActive : ShopSlotWidget.State.Locked);

            if (isTarget)
            {
                slot.ThrobSmall(0.5f);
            }
        }
    }

    public void SetSelected(BgSkinConfig item, bool skipAnim = false)
    {
        Array.ForEach(SlotsEnumerator().ToArray(), r => r.SetSelected(r.CurrentItem == item, skipAnim));
    }

    public bool HasItem(BgSkinConfig item)
    {
        return SlotsEnumerator().Any(r => r.CurrentItem == item);
    }

    public void RefreshStates()
    {
        Array.ForEach(SlotsEnumerator().ToArray(), r => r.UpdateState());
    }

    public bool HasSelected()
    {
        return SlotsEnumerator().FirstOrDefault(r => r.IsSelected) ?? false;
    }
}
