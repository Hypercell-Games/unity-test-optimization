using System.Collections.Generic;
using UnityEngine;

public class ShopSlotsRowLayout : MonoBehaviour
{
    public List<ShopSlotWidget> slots = new();

    public int SlotCount => slots.Count;

    public ShopSlotWidget GetSlot(int index)
    {
        return slots[index];
    }
}
