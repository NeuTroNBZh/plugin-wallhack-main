# CS2 Wallhack Plugin

A Counter-Strike 2 server-side plugin built on [CounterStrikeSharp](https://docs.cssharp.dev/) providing wallhack, invisibility, and fun admin commands for private/custom servers.

> **Original plugin:** [labaland/plugin-wallhack](https://github.com/labaland/plugin-wallhack)  
> **Maintained by:** [NeuTroNBZh](https://github.com/NeuTroNBZh)

---

## Changes from the original

- Reworked **Wallhack** — glow entities created per player, selectively transmitted via `OnCheckTransmit`
- Reworked **Invisibility** — per-tick alpha blending, proper weapon and shadow hiding
- Invisible players no longer cast shadows or expose weapon models to other players
- Wallhack temporarily reveals invisible enemies when they make noise (shoot, reload, plant/defuse)
- Fixed **RCON** command
- Fixed multiple server crash bugs
- Better command handling: aliases, partial name matching, permission validation
- **New — Infinite Money:** grants $65 535 permanently to a player, auto-refilled on every purchase, spawn, and round start, until removed by an admin or the server restarts

---

## Requirements

- A CS2 dedicated server
- [CounterStrikeSharp](https://docs.cssharp.dev/docs/guides/getting-started.html) installed

---

## Installation

1. Download the latest release from the **[Releases](../../releases)** page.
2. Copy the plugin folder to:

```
csgo/addons/counterstrikesharp/plugins/WallhackPluginCS2/
```

3. Start the server once — the plugin generates its config automatically.

---

## Admin setup

Add admins in `csgo/addons/counterstrikesharp/configs/admins.json`:

```json
{
  "YourName": {
    "identity": "STEAM_0:0:XXXXXXXX",
    "flags": ["@css/generic", "@css/rcon"]
  }
}
```

**Permission requirements:**

| Flag | Commands |
|---|---|
| `@css/generic` | `!wh`, `!wallhack`, `!invis`, `!invisible`, `!money`, `!infmoney` |
| `@css/rcon` | `!rcon` |

Both permission strings are configurable in the plugin config — no recompile needed.

---

## Commands

### Wallhack

```
!wh <player>
!wallhack <player>
```

Toggles a glowing outline through walls on the target player's enemies. Run again to remove it.

---

### Invisibility

```
!invis <player>
!invisible <player>
```

Toggles invisibility for the target player. The player briefly reappears to wallhackers when they shoot, reload, or make noise.

---

### Infinite Money

```
!infmoney <player>
```

Toggles permanent $65 535 for the target player. Money is automatically restored after every purchase, on every spawn, and at every round start. Run again to remove the privilege. The privilege is also removed when the player disconnects or the server restarts.

---

### Money (one-time)

```
!money <amount> <player>
```

Sets the target player's money to the specified amount once.

---

### RCON

```
!rcon <command>
```

Executes a server console command. Blocked commands: `quit`, `exit`, `restart`.

---

### Partial name matching

All player-targeting commands accept partial names. Matching priority:

1. Exact name
2. Starts with query
3. A word in the name starts with query
4. Name contains query

If multiple players match, the command lists them and asks you to be more specific.

---

## Configuration

Auto-generated at first launch:

```
csgo/addons/counterstrikesharp/configs/plugins/WallhackPluginCS2/WallhackPluginCS2.json
```

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

| Key | Description |
|---|---|
| `ColorR` / `ColorG` / `ColorB` | RGB color of the wallhack glow (0–255) |
| `CommandPermission` | Permission flag required for most commands |
| `RconPermission` | Permission flag required for `!rcon` |
| `WallhackEnabled` | Enable or disable the wallhack feature entirely |
| `InvisibleEnabled` | Enable or disable the invisibility feature entirely |

---

## Building from source

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
dotnet build WallhackPluginCS2.csproj -c Release
```

Output: `bin/Release/net8.0/WallhackPluginCS2.dll`

---

## Credits

- **Original plugin:** [labaland](https://github.com/labaland/plugin-wallhack)
- **Maintained & improved by:** [NeuTroNBZh](https://github.com/NeuTroNBZh)
