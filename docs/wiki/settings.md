## In the game's menus

| Where | Row | Default | What it does |
| --- | --- | --- | --- |
| Settings, General | PeakAchiever: max pinned badges | 5 | How many badges you can pin at once (1 to 12). |
| Settings, General | PeakAchiever: max team pins | 5 | How many badges the host can pin for the team, on top of their own (1 to 12). |
| Settings, General | PeakAchiever: on-screen markers | On | A marker over what a locator points at (belltower, antlion, Mesa tomb). |
| Settings, Controls | PeakAchiever: show / hide the tracker | F6 | Click it and press a key, or reset it. |

## Config file

`BepInEx/config/Altaks.PeakAchiever.cfg`, section `[Tracker]`. The menu rows edit the same keys.

| Key | Default | Meaning |
| --- | --- | --- |
| `ToggleKey` | `<Keyboard>/f6` | Shows or hides the tracker. |
| `MaxPinnedBadges` | `5` | 1 to 12. |
| `MaxTeamPins` | `5` | 1 to 12. |
| `ShowMarkers` | `true` | On-screen markers over locator targets. |
| `PinnedBadges` | empty | Your pins, edited from the badges page. A leading `+` marks a badge pinned for an ally. |

## Statistics

On the badges page, **Statistics** opens your biome times per ascent: how many times, median, best, and the whole climb of each map layout. **Erase this ascent** asks for a second click within 3 seconds. The times live in `BepInEx/config/Altaks.PeakAchiever.splits.csv`; delete it to reset the ETA.
