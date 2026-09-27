using System.Linq;
using HarmonyLib;
using PeakAchiever.Tracking;

namespace PeakAchiever.Inventory;

/// <summary>Marks inventory slots holding items a pinned clean-run badge forbids.</summary>
[HarmonyPatch]
internal static class InventoryPatches
{
    // GUIManager fills the three hotbar slots, the backpack slot and the temporary slot through this.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(InventoryItemUI), nameof(InventoryItemUI.SetItem))]
    private static void MarkHotbarSlot(InventoryItemUI __instance, ItemSlot slot) =>
        InventoryMark.Show(__instance.transform, HotbarMark(__instance, slot, Plugin.Tracker.ForbiddenItems));

    // The opened backpack wheel fills each item slice through this (BackpackWheelSlice.cs).
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BackpackWheelSlice), nameof(BackpackWheelSlice.InitItemSlot))]
    private static void MarkBackpackSlice(BackpackWheelSlice __instance) =>
        InventoryMark.Show(
            __instance.transform,
            IsForbidden(__instance.itemSlot, Plugin.Tracker.ForbiddenItems) ? InventoryMarkKind.Forbidden : null
        );

    private static InventoryMarkKind? HotbarMark(InventoryItemUI slotUi, ItemSlot? slot, ItemTraits forbidden)
    {
        if (forbidden == ItemTraits.None || slot == null || slot.IsEmpty())
            return null;
        if (!slotUi.isBackpack)
            return IsForbidden(slot, forbidden) ? InventoryMarkKind.Forbidden : null;
        bool holdsForbidden =
            slot.data != null
            && slot.data.TryGetDataEntry(DataEntryKey.BackpackData, out BackpackData contents)
            && contents.itemSlots.Any(inside => IsForbidden(inside, forbidden));
        return holdsForbidden ? InventoryMarkKind.HoldsForbidden : null;
    }

    private static bool IsForbidden(ItemSlot? slot, ItemTraits forbidden) =>
        slot != null && !slot.IsEmpty() && (ItemTraitReader.Of(slot.prefab) & forbidden) != ItemTraits.None;
}
