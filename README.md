# Wallhack Plugin 2026

[![Donate](https://img.shields.io/badge/Support-Donatello-blue)](https://donatello.to/opalgeorgii)

## Overview

This plugin recreates the **1 vs 5 wallhack** and **Invisible** style gameplay for **Counter-Strike 2**, inspired by videos from **dima_wallhacks**.

This project was heavily inspired by the original [FunnyPlugin](https://github.com/robieless/FunnyPlugin), but it was significantly reworked and fixed so that **wallhack** and **invisibility** work as intended, **RCON** works properly, and the overall plugin is more stable, cleaner, and easier to use.

For a full CS2 server installation guide, please visit my other repository: [CS2ConfigCopier](https://github.com/opalgeorgii/CS2ConfigCopier)

---

## Main improvements and fixes

Compared to the original inspiration, this version includes major fixes and reworks such as:

- fixed and reworked **Wallhack**
- fixed and reworked **Invisibility**
- invisible players no longer cast their player-model shadows for other players
- invisible players no longer expose their world weapons, grenades, knives, or related world rendering to other players
- wallhack correctly reveals invisible enemies only for a limited time when they make a sound
- fixed **RCON**, which previously was not working correctly
- fixed multiple bugs that could lead to **server crashes**
- improved command handling with:
  - command aliases
  - partial name matching
  - better permission handling
- improved overall structure, stability, and usability

---

## Installation

1. Install [CounterStrikeSharp](https://docs.cssharp.dev/docs/guides/getting-started.html) on your server.
2. Download this plugin from the **Releases** page.
3. Put the plugin files into:

```text
server/game/csgo/addons/counterstrikesharp/plugins/WallhackPluginCS2
```

4. If the `WallhackPluginCS2` folder does not exist, create it manually.
5. Launch the server once so the plugin generates its config.

For a full server installation and setup guide, please visit: [CS2ConfigCopier](https://github.com/opalgeorgii/CS2ConfigCopier)

---

## Admin setup

To use the commands, add the player as an admin in:

```text
server/game/csgo/addons/counterstrikesharp/configs/admins.json
```

Example:

```json
{
  "playername": {
    "identity": "steamid",
    "flags": [
      "@css/generic",
      "@css/rcon"
    ]
  }
}
```

### Permission notes

By default:

- `@css/generic` is required for:
  - `!wh`
  - `!wallhack`
  - `!invis`
  - `!invisible`
  - `!money`

- `@css/rcon` is required for:
  - `!rcon`

You can change these permission strings later in the plugin config without recompiling the code.

---

## Commands

### Wallhack

- `!wh <playername>`
- `!wallhack <playername>`

You can also use a **partial player name**.

In many cases, the **first letter is enough** if it uniquely matches one player.

Example:

```text
!wh a
```

If multiple players match the same partial name, type more letters until it becomes unique.

---

### Invisibility

- `!invis <playername>`
- `!invisible <playername>`

You can also use a **partial player name** the same way as wallhack.

Example:

```text
!invis av
```

---

### Money

- `!money <amount> <playername>`

Partial player names work here too.

Example:

```text
!money 16000 ava
```

---

### RCON

- `!rcon <command>`

Example:

```text
!rcon mp_warmup_end
```

---

## Configuration

After the first launch, the plugin creates its config here:

```text
server/game/csgo/addons/counterstrikesharp/configs/plugins/WallhackPluginCS2/WallhackPluginCS2.json
```

Default example:

```json
{
  "ColorR": 255,
  "ColorG": 0,
  "ColorB": 128,
  "CommandPermission": "@css/generic",
  "RconPermission": "@css/rcon",
  "WallhackEnabled": true,
  "InvisibleEnabled": true,
  "ConfigVersion": 1
}
```

### Glow color

You can change the wallhack glow color by editing:

- `ColorR`
- `ColorG`
- `ColorB`

You do **not** need to recompile the code to change these values.

---

## Notes

- The plugin supports partial name matching for player-based commands.
- If a partial name matches more than one player, be more specific.
- The target-name HUD text (`Enemy: <name>`) is controlled client-side.
- To hide that text, the player needs to disable it manually in the client console with:

```text
hud_showtargetid 0
```

---

## Support

If you wish to support me, you can donate here:

**[Donate via Donatello](https://donatello.to/opalgeorgii)**

---

## Credits

- Original inspiration: [robieless/FunnyPlugin](https://github.com/robieless/FunnyPlugin)
- Server setup guide: [opalgeorgii/CS2ConfigCopier](https://github.com/opalgeorgii/CS2ConfigCopier)

---

## Contact

If you find bugs or want to suggest improvements, open an issue in the repository.
