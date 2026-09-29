# Changelog

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Added

- Speed Climber card: a bar of the run time against one hour, and the time spent in each biome, the
  current one in yellow. Past one hour the red cross keeps the run time and the splits below it.
- Cool Cucumber, Bundled Up and Tread Lightly cards: a bar of the highest heat, cold or spores rate
  reached in the biome against the game's limit (10%, 20%, 25%).
- Speed Climber splits compare with the median of past runs at the same ascent: `(+1:20)` in red when
  slower, `(-0:40)` in green when faster; the current biome only once it runs over.
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
