# Handoff: PeakAchiever, 2026-09-29

State of the work and what comes next, for a fresh session. Project rules, build, test, launch and
publish steps live in `CLAUDE.md`; behaviour in `README.md`; changes in `CHANGELOG.md`. Replies to the
user are in French.

## Where things stand

- `main` at `f1193c4`: gate green (`dotnet build`, `dotnet test`: 200 tests). No open PR, no other branch.
- **0.2.0 is published** on Thunderstore: https://thunderstore.io/c/peak/p/Altaks/PeakAchiever/
  (PRs #3 to #21).
- **Unreleased on `main`** (`CHANGELOG.md`, section `[Unreleased]`), merged from PRs #22 to #25:
  statistics panel in PEAK's own style, rate and time bars as stamina bar segments, pins for allies,
  tearing cards and lost runs. Each PR description has its manual testing checklist.
- Validated in game by the user: the statistics panel and its hints, the affliction bars (after #24's
  fix), pins for allies. **Not yet seen in game: the tearing cards (#25).**
- The test profile holds only the Debug build (the Thunderstore-installed copy was deleted on the
  user's request). The user asked that nothing be launched at the end of this session.

## Next steps

1. Have the user test #25 in game, following its checklist. The open question is whether the tracker
   is visible on the end screen at all: if it is hidden there, a lost run's cards tear unseen
   (`TrackerHud` shows cards only when `!GUIManager.InPauseMenu`; the end screen's state is untested).
2. Then release **0.2.1**: bump `<Version>`, turn `[Unreleased]` into `[0.2.1]`, publish as `CLAUDE.md`
   says. A published version is permanent; publish only once the user confirms.

## Backlog the user has not scheduled

- Mycology restarts at zero after a reconnect: the game does not serialize `nonToxicMushroomsEaten`
  (see `CLAUDE.md`). Idea: say so on the card.
- Gamepad navigation: the statistics panel and the tracker-key row on the Controls page take mouse
  clicks only.
- The tracker-key row hides the native duplicate-key warning; it could compare with the game's bindings.
- The Nadir biome (`Biome.BiomeType.Void`) has no name key in `StatusText.BiomeNameKeys`: it would show
  "Void" in the splits.
- The Peak split came out at 6 s in the one run recorded: the final climb seems to count under the
  previous biome. Confirm on more runs before calling it right.
- In a run the game gives no id, rejoining after restarting the game can duplicate split lines.
- `HudStyle` still has `gameCheck ?? Circle`, a `??` on a Unity object (the rule in `CLAUDE.md` says
  `== null`).

## How the user works (beyond `CLAUDE.md`)

- Visual work: a mockup first (an Artifact), signed off, then code. A mockup shows only what was
  asked; ideas on top are offered in words (also in the user's memory file `mockup-scope.md`).
- UI inside the game's menus reuses PEAK's own elements (rule in `CLAUDE.md`).
- Several tasks become a stack of PRs, one per task; the user asks to "merge toute la stack": merge
  bottom-up with merge commits, retargeting each PR to `main` first, then delete the merged branches.
- Commits: Conventional Commits, scoped, no AI attribution.
- To learn how a game UI element is built, a temporary dump (never committed) logs its hierarchy at
  runtime (sprites, colours, fonts, sizes); that is how the pause menu and the stamina bar segments
  were read.

## Reference

- Mockups (private Artifacts): statistics panel directions
  https://claude.ai/artifact/KkCsJTRhUYCWA7tkCLbmcu, tearing animation
  https://claude.ai/artifact/9mbJBkEJs4pCcoqL5uisn1, affliction bars
  https://claude.ai/artifact/5wPUMsihzdgiquvEqreEZA.
- Decompiled game code is not in the repo: regenerate it with the `ilspycmd` line in `CLAUDE.md`.

## Suggested skills

- `alta-dev`: the working standard for any change (orientation, spec, tests with a mutation proof,
  review, delivery).
- `grill-me`: before building anything from the backlog, to settle its open choices with the user.
- `artifact-design`: before any mockup of a visual change.
- `handoff`: at the end of the next session.
