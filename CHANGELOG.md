# Changelog

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

## [0.4.0] - 2026-10-08

### Added

- Tracker cards, redone: a dark stitched patch with chips under the name ("All runs", "For an ally") and a section across the card for what the badge needs.
- Biome badges say where their biome is on the run's map ("Mesa, next biome", "Now, in the Mesa").
- The items each badge needs or is helped by, as the game's inventory icons with a count (from the PEAK
  wiki; 24 Karat takes only the Ancient Idol, the one item the Kiln's lava accepts).
- Bellringer counts the belltowers the whole team has lit. Bellringer, Megaentomology and 24 Karat get a
  locator under the icon (arrow and distance to the nearest unlit belltower, antlion or the Mesa tomb),
  and an on-screen marker over it, switchable in Settings, General.
- The host's team pins now show on every modded tracker, as a group above the own pins.
- The two pin limits, `MaxPinnedBadges` and `MaxTeamPins`, are sliders at the end of the game's Settings,
  General tab (1 to 12). They edit the same config keys; the refusal toasts now point there.
- The mod speaks all 15 of the game's languages, not only English and French, using the game's own words.

### Fixed

- PEAK 2.6.b: in the Nadir, Speed Climber no longer breaks the tracker, and badges tied to a biome no
  longer get a "behind you" red cross (items can warp the team to any segment from there). The Nadir has
  no ETA, and shows under its own name in the biome times.
- The Scout Effigy no longer carries the Leave No Trace red cross in the inventory: building it revives a
  scout and places nothing that counts.
- In the airport, today's map is worked out over the game's scene list, as the check-in kiosk does.
- The statistics panel no longer runs off the screen: past 520 px its rows scroll with the mouse wheel,
  and a short ascent still gets a short panel. PEAK 2.6.b has eight map layouts, one total row each.

### Changed

- Earned cards fold to one line 5 seconds after the check mark shows; torn cards settle as one torn line
  once their tear has played. Order: in play, earned, torn.
- Speed Climber: the ETA sits beside the run time; the splits read one biome a line, in columns.
- A rate close to its limit shows the margin left.
- Checked against PEAK 2.6.b: every badge rule still matches the game. Maps now pair their biomes freely
  (8 layouts instead of 2), which the pin clashes already follow, read from the game's level table.

## [0.3.0] - 2026-09-29

### Added

- **Scouts panel**, for the host of a multiplayer game: a **Scouts** button on the badges page lists the
  badges worth pinning for the whole team, those no scout has first ("Recommended"), then those some
  still miss, with who misses them. Up to `MaxTeamPins` (5 by default) can be pinned. The pins stay in
  the room when the host leaves, the next host with the mod takes over, and at each run start the pins
  every scout with the mod has earned are dropped. The team pins do not show on the trackers yet.
- Players with the mod share which badges they earned, through Photon player properties under a
  `PeakAchiever.` key: players without the mod ignore them, and appear in the panel as unknown.

- A card whose badge becomes impossible tears in two along a jagged line, then falls to the end of the
  tracker, where it stays torn and crossed out.
- A lost run (the end screen with nobody at the summit, solo or the whole team) rules out every badge
  still in play, "Impossible: run lost", and their cards tear one after the other.
- Badges already earned can be pinned, to help an ally earn them in a multiplayer run. They stay pinned
  from one run to the next until unpinned; the config marks them with a leading `+`.

### Changed

- The heat, cold and spores bars are drawn like the matching segment of PEAK's stamina bar (its hatched
  fill, outline and icon), and the Speed Climber time like the arrow segment. Near the limit the outline
  blinks instead of the bar turning orange.
- The statistics panel is made of PEAK's own parts: the badge popup's paper over a dark veil, the game's
  ribbons as buttons. The ascent arrows turn grey, with a hint, when no other ascent has times.
- The Back, Statistics and Scouts buttons sit side by side: the Statistics button used to cover Back.

## [0.2.0]

### Fixed

- Speed Climber no longer shows its red cross during the last second of the hour, which the game still counts.
- Lone Wolf no longer shows a red cross when other scouts are in the run: the game only counts scouts at the summit, and they can leave before.
- Biome times are now recorded in runs the game gives no id to (a solo run, among others): the mod
  makes one for the run.
- The Foraging checklist no longer lists Clusterberry_UNUSED, an item the game no longer uses.

### Changed

- `ToggleKey` in the config is now an Input System path (`<Keyboard>/f6`). A key saved by 0.1.0 is
  converted; one with modifiers falls back to F6, with a warning in `LogOutput.log`.
- Progress bars have fully round ends and a light rim.

### Added

- Speed Climber card: a bar of the run time against one hour, and the time spent in each biome, the
  current one in yellow. Past one hour the red cross keeps the run time and the splits below it.
- Cool Cucumber, Bundled Up and Tread Lightly cards: a bar of the highest heat, cold or spores rate
  reached in the biome against the game's limit (10%, 20%, 25%).
- Speed Climber splits compare with the median of past runs at the same ascent: `(+1:20)` in red when
  slower, `(-0:40)` in green when faster; the current biome only once it runs over.
- The tracker key can be set from the game's Controls menu (pause menu, Controls), in a row like the
  game's own at the end of the first column: click it and press a key on the game's rebinding page,
  or use its reset button to go back to F6.
- Suggested pins: on the badges page, a yellow star on as many badges as there are free pin slots,
  all doable on today's map (or the run's) and compatible with the pins and each other. Badges tied
  to that map's biomes come first, then clean runs. Locked secret badges are never starred.
- Statistics panel, opened from a Statistics button under the Back button of the pause menu's badges page:
  per ascent, each biome's times, median and best, the whole climb of each map layout, and a
  two-click erase of the ascent shown.
- Warnings in orange: the heat, cold and spores bar and figures from 75% of the limit (8% of 10%),
  and the Speed Climber ETA once it is past one hour.
- Speed Climber ETA: when the summit should be reached, from the median time of each biome in past
  runs at the same ascent. The times of every biome finished are saved to
  `BepInEx/config/Altaks.PeakAchiever.splits.csv`, mini runs excepted.
- Badges that no map allows together can no longer be pinned together: the click is refused with a
  message naming the pinned badge in the way, and the badges page fades them, with the reason in
  their tooltip. The maps come from the game's own level table.
- Eating checklists: the items this map yields none of move to a separate, fainter row, "Not seen on
  this map". A hint read from the level's spawners, never a red cross.
- Badges page: a red cross on the badges today's map cannot hold (the level the airport kiosk sends,
  or the run's own map during a run), with the missing biome in the tooltip. They stay pinnable.
- Foraging, Mycology, Advanced Mycology and Gourmand cards: a grid of every item that counts, dimmed
  until eaten this run, then in full colour with a green tick.

## [0.1.0]

### Added

- Pin and unpin badges from the pause menu's badges page, with a pin marker and a click hint.
- Top-right tracker during a run: progress bars, "holding so far", green check and red cross with reason.
- Pins saved in the config, earned ones cleared at the start of the next run.
- `ToggleKey` (default F6) and `MaxPinnedBadges` (default 5) settings.
- Red cross on inventory slots holding an item that would break a pinned Naturalist or Leave No Trace
  badge, and a warning on the backpack slot when one is inside.
- English and French texts.
