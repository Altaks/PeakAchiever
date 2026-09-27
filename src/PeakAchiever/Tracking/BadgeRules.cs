using System.Collections.Generic;
using System.Linq;

namespace PeakAchiever.Tracking;

/// <summary>
/// The single table of how each badge is tracked. Every entry is traced to the game code that grants it
/// (decompiled Assembly-CSharp and scene data, game v2.4.c). A one-off with no blocker is a badge the code
/// shows can happen anywhere in the run: it shows as doable and never gets a red cross.
/// </summary>
internal static class BadgeRules
{
    // Thresholds the game hardcodes inline in AchievementManager.InitAchievementRequirementData.
    private const int RopeMetersForKnotTying = 100;
    private const int RevivesForClutch = 3;
    private const int LuggageForPlunderer = 15;
    private const int HealsForFirstAid = 20;
    private const int ClownLuggageForJester = 2;
    private const int ArrowsForArchery = 10;
    private const int MealsForCooking = 20;
    private const int MoraleBoostsForHappyCamper = 5;
    private const int PitonsForBouldering = 10;
    private const int PoisonHealedForToxicology = 200;
    private const int MetersForAscender = 5000;
    private const int MilkBlocksForCalciumIntake = 100;
    // AchievementManager.TestWonRun grants Gourmand at 4 distinct cooked dishes, with a bare literal.
    private const int DishesForGourmand = 4;

    internal static readonly Dictionary<ACHIEVEMENTTYPE, BadgeRule> Rules = new()
    {
        // Run counters (AchievementManager.runBasedAchievements).
        [ACHIEVEMENTTYPE.KnotTyingBadge] = BadgeRule.Counted(new RunValueTarget(RUNBASEDVALUETYPE.RopePlaced, RopeMetersForKnotTying)),
        [ACHIEVEMENTTYPE.ClutchBadge] = BadgeRule.Counted(new RunValueTarget(RUNBASEDVALUETYPE.ScoutsResurrected, RevivesForClutch)),
        [ACHIEVEMENTTYPE.PlundererBadge] = BadgeRule.Counted(new RunValueTarget(RUNBASEDVALUETYPE.LuggageOpened, LuggageForPlunderer)),
        [ACHIEVEMENTTYPE.FirstAidBadge] = BadgeRule.Counted(new RunValueTarget(RUNBASEDVALUETYPE.FriendsHealedAmount, HealsForFirstAid)),
        [ACHIEVEMENTTYPE.JesterBadge] = BadgeRule.Counted(new RunValueTarget(RUNBASEDVALUETYPE.ClownLuggageOpened, ClownLuggageForJester)),
        [ACHIEVEMENTTYPE.ArcheryBadge] = BadgeRule.Counted(new RunValueTarget(RUNBASEDVALUETYPE.ArrowsRemoved, ArrowsForArchery)),

        // Distinct items eaten this run (AchievementManager.AddTo*Eaten, TestWonRun).
        [ACHIEVEMENTTYPE.ForagingBadge] = BadgeRule.Counted(new RunCollectionTarget(RunCollection.DifferentBerriesEaten, AchievementManager.FRUITSNEEDEDFORACHIEVEMENT)),
        [ACHIEVEMENTTYPE.AdvancedMycologyBadge] = BadgeRule.Counted(new RunCollectionTarget(RunCollection.DifferentShroomBerriesEaten, AchievementManager.SHROOMBERRIESNEEDEDFORACHIEVEMENT)),
        [ACHIEVEMENTTYPE.MycologyBadge] = BadgeRule.Counted(new RunCollectionTarget(RunCollection.DifferentNonToxicMushroomsEaten, AchievementManager.MUSHROOMSNEEDEDFORACHIEVEMENT)),
        [ACHIEVEMENTTYPE.GourmandBadge] = BadgeRule.Counted(new RunCollectionTarget(RunCollection.GourmandDishesEaten, DishesForGourmand)),

        // Lifetime Steam stats (AchievementManager.steamStatBasedAchievements).
        [ACHIEVEMENTTYPE.CookingBadge] = BadgeRule.Counted(new LifetimeStatTarget(STEAMSTATTYPE.MealsCooked, MealsForCooking)),
        [ACHIEVEMENTTYPE.HappyCamperBadge] = BadgeRule.Counted(new LifetimeStatTarget(STEAMSTATTYPE.MoraleBoosts, MoraleBoostsForHappyCamper)),
        [ACHIEVEMENTTYPE.BoulderingBadge] = BadgeRule.Counted(new LifetimeStatTarget(STEAMSTATTYPE.PitonsPlaced, PitonsForBouldering)),
        [ACHIEVEMENTTYPE.ToxicologyBadge] = BadgeRule.Counted(new LifetimeStatTarget(STEAMSTATTYPE.PoisonHealed, PoisonHealedForToxicology)),
        [ACHIEVEMENTTYPE.AscenderBadge] = BadgeRule.Counted(new LifetimeStatTarget(STEAMSTATTYPE.HeightClimbed, MetersForAscender)),
        [ACHIEVEMENTTYPE.BookwormBadge] = BadgeRule.Counted(new LifetimeStatTarget(STEAMSTATTYPE.TotalPagesRead, AchievementManager.MAX_GUIDEBOOK_PAGES)),
        [ACHIEVEMENTTYPE.CalciumIntakeBadge] = BadgeRule.Counted(new LifetimeStatTarget(STEAMSTATTYPE.DamageBlockedByMilk, MilkBlocksForCalciumIntake)),

        // Clean runs checked at the summit (AchievementManager.TestWonRun, TestTimeAchievements).
        [ACHIEVEMENTTYPE.BalloonBadge] = BadgeRule.CleanRun(new RunValueCeiling(RUNBASEDVALUETYPE.FallDamageTaken, 0f, BrokenCondition.TookFallDamage)),
        [ACHIEVEMENTTYPE.NaturalistBadge] = BadgeRule.CleanRunForbidding(
            ItemTraits.PackagedFood,
            new RunValueCeiling(RUNBASEDVALUETYPE.PackagedFoodEaten, 0f, BrokenCondition.AtePackagedFood)
        ),
        [ACHIEVEMENTTYPE.SurvivalistBadge] = BadgeRule.CleanRun(new RunValueCeiling(RUNBASEDVALUETYPE.TimesPassedOut, 0f, BrokenCondition.PassedOut)),
        [ACHIEVEMENTTYPE.LeaveNoTraceBadge] = BadgeRule.CleanRunForbidding(
            ItemTraits.PlacesPermanentObject,
            new RunValueCeiling(RUNBASEDVALUETYPE.PermanentItemsPlaced, 0f, BrokenCondition.PlacedPermanentItem)
        ),
        [ACHIEVEMENTTYPE.LoneWolfBadge] = BadgeRule.CleanRun(new SoloOnly()),
        [ACHIEVEMENTTYPE.SpeedClimberBadge] = BadgeRule.CleanRun(new RunDurationCeiling(AchievementManager.ONE_HOUR_IN_SECONDS)),

        // Clean biomes, checked once the next area is reached (MountainProgressHandler.CheckAreaAchievement).
        [ACHIEVEMENTTYPE.CoolCucumberBadge] = BadgeRule.CleanRun(
            new BiomeOnMap(Biome.BiomeType.Mesa),
            new RunValueCeiling(RUNBASEDVALUETYPE.MaxHeatTakenInMesa, AchievementManager.MAX_MESA_HEAT_PERCENTAGE, BrokenCondition.TooMuchHeat)
        ),
        [ACHIEVEMENTTYPE.BundledUpBadge] = BadgeRule.CleanRun(
            new BiomeOnMap(Biome.BiomeType.Alpine),
            new RunValueCeiling(RUNBASEDVALUETYPE.MaxColdTakenInAlpine, AchievementManager.MAX_ALPINE_COLD_PERCENTAGE, BrokenCondition.TooMuchCold)
        ),
        [ACHIEVEMENTTYPE.TreadLightlyBadge] = BadgeRule.CleanRun(
            new BiomeOnMap(Biome.BiomeType.Roots),
            new RunValueCeiling(RUNBASEDVALUETYPE.MaxSporesTakenInRoots, AchievementManager.MAX_ROOTS_SPORES_PERCENTAGE, BrokenCondition.TooManySpores)
        ),
        // Traps only count in the swamp (AchievementManager.LocalCharacterHitByTrap); checked just before the peak.
        [ACHIEVEMENTTYPE.MedievalHistoryBadge] = BadgeRule.CleanRun(
            new BiomeOnMap(Biome.BiomeType.Swamp),
            new RunValueCeiling(RUNBASEDVALUETYPE.HitByTraps, 0f, BrokenCondition.HitByTrap)
        ),

        // Area badges, granted for each area passed after the one the player joined in
        // (MountainProgressHandler.CheckAreaAchievement; progress points read from the game's scenes).
        [ACHIEVEMENTTYPE.BeachcomberBadge] = BadgeRule.OneOff(new BiomeOnMap(Biome.BiomeType.Shore)),
        [ACHIEVEMENTTYPE.TrailblazerBadge] = BadgeRule.OneOff(new BiomeOnMap(Biome.BiomeType.Tropics)),
        [ACHIEVEMENTTYPE.AlpinistBadge] = BadgeRule.OneOff(new BiomeOnMap(Biome.BiomeType.Alpine)),
        [ACHIEVEMENTTYPE.NomadBadge] = BadgeRule.OneOff(new BiomeOnMap(Biome.BiomeType.Mesa)),
        [ACHIEVEMENTTYPE.ForestryBadge] = BadgeRule.OneOff(new BiomeOnMap(Biome.BiomeType.Roots)),
        [ACHIEVEMENTTYPE.WandererBadge] = BadgeRule.OneOff(new BiomeOnMap(Biome.BiomeType.Swamp)),
        [ACHIEVEMENTTYPE.VolcanologyBadge] = BadgeRule.OneOff(new BiomeOnMap(Biome.BiomeType.Volcano)),

        // One-off events tied to where their trigger is placed.
        // Action_ShowBinocularOverlay.TestLookAtSun requires the current biome to be the Mesa.
        [ACHIEVEMENTTYPE.AstronomyBadge] = BadgeRule.OneOff(new InBiome(Biome.BiomeType.Mesa)),
        // Scene placements read from the game's levels: every instance sits under these biomes' roots.
        [ACHIEVEMENTTYPE.MegaentomologyBadge] = BadgeRule.OneOff(new InBiome(Biome.BiomeType.Mesa)), // Antlion
        [ACHIEVEMENTTYPE.DaredevilBadge] = BadgeRule.OneOff(new InBiome(Biome.BiomeType.Mesa)), // ScoutCannonAchievementZone
        [ACHIEVEMENTTYPE.WebSecurityBadge] = BadgeRule.OneOff(new InBiome(Biome.BiomeType.Roots)), // Spider
        [ACHIEVEMENTTYPE.BellringerBadge] = BadgeRule.OneOff(new InBiome(Biome.BiomeType.Swamp)), // GhostFire bells
        [ACHIEVEMENTTYPE.AnimalSerenadingBadge] = BadgeRule.OneOff(new InBiome(Biome.BiomeType.Alpine, Biome.BiomeType.Mesa)), // Capybara
        [ACHIEVEMENTTYPE.ArboristBadge] = BadgeRule.OneOff(new InBiome(Biome.BiomeType.Tropics, Biome.BiomeType.Roots)), // GiantTreeAchievementZone

        // One-off events with no place constraint the code proves: their trigger is an item or a
        // character that can show up anywhere (Resources prefabs), or the summit itself.
        [ACHIEVEMENTTYPE.PeakBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.TriedYourBestBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.BingBongBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.EsotericaBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.ResourcefulnessBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.EnduranceBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.AeronauticsBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.UltimateBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.CompetitiveEatingBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.EmergencyPreparednessBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.TwentyFourKaratBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.MentorshipBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.AppliedEsotericaBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.LastResortBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.HangGlidingBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.DisasterResponseBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.NeedlepointBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.UndeadEncounterBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.MycoacrobaticsBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.ExorcistBadge] = BadgeRule.OneOff(),
        [ACHIEVEMENTTYPE.CryptogastronomyBadge] = BadgeRule.OneOff(), // Mandrake item
        [ACHIEVEMENTTYPE.WellRestedBadge] = BadgeRule.OneOff(), // EarlyWorm item
        [ACHIEVEMENTTYPE.RuleZeroBadge] = BadgeRule.OneOff(),
    };

    /// <summary>The lifetime stats the table reads, so the snapshot fetches only those.</summary>
    public static readonly STEAMSTATTYPE[] LifetimeStats = Rules
        .Values.Select(rule => rule.ProgressMeasure)
        .OfType<LifetimeStatTarget>()
        .Select(target => target.Stat)
        .ToArray();

    private static readonly BadgeRule Unproven = BadgeRule.OneOff();

    public static BadgeRule For(ACHIEVEMENTTYPE badge) => Rules.TryGetValue(badge, out BadgeRule rule) ? rule : Unproven;
}
