# Changelog

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Added

- Speed Climber card: a bar of the run time against one hour, and the time spent in each biome, the
  current one in yellow. Past one hour the red cross keeps the run time and the splits below it.
- Cool Cucumber, Bundled Up and Tread Lightly cards: a bar of the highest heat, cold or spores rate
  reached in the biome against the game's limit (10%, 20%, 25%).
- Speed Climber ETA: when the summit should be reached, from the median time of each biome in past
  runs at the same ascent. The times of every biome finished are saved to
  `BepInEx/config/Altaks.PeakAchiever.splits.csv`, mini runs excepted.
- Badges that no map allows together can no longer be pinned together: the click is refused with a
  message naming the pinned badge in the way, and the badges page fades them, with the reason in
  their tooltip. The maps come from the game's own level table.
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
