# PeakAchiever

Pin the badges you want to chase, and see during your run which ones you can still earn.

- **Pin from the pause menu.** Open the badges page of the pause menu and click a badge you have not
  earned yet: a yellow pin marks it, and the badge's tooltip says whether a click pins or unpins it.
  Works in the airport too, so you can pick your targets before take-off.
- **Track in the top-right corner.** During a run, each pinned badge shows its icon, name and
  condition, taken from the game in your language.
  - A yellow bar and `current / total` for badges with a counter. `LIFETIME` marks counters the game
    keeps across all runs (meals cooked, height climbed...).
  - "Holding so far" for clean-run badges (no fall damage, no packaged food...) still intact.
  - Speed Climber shows the run time against one hour, and the time spent in each biome so far (the
    current one in yellow). The time and splits stay under the red cross once the hour is over.
  - A green check once earned.
  - A red cross, with the reason, once the badge can no longer be earned this run: its biome is not
    on this map or is behind you, or the clean-run condition broke.
- **Forbidden items marked in the inventory.** While a pinned Naturalist or Leave No Trace badge is
  still holding, a red cross sits in the bottom-right corner of every hotbar slot holding an item that
  would break it (packaged food; pitons, rope spools, the rope cannon, the chain launcher and other
  placeable objects). The backpack slot shows a yellow warning if something forbidden is inside, and
  the opened backpack shows the cross on that item. The marks go away once the badge is earned or broken.
- **Pins persist.** They are saved in the config. At the start of the next run, badges earned are
  unpinned and failed ones are doable again.
- Secret badges keep their `???` until earned. The tracker hides while the pause menu is open.
- Client-side only: other players do not need the mod.

## Installation

Requires [BepInExPack for PEAK](https://thunderstore.io/c/peak/p/BepInEx/BepInExPack_PEAK/).

- **With a mod manager** (Thunderstore Mod Manager, r2modman, Gale): install PeakAchiever into your
  PEAK profile and launch the game from the manager.
- **By hand**: copy `Altaks.PeakAchiever.dll` into `BepInEx/plugins/` of your PEAK BepInEx install.

Tested with PEAK 2.4.c. If a game update breaks the mod, the error shows in `BepInEx/LogOutput.log`.

## Configuration

`BepInEx/config/Altaks.PeakAchiever.cfg`, section `[Tracker]`:

| Key | Default | Meaning |
| --- | --- | --- |
| `ToggleKey` | `F6` | Shows or hides the tracker during a run. |
| `MaxPinnedBadges` | `5` | How many badges can be pinned at once (1 to 12). |
| `PinnedBadges` | empty | The pins, edited from the pause menu. |

## How badges are judged

Every rule comes from the game's own code and scene data (game version 2.4.c). When nothing in the
game can rule a badge out, the tracker never shows a red cross for it.

| Kind | Badges | Red cross when |
| --- | --- | --- |
| Run counter | Knot Tying, Clutch, Plunderer, First Aid, Jester, Archery, Foraging, Mycology, Advanced Mycology, Gourmand | never |
| Lifetime counter | Cooking, Happy Camper, Bouldering, Toxicology, Ascender, Bookworm, Calcium Intake | never |
| Clean run | Balloon, Naturalist, Survivalist, Leave No Trace, Lone Wolf, Speed Climber | the condition breaks |
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
