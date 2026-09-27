using PeakAchiever.Tracking;

namespace PeakAchiever.Inventory;

/// <summary>Reads from an item prefab what using it would do, in the terms the badge rules forbid.</summary>
internal static class ItemTraitReader
{
    public static ItemTraits Of(Item item)
    {
        ItemTraits traits = ItemTraits.None;
        // AchievementManager.TestItemConsumed counts an eaten item as packaged food by this tag.
        if (item.itemTags.HasFlag(Item.ItemTags.PackagedFood))
            traits |= ItemTraits.PackagedFood;
        // The item components whose use calls GameUtils.IncrementPermanentItemsPlaced (game v2.4.c):
        // pitons (CharacterItems), constructables, rope spools, the rope cannon and the chain launcher.
        if (
            item.TryGetComponent(out ClimbingSpikeComponent _)
            || item.TryGetComponent(out Constructable _)
            || item.TryGetComponent(out RopeTier _)
            || item.TryGetComponent(out RopeShooter _)
            || item.TryGetComponent(out VineShooter _)
        )
            traits |= ItemTraits.PlacesPermanentObject;
        return traits;
    }
}
