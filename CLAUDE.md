# PeakAchiever

BepInEx 5 mod for PEAK: pin badges from the pause menu, track during the run which ones can still be
earned. User-facing behaviour is in `README.md`; this file holds what the code and git history do not.

## Layout

- `src/PeakAchiever/Tracking/`: pure rules, no Unity calls. `BadgeRules.cs` is the single table of how
  each badge is judged; `RunFacts` is the snapshot the rules read.
- `Game/`: adapters reading the game (`RunFactsReader`, `BadgeCatalog`, `PinnedBadgeTracker`) and the
  refresh hooks. `Hud/`, `PauseMenu/`, `Inventory/`, `Controls/`: UI plus their Harmony hooks.
- `Localization/ModText.cs`: every mod string, one row per key, English then French. Badge names and
  descriptions come from the game's own table.

## Rules for badge logic

- **Every rule cites its source**: the decompiled class and method, or the scene data it was read from,
  with the game version. Code rules last verified against **PEAK 2.6.b**; the scene placements behind the
  "found in a biome" rules were last read in 2.4.c. Re-check `BadgeRules.cs` after a game update.
- **Never a red cross without proof.** A badge nothing in the game can rule out stays "doable".
- Read run counters from `runBasedValueData` dictionaries directly: `GetRunBasedInt/Float` write a zero
  for a missing key, which raises `Player.OnAchievementProgressChanged` and loops the refresh.
- Game assets (icons, texts, fonts) are read at runtime and never shipped. Only `src/PeakAchiever/Assets/*.png`
  (drawn for the mod) and `icon.png` go into the package.

## UI inside the game's menus

- **Anything placed in one of the game's menus is a copy of that menu's own element**, never a widget built
  in code: `Instantiate` a sibling (a Controls row, the badges page's Back button), drop its game-bound
  component, give its `Button` a fresh `onClick` (that also drops the inspector's listeners), and insert it
  in the menu's own layout. Its label goes through `GameTextTable`, so its `LocalizedText` keeps
  translating. `UiFactory` widgets are only for the mod's own surfaces (tracker cards, toast, stats panel).
- Unity objects override the null check: never `?.`, `??` or `is { }` on them, compare with `== null`.

## Build and tests

- `dotnet` must be the SDK 10 in `$HOME/.dotnet`; the system one is 7.0. In a shell:
  `export DOTNET_ROOT=$HOME/.dotnet PATH="$HOME/.dotnet:$PATH"`.
- The gate is `dotnet build` then `dotnet test`. Tests need the game installed: they load its assemblies.
  Paths live in the git-ignored `Config.Build.user.props`.
- `tests/PeakAchiever.Tests/PeakAchiever.Tests.csproj` declares `IsTestProject` itself: restore evaluates the PolySharp
  exclusion before `Microsoft.NET.Test.Sdk` props load.
- `BepInEx.Core` (target `SkipBepinRefs`) strips BepInEx and Harmony from build output; the test project
  copies them back.
- MonoMod cannot detour on the .NET 10 test host, so hooks cannot be applied in tests.
  `HarmonyPatchTargetsTests` resolves each target through HarmonyX's internal `PatchTools.GetOriginalMethod`
  instead. Adding a hook means updating its expected count.
- Prove a new test with a mutation: break the code, watch it go red, restore.
- xUnit rejects a public theory taking an internal type: pass an internal enum as `object` and cast it.

## Investigating the game

- Decompile: `ilspycmd -p -o <out> -r <PEAK>/PEAK_Data/Managed <PEAK>/PEAK_Data/Managed/Assembly-CSharp.dll`.
  If a `dotnet` tool reports a missing runtime, set `DOTNET_ROOT=$HOME/.dotnet`.
- Scene and prefab data (which biome holds an Antlion, progress points, secret badges): UnityPy, one file
  at a time. Read each MonoBehaviour's `m_Script` header without a type tree generator, attach the generator
  only for matching scripts, and write results to disk as they come. Loading every scene with the generator
  attached ran past 25 minutes and 11 GB.
- Single assets are cheaper than scenes. The level table is the `MapBaker` MonoBehaviour in
  `PEAK_Data/data.unity3d`: find it by the name after the `m_Script` header, then decode its raw bytes by
  hand. In 2.4.c, 48 bytes of base-class fields sit before `ScenePaths`, then `BiomeIDs`, then
  `selectedBiomes` (each: `List<BiomeType>` as int32, `List<string>` variant names). At runtime the mod
  logs the same table: `25 levels, map layouts: ...`.
- Game facts the rules depend on and the code alone does not show: `SerializableRunBasedValues` does not
  serialize `nonToxicMushroomsEaten`, so Mycology restarts at zero after a reconnect; `RunManager.RunId`
  can be empty (seen in a solo run), hence the mod's own run key in `SplitHistoryStore`.

## Running the game with the mod

- A Debug build deploys the DLL to the profile set by `PEAKBepInExDir`. The game loads it only at startup.
- Remove any Thunderstore-installed copy (`plugins/Altaks-PeakAchiever/`) from that profile first: with the
  same version, BepInEx skips one of the two copies ("Skipping [PeakAchiever] because a newer version
  exists") and it may be the Debug build.
- Launch through Steam with the profile's preloader. PEAK ships **Doorstop 4.4.1**, whose flag is
  `--doorstop-target-assembly`; the Doorstop 3 flag `--doorstop-target` silently loads no mod.
  ```
  steam.exe -applaunch 3527290 --doorstop-enable true --doorstop-target-assembly "<profile>\BepInEx\core\BepInEx.Preloader.dll"
  ```
- Confirm with `<profile>/BepInEx/LogOutput.log`: a fresh timestamp and `Plugin PeakAchiever is loaded!`.
  HarmonyX warnings about `MoreAscents`, `Everest`, `UnnamedProducts`, `PropSpawner_*` come from other mods.

## Publishing to Thunderstore

- Team `Altaks`, package `PeakAchiever`. The token sits in Windows Credential Manager under
  `service thunderstore team Altaks`; it never goes through the chat.
- `Config.Build.user.props` must keep the template's `EnsureThunderstoreToken` target, or publishing
  fails with "no value for Token".
- Bump `<Version>` in `src/PeakAchiever/PeakAchiever.csproj` and `CHANGELOG.md` first: a published version
  is permanent. Then `dotnet build -c Release -p:PublishTS=true`, at default verbosity (`-v d` prints the token).
