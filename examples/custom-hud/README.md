# Custom HUD Example

This directory contains a bare-bones example of how a custom HUD can be implemented. Read detailed descriptions of how
this works [here](../../README.md#custom-hud).

**Files:**

- **`boss_hud_example.xml`:** Example layout file to be put into `panorama\layout\custom_game`.
- **`boss_hud_example.css`:** Example stylesheet to be put into `panorama\styles\custom_game`
- **`bosshud_debugger.js`:** `cs_script` for debugging BossHud custom HUDs; see more below.

## Debugger

To rapidly prototype and iterate on custom HUDs for BossHud, the CS2 Workshop Tools can be used in combination with the
files in this folder. This also provides live-editing to instantly visualize changes.

**Setup steps:**

1. Launch CS2 Workshop Tools & create/enter a custom addon.
2. Move the files of this directory into the custom addon.
3. Create or re-use a basic map.
4. Create a `custom_hud_layout` entity with the name `boss_hud` and pick your layout.
5. Create a `point_script` entity and pick the debugger script.
6. Build & run the map
7. The HUD should now render and you can live-edit the files (including the debugger script).
