# PeakAchiever

Pin the badges you want to chase, and see during your run which ones you can still earn.

- **Pin from the pause menu.** Open the badges page of the pause menu and click a badge: a yellow
  pin marks it, and the badge's tooltip says whether a click pins or unpins it.
  Works in the airport too, so you can pick your targets before take-off.
- **Today's map.** In the airport, badges today's map cannot hold carry a red cross on the badges
  page, and their tooltip names the missing biome ("Not on today's map (Roots)"). During a run the
  same mark reads the run's own map. They can still be pinned, for another day.
- **Suggested pins.** The badges page stars as many badges as there are free pin slots: all doable
  on today's map (or the run's), and compatible with your pins and with each other. Badges tied to
  that map's biomes come first (the map alternates, so they wait otherwise), then clean runs. A locked
  secret badge is never starred.
- **No incompatible pins.** After the Shore, a map holds one biome of each pair: Tropics or Roots, Alpine
  or Mesa, Caldera or the Gloom (read from the game's level table). A badge that needs a biome no map shares with a pinned badge is
  faded on the badges page, its tooltip names the pin in the way, and clicking it is refused. Only
  proven clashes count: Lone Wolf, for instance, stays pinnable with Clutch, since other scouts can
  leave before the summit.
- **Track in the top-right corner.** During a run, each pinned badge shows its icon, name and
  condition, taken from the game in your language.
  - Each card is a dark, stitched patch: icon and state mark, the name with its chips, the condition,
    the status, and across the whole card, what the badge needs.
  - A yellow bar and `current / total` for badges with a counter. An "All runs" chip marks counters
    the game keeps across all runs (meals cooked, height climbed...).
  - A badge tied to a biome says where it is on this run's map: "Mesa, next biome", "Mesa, in 2
    biomes", then "Now, in the Mesa" in yellow.
  - The items a badge needs, as the game's own inventory icons with a count when several are needed:
    "Needs" or "One of" on the left, "Helps" on the right (from the PEAK wiki, 24 Karat from the game's
    code: only the Ancient Idol counts).
  - Bellringer counts the belltowers the whole team has lit, against 5. Bellringer, Megaentomology and
    24 Karat have a locator under the icon: an arrow turned towards the nearest unlit belltower,
    antlion or the Mesa tomb, with its distance. A marker can also show over that target on screen
    (Settings, General, "On-screen markers"; on by default).
  - "Holding so far" for clean-run badges (no fall damage, no packaged food...) still intact.
  - Speed Climber shows the run time on a bar drawn like the stamina bar's arrow segment, its ETA
    beside it, and the time spent in each biome so far, one biome a line in columns (the current one
    in yellow). The time and splits stay under the red cross once the hour is over.
  - Each biome time is followed by its gap to the median of past runs at the same ascent: `+1:20`
    in red when slower, `-0:40` in green when faster. The biome in progress shows a gap only once
    it runs over its median.
  - The ETA is the run time so far, plus the median time past runs at the same ascent took for the
    rest of the current biome and for every biome ahead. It shows once each of those biomes has been
    finished at least once with the mod installed: green within the hour, orange past it.
  - Cool Cucumber, Bundled Up and Tread Lightly show the highest heat, cold or spores rate reached in
    their biome against the limit, as `max 4% / 10%` and a bar drawn like that affliction's segment of
    the stamina bar. From 75% of the limit (8% of 10%) the bar's outline blinks and the margin left
    shows in orange ("2% left").
  - Foraging, Mycology, Advanced Mycology and Gourmand show every item that counts as a grid of
    icons: dimmed until eaten this run, then in full colour with a green tick. The item lists are read
    from the game, and written to `BepInEx/LogOutput.log` the first time a run needs them. Items no
    spawner, item or luggage of this level can yield move to a fainter row, "Not seen on this map":
    a hint, since an item could still come from somewhere the mod does not read, so the badge never
    gets a red cross for it.
  - A green check once earned; 5 seconds later the card folds to one line, under the cards in play.
  - A red cross, with the reason, once the badge can no longer be earned this run: its biome is not
    on this map or is behind you, the clean-run condition broke, or the run was lost (the end screen
    opened with nobody at the summit). The card tears in two, falls to the end of the tracker and
    settles there as one torn line.
- **Forbidden items marked in the inventory.** While a pinned Naturalist or Leave No Trace badge is
  still holding, a red cross sits in the bottom-right corner of every hotbar slot holding an item that
  would break it (packaged food; pitons, rope spools, the rope cannon, the chain launcher and other
  placeable objects). The backpack slot shows a yellow warning if something forbidden is inside, and
  the opened backpack shows the cross on that item. The marks go away once the badge is earned or broken.
- **Pins persist.** They are saved in the config. At the start of the next run, badges earned are
  unpinned and failed ones are doable again.
- **Help an ally.** A badge you already earned can be pinned too, as a reminder of what a friend is
  chasing in a multiplayer run: its card carries a "For an ally" chip and "You have it", and it stays
  pinned from one run to the next until you unpin it.
- **Scouts, for the host.** In a multiplayer game, the host's badges page has a **Scouts** button: it
  lists the badges worth pinning for the team, those nobody has first, then those some scouts still
  miss. Scouts with the mod share which badges they earned; the others show as unknown. The team pins
  stay when the host leaves (the next host with the mod takes over), and those every scout has earned
  are dropped at the next run start. They show on the tracker of every scout with the mod, as a group
  above their own pins.
- **In your language.** The mod's own texts come in the game's 15 languages (English, French, Italian,
  German, Spanish for Spain and Latin America, Brazilian Portuguese, Russian, Ukrainian, Simplified and
  Traditional Chinese, Japanese, Korean, Polish, Turkish), in the game's own words, and follow the
  language set in the game's settings.
- Secret badges keep their `???` until earned. The tracker hides while the pause menu is open.
- Other players do not need the mod: the mod only shares data under its own keys, which the game ignores.

## Installation

Requires [BepInExPack for PEAK](https://thunderstore.io/c/peak/p/BepInEx/BepInExPack_PEAK/).

- **With a mod manager** (Thunderstore Mod Manager, r2modman, Gale): install PeakAchiever into your
  PEAK profile and launch the game from the manager.
- **By hand**: copy `Altaks.PeakAchiever.dll` into `BepInEx/plugins/` of your PEAK BepInEx install.

Tested with PEAK 2.6.b. If a game update breaks the mod, the error shows in `BepInEx/LogOutput.log`.

## Configuration

`BepInEx/config/Altaks.PeakAchiever.cfg`, section `[Tracker]`:

| Key | Default | Meaning |
| --- | --- | --- |
| `ToggleKey` | `<Keyboard>/f6` | Shows or hides the tracker during a run. Set it from the game's Controls menu, in the row "PeakAchiever: show / hide the tracker" at the end of the first column (click it and press a key, or reset it to F6). A key saved by 0.1.0 is converted; one with modifiers falls back to F6. |
| `MaxPinnedBadges` | `5` | How many badges can be pinned at once (1 to 12). Also in the game's Settings, General tab: "PeakAchiever: max pinned badges". |
| `MaxTeamPins` | `5` | How many badges the host can pin for the whole team in a multiplayer game, on top of their own (1 to 12). Also in Settings, General: "PeakAchiever: max team pins". |
| `PinnedBadges` | empty | The pins, edited from the pause menu. |

On the badges page, a **Statistics** button next to the Back button opens the biome times per ascent:
how many times, median, best, and the whole climb of each map layout. **Erase this ascent** asks for
a second click within 3 seconds, then removes that ascent's lines from the file. The panel takes
mouse clicks; it is not reachable with a gamepad yet.

The time of every biome you finish (mini runs excepted, and not the biome you joined a run in) goes
to `BepInEx/config/Altaks.PeakAchiever.splits.csv`, one line each: run id (the game's, or one the mod
makes when the game gives none), ascent, place in the run, biome, seconds. Delete the file to reset the ETA.

## How badges are judged

Every rule comes from the game's own code (checked against game version 2.6.b) and scene data (read in
2.4.c). When nothing in the game can rule a badge out, the tracker never shows a red cross for it.

| Kind | Badges | Red cross when |
| --- | --- | --- |
| Run counter | Knot Tying, Clutch, Plunderer, First Aid, Jester, Archery, Foraging, Mycology, Advanced Mycology, Gourmand | never |
| Lifetime counter | Cooking, Happy Camper, Bouldering, Toxicology, Ascender, Bookworm, Calcium Intake | never |
| Clean run | Balloon, Naturalist, Survivalist, Leave No Trace, Speed Climber | the condition breaks |
| Alone at the summit | Lone Wolf | never: the game counts scouts only at the summit, and others can leave before |
| Clean biome | Cool Cucumber, Bundled Up, Tread Lightly, Medieval History | the biome is not on the map, or the condition breaks |
| Area reached | Beachcomber, Trailblazer, Alpinist, Volcanology, Nomad, Forestry, Wanderer | the biome is not on the map |
| Found in a biome | Astronomy, Megaentomology, Daredevil (Mesa); Web Security (Roots); Bellringer (Gloom); Animal Serenading (Alpine or Mesa); Arborist (Tropics or Roots) | none of those biomes is on the map, or all are behind you |
| Anywhere | every other badge | never |

If an achievement-blocking mode is active for the run, the tracker says so above the cards.

## Credits and licence

MIT licence, see `LICENSE`. The mod ships only its own cross and pin icons. Badge icons, names,
descriptions and fonts are read from your copy of the game while it runs, and never redistributed.
PEAK belongs to its developers; this mod is not affiliated with them.

## Development

Built from the PEAK BepInEx template (NuGet `PEAKModding.BepInExTemplate`). Copy
`Config.Build.user.props.template` to `Config.Build.user.props` and point it at your game and BepInEx
folders; a Debug build then copies the plugin there.

```sh
dotnet build          # builds and deploys the plugin
dotnet test           # unit tests; needs the game installed, they load its assemblies
dotnet build -c Release   # Thunderstore package in ./artifacts/thunderstore/
```
