using System.Collections.Generic;

namespace PeakAchiever.Tracking;

/// <summary>How the items a badge lists are needed.</summary>
internal enum NeedKind
{
    /// <summary>Every item listed, in the quantity given.</summary>
    All,

    /// <summary>Any one of the items listed.</summary>
    OneOf,
}

/// <param name="ItemName">The item's Item.UIData.itemName, compared ignoring case (v2.6.b).</param>
/// <param name="Count">How many the badge takes at once; 1 shows no count.</param>
internal readonly record struct NeededItem(string ItemName, int Count = 1);

/// <param name="Helps">Items that make the badge easier without being needed.</param>
internal sealed record BadgeNeed(NeedKind Kind, IReadOnlyList<NeededItem> Items, IReadOnlyList<NeededItem> Helps);

/// <summary>
/// The items each badge needs or is helped by, shown on its card. From the PEAK wiki
/// (https://peak.wiki.gg/wiki/Badges, read 2026-10-08), except where the game's code says otherwise.
/// </summary>
internal static class BadgeNeeds
{
    private static BadgeNeed All(params NeededItem[] items) => new(NeedKind.All, items, []);

    private static BadgeNeed OneOf(params NeededItem[] items) => new(NeedKind.OneOf, items, []);

    private static BadgeNeed HelpedBy(params NeededItem[] items) => new(NeedKind.All, [], items);

    private static NeededItem I(string itemName, int count = 1) => new(itemName, count);

    public static readonly IReadOnlyDictionary<ACHIEVEMENTTYPE, BadgeNeed> Needs = new Dictionary<ACHIEVEMENTTYPE, BadgeNeed>
    {
        // Lava.ValidIdolSacrifice takes only an item tagged GoldenIdol in the Kiln, and the Ancient Idol is the
        // one item with that tag (v2.6.b); the wiki also lists four items that do not count.
        [ACHIEVEMENTTYPE.TwentyFourKaratBadge] = All(I("Ancient Idol")),
        // CharacterBalloons grants it at 6 balloons held while off the ground (v2.6.b); a bunch holds 3.
        [ACHIEVEMENTTYPE.AeronauticsBadge] = OneOf(I("Balloon", 6), I("Balloon Bunch", 2)),
        [ACHIEVEMENTTYPE.AnimalSerenadingBadge] = OneOf(I("Bugle"), I("Bugle of Friendship"), I("Scoutmaster's Bugle")),
        [ACHIEVEMENTTYPE.AppliedEsotericaBadge] = All(I("THEBOOKOFBONES")),
        [ACHIEVEMENTTYPE.ArboristBadge] = HelpedBy(I("anti-rope spool"), I("rope cannon")),
        [ACHIEVEMENTTYPE.ArcheryBadge] = HelpedBy(I("Fortified Milk"), I("AMULET_HEALING"), I("Ancient Idol"), I("Bandages"), I("First Aid Kit")),
        [ACHIEVEMENTTYPE.AstronomyBadge] = All(I("Binoculars")),
        [ACHIEVEMENTTYPE.BingBongBadge] = All(I("Bing Bong")),
        [ACHIEVEMENTTYPE.BoulderingBadge] = All(I("Piton")),
        [ACHIEVEMENTTYPE.CalciumIntakeBadge] = All(I("Fortified Milk")),
        [ACHIEVEMENTTYPE.CompetitiveEatingBadge] = All(I("Hot Dog", 3)),
        [ACHIEVEMENTTYPE.CryptogastronomyBadge] = All(I("Mandrake")),
        [ACHIEVEMENTTYPE.DaredevilBadge] = All(I("Scout Cannon")),
        [ACHIEVEMENTTYPE.DisasterResponseBadge] = All(I("Rescue Claw")),
        [ACHIEVEMENTTYPE.EmergencyPreparednessBadge] = OneOf(I("Bandages"), I("First Aid Kit")),
        [ACHIEVEMENTTYPE.ExorcistBadge] = OneOf(I("Candlestick"), I("Faerie Lantern"), I("Portable Stove")),
        [ACHIEVEMENTTYPE.FirstAidBadge] = OneOf(I("Bandages"), I("First Aid Kit")),
        [ACHIEVEMENTTYPE.HangGlidingBadge] = All(I("Glider")),
        [ACHIEVEMENTTYPE.KnotTyingBadge] = OneOf(I("rope spool"), I("anti-rope spool"), I("rope cannon"), I("anti-rope cannon")),
        [ACHIEVEMENTTYPE.LastResortBadge] = All(I("RitualDagger")) with { Helps = [I("Blowgun")] },
        [ACHIEVEMENTTYPE.MegaentomologyBadge] = HelpedBy(I("Fortified Milk"), I("AMULET_HEALING"), I("anti-rope cannon"), I("VOIDLAUNCHER")),
        [ACHIEVEMENTTYPE.MycoacrobaticsBadge] = All(I("Bounce Fungus")) with { Helps = [I("Balloon"), I("Rescue Claw")] },
        [ACHIEVEMENTTYPE.NeedlepointBadge] = All(I("Cactus", 5)),
        [ACHIEVEMENTTYPE.ResourcefulnessBadge] = OneOf(I("Bird")),
        [ACHIEVEMENTTYPE.RuleZeroBadge] = All(
            I("AMULET_DOUBLEJUMP"),
            I("AMULET_HEALING"),
            I("AMULET_CLONE"),
            I("AMULET_INFINITESTAM"),
            I("Strange Gem"),
            I("ScoutsHonor")
        ),
        [ACHIEVEMENTTYPE.ToxicologyBadge] = OneOf(I("Antidote"), I("Cure-All"), I("First Aid Kit"), I("Medicinal Root")),
        [ACHIEVEMENTTYPE.UltimateBadge] = All(I("Frisbee")),
        [ACHIEVEMENTTYPE.WellRestedBadge] = All(I("EARLYWORM")),
    };

    public static BadgeNeed? For(ACHIEVEMENTTYPE badge) => Needs.TryGetValue(badge, out BadgeNeed need) ? need : null;
}
