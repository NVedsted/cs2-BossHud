# BossHud

> :warning: This is still in early development pending broader testing.

A CounterStrikeSharp plugin implementing support for HP bars for bosses primarily intended for the Zombie Escape (ZE)
game-mode.

## Features

- Show HP bars for bosses
- Player damage statistics
- Supports the configuration convention used by the broader ZE community.

## Prerequisites

- CounterStrikeSharp 1.0.370 or newer

## Installation

1. Download the latest release.
2. Extract files into your server files
3. Restart the server or reload the plugin.
4. Put map configurations into `addons/counterstrikesharp/configs/plugins/BossHud/maps/<map name>.jsonc`

## Known limitations

- `hurttrigger` is currently not supported.
- Limited global configuration options
- Hitmarker support is disabled pending changes to existing plugin.
- It does not play nicely with other plugins trying to display centered messages.
- Not possible to reload map configs without reloading the plugin or changing map.

## Custom HUD

Custom HUDs can be implemented as [described on VDC](https://developer.valvesoftware.com/wiki/Panorama/Overview). The
plugin will control the HUD by adding/removing classes & setting dialog variables as described below. Each custom HUD
must define how many bosses it can show & how many HP stages it requires for rendering, e.g., gradients or HP bars. The
custom HUD is expected to contain a [Boss Container](#boss-container) panel and a [Boss Entry](#boss-entry) panel for
each boss that it can show concurrently.

See [`examples/custom-hud`](examples/custom-hud) for an example & suggested setup for creating/debugging custom HUDs.

TODO: Add remaining settings

| Setting     | Required? | Description                                                      | Example |
|-------------|-----------|------------------------------------------------------------------|---------|
| `MaxBosses` | Yes       | The max amount of bosses the HUD can show at once.               | `5`     |
| `HpStages`  | No        | The amount of HP stages the UI supports, e.g., to style HP bars. | `100`   |

### Boss Container

**Panel ID:** `bosses`

The container panel carries global information about the state of BossHud such as whether any bosses are active. It is
recommended to put boss entries inside this container.

| Class       | Description                   |
|-------------|-------------------------------|
| `active`    | Any boss is active            |
| `single`    | Only a single boss is active  |
| `multi`     | Multiple bosses are active    |
| `overflown` | Not all bosses could be shown |

| String            | Description                                | Example  |
|-------------------|--------------------------------------------|----------|
| `not_shown_count` | Amount of active bosses that are not shown | `2`, `0` |

### Boss entry

**Panel ID:** `boss-<index>` (index starts at 0)

Each boss entry represents a slot where information about an active boss can be stored. An entry should not be visible
unless it is active. It is recommended to nest this under the container.

Each boss has an HP stage which is a number from 0 to the configured HP stages (exclusive); its value is based on the HP
percentage of the boss. This is communicated through stage classes that can then be used to style, e.g., gradients, HP
bars, etc. Be aware that only one stage class is active at a time. E.g., if the custom HUD is configured to have 5
stages, `hp_stage_4` will be applied at full health, `hp_stage_2` at ~50%, and `hp_stage_0` when it is close to 0%. The
higher the stage count, the more detail can be applied at the cost of more CSS classes.

> **Tip:** Favor using CSS transitions to smoothen the HUD over ramping stage counts extremely high.

| Class          | Description                                                  |
|----------------|--------------------------------------------------------------|
| `active`       | Boss entry is active                                         |
| `hp_stage_<n>` | The HP stage the boss is in. See details above.              |
| `named`        | Boss has a name (string `name` will be empty if not present) |
| `hit`          | Boss is hit-based                                            |
| `hp`           | Boss is HP-based                                             |

| String               | Description                                                        | Example           |
|----------------------|--------------------------------------------------------------------|-------------------|
| `name`               | Name of boss if present (class `named` will be applied if present) | `Big Monster`, `` |
| `current_hp`         | Current HP of the boss                                             | `279`             |
| `max_hp`             | Max HP of the boss                                                 | `1000`            |
| `percentage_rounded` | HP as a percentage rounded to nearest integer (no %-sign included) | `57`              |

## Credits

- Community & code bases of [Source2ZE](https://github.com/Source2ZE) have been of immense help answering ad-hoc
  questions and generally do awesome work for CS2!
- [SRCDSLab](https://github.com/srcdslab) & their BossHP/HUD plugins provided insights into how it was done in
  SourceMod.
- [GFL](https://github.com/gflze/CS2-ZE-Configs) sharing their configs with the broader community is awesome on its own
  and these configs serve as the baseline for testing of configuration support.