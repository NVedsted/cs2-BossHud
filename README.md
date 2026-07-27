# BossHud

> :warning: This is still in early development pending broader testing.

A CounterStrikeSharp plugin implementing support for HP bars for bosses primarily intended for the Zombie Escape (ZE) game-mode.

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

## Credits

- Community & code bases of [Source2ZE](https://github.com/Source2ZE) have been of immense help answering ad-hoc questions and generally do awesome work for CS2!
- [SRCDSLab](https://github.com/srcdslab) & their BossHP/HUD plugins provided insights into how it was done in SourceMod.
- [GFL](https://github.com/gflze/CS2-ZE-Configs) sharing their configs with the broader community is awesome on its own and these configs serve as the baseline for testing of configuration support.