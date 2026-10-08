# Handoff: PeakAchiever, 2026-10-08

State of the work and what comes next, for a fresh session. Project rules, build, test, launch and
publish steps live in `CLAUDE.md`; behaviour in `README.md`; changes in `CHANGELOG.md`. Replies to the
user are in French.

## Where things stand

- **0.3.0 is published** on Thunderstore (about 1.5k downloads). PEAK updated to **2.6.b** on Oct 6.
- **An open stack of PRs, nothing merged**, each based on the one before, merge bottom-up:
  1. #27 `fix/game-2.6.b`: Nadir segment (no ETA crash, no false "behind you" cross), Nadir name
     (`AREA_VOID`), Scout Effigy out of Leave No Trace, today's map wrapped over `ScenePaths`, docs.
  2. #28 `fix/stats-height`: the statistics rows scroll past 520 px (`PaperKit.ScrollList`).
  3. #29 `feat/settings-pin-rows`: MaxPinnedBadges / MaxTeamPins as rows of Settings, General.
  4. #30 `feat/hud-cards`: new cards, needed items as tabs under the card, locators and on-screen
     markers (with an OFF / ON row), bell counter, team pins on every modded tracker, wiki drafts.
  5. #31 `feat/i18n-languages`: the mod's strings in the game's 15 languages, HUD font fallbacks.
- Gate green on the tip: `dotnet build`, `dotnet test` 254/254. The user tested #30 in game and signed it
  off ("c'est bon pour moi"); #27 to #29 and #31 are not seen in game yet, each PR lists its checks.
- The test profile holds only the Debug build: the Thunderstore copy (`plugins/Altaks-PeakAchiever/`) was
  removed for testing, to be reinstalled from the mod manager.

## Next steps

1. Merge the stack bottom-up when the user asks (retarget each PR to `main` first, merge commits, delete
   the branches), then release **0.4.0**: bump `<Version>`, turn `[Unreleased]` into `[0.4.0]`, publish
   as `CLAUDE.md` says, only once the user confirms.
2. Screenshots for the Thunderstore page: the user takes them (shot list in the conversation: tracker with
   4 to 5 cards, badges page, Bellringer locator and marker, statistics, Scouts and the team group,
   Settings rows, inventory cross). Commit them under `docs/screenshots/`, link them from the README with
   raw GitHub URLs (Thunderstore renders the README).
3. Wiki: the six drafts in `docs/wiki/` wait for the user's review; publish them through the API in
   `CLAUDE.md` only on the user's OK (they are public at once).

## Backlog the user has not scheduled

- Aeronautics' balloon bunch tile was missing once in game though the item resolves; a debug line in
  `NeedsBlock` lists what each row found. Check it before calling it fixed.
- The "found in a biome" scene placements (Antlion, Spider, Capybara, GhostFire, cannon zone, giant tree)
  were last read in 2.4.c; 2.6.b has 21 levels in 8 layouts. Re-read them with UnityPy (heavy).
- 24 Karat: no red cross when the map has no tomb until the scene data proves the idol comes only from it;
  the user also mentioned the tomb being "open", which the code does not model.
- After a host warp (`Action_WarpToBiome`), `JoinedInSegment >= 0` for everyone, so the first biome time
  loses its gap; entering the Nadir un-crosses torn biome cards (nothing proves them impossible there).
- `MapItemScanner` misses `SingleItemSpawner` and `CookingBehavior_ReplaceItem`: an item can show "Not seen
  on this map" by mistake (a hint only, never a red cross).
- Earlier backlog still open: Mycology resets after a reconnect; gamepad navigation of the panels; the
  tracker-key row hides the game's duplicate-key warning; duplicate split lines after a rejoin with no run id;
  the 6 s Peak split; `HudStyle` `gameCheck ?? Circle` on a Unity object.
- Native-speaker pass on the ja, ko, tr and pl strings (flagged guesses: the word for "host" in es, pt, zh,
  zh-TW, tr).

## How the user works (beyond `CLAUDE.md`)

- Visual work: a mockup first (an Artifact), signed off, then code. The user reviews by commenting on the
  artifact and by sketching (the card sketch board below); read the comments and sketches with the
  ArtifactComments / ArtifactData tools.
- Cards show facts as icons and counts, never explanatory sentences (memory `card-content.md`).
- UI inside the game's menus reuses PEAK's own elements; the Settings menu takes registered settings.
- Several tasks become a stack of PRs, one per task; commits Conventional, scoped, no AI attribution.

## Reference

- Card mockups (private Artifacts): states and picks https://claude.ai/artifact/WkVtcMZzruGjKcAc192urD,
  the 64-badge catalogue https://claude.ai/artifact/YZg44cWqgJG6jWT2YYGEPK, the sketch board
  https://claude.ai/artifact/K3AATHdhw5UMKDMTmWPH2V. Older: statistics panel
  https://claude.ai/artifact/KkCsJTRhUYCWA7tkCLbmcu, tearing https://claude.ai/artifact/9mbJBkEJs4pCcoqL5uisn1,
  affliction bars https://claude.ai/artifact/5wPUMsihzdgiquvEqreEZA.
- Decompiled game code is not in the repo: regenerate it with the `ilspycmd` line in `CLAUDE.md`.

## Suggested skills

- `alta-dev`: the working standard for any change (orientation, spec, tests with a mutation proof,
  review, delivery).
- `grill-me`: before building anything from the backlog, to settle its open choices with the user.
- `artifact-design`: before any mockup of a visual change.
- `handoff`: at the end of the next session.
