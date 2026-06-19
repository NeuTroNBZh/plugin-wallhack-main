# CS2 Wallhack Plugin

A Counter-Strike 2 server-side plugin built on [CounterStrikeSharp](https://docs.cssharp.dev/) providing wallhack, invisibility, and admin commands for private/custom servers.

> **Original plugin:** [labaland/plugin-wallhack](https://github.com/labaland/plugin-wallhack)  
> **Maintained by:** [NeuTroNBZh](https://github.com/NeuTroNBZh)

---

## Changes from the original

- Reworked **Wallhack** — glow entities per player, selectively transmitted via `OnCheckTransmit`; per-team glow color (T and CT separate colors)
- Reworked **Invisibility** — permanent alpha 0 + transmit blocking; player and weapons fully hidden regardless of movement or actions
- Invisible players no longer cast shadows or expose weapon models to other players
- **New — Reveal-on-move invisibility:** optional mode (`InvisibleRevealOnMove` config key) — invisible while still, but revealed with a fade-out and a centered HUD gauge when moving, shooting, reloading, taking damage, planting or defusing
- Fixed **RCON** command
- Fixed multiple server crash bugs (`WriteEnterPVS: GetEntServerClass failed`)
- Improved command handling: aliases, partial name matching, permission validation, self-toggle (no argument = applies to yourself)
- Silent mode: when [CS2-SimpleAdmin](https://github.com/daffyyyy/CS2-SimpleAdmin) is installed, commands respect the admin's silent mode — confirmations are not shown to the target player
- **New — HP:** set any player's HP on demand, with preset options in the SimpleAdmin menu
- **New — Infinite Money:** grants $65 535 permanently, auto-refilled on every purchase, spawn and round start
- **New — God mode:** invincibility toggle, health restored every tick
- **New — Speed boost:** configurable multiplier (default 1.5×), properly reset on toggle
- **New — Slay / Slap:** kill or deal damage to a player
- **New — Status:** view all active privileges for a player
- **New — Reset All:** remove every privilege from every player at once
- **New — CS2-SimpleAdmin integration:** a "Wallhack Plugin" category is automatically added to the `!admin` menu when SimpleAdmin is present

---

## Requirements

- A CS2 dedicated server
- [CounterStrikeSharp](https://docs.cssharp.dev/docs/guides/getting-started.html) installed
- *(Optional)* [CS2-SimpleAdmin](https://github.com/daffyyyy/CS2-SimpleAdmin) for the admin menu integration

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
| `@css/generic` | All commands below except `!rcon` |
| `@css/rcon` | `!rcon` |

---

## Commands

All player-targeting commands accept an optional `<player>` argument. If omitted, the command applies to **yourself**.

### Wallhack

```
!wh [player]
!wallhack [player]
```

Toggles a glowing outline through walls for the target's enemies. Run again to remove.

---

### Invisibility

```
!invis [player]
!invisible [player]
```

Toggles full invisibility — alpha 0 + entity hidden from all other players' transmit list. Permanent: the player stays invisible regardless of movement, shooting, or any action.

Two modes (server-side, via the `InvisibleRevealOnMove` config key):
- **Permanent (default):** the player stays fully invisible regardless of any action.
- **Reveal on move:** invisible while still, but momentarily revealed (fade-out) when making noise, shooting, reloading, taking damage, planting or defusing — with a centered HTML gauge showing the current visibility level.

---

### God mode

```
!god [player]
```

Toggles invincibility. Health is restored every server tick, preventing any lethal hit.

---

### Speed boost

```
!speed [player]              → toggle 1.5× speed
!speed <multiplier>          → set a custom multiplier for yourself (0.1–10)
!speed <multiplier> [player] → set a custom multiplier for a player
```

Running `!speed [player]` again when they already have speed removes it and immediately resets their movement speed.

---

### HP

```
!hp <amount> [player]
```

Sets HP to the specified value immediately. If no player is given, applies to yourself. Values above the default max health also raise the max health bar.

Examples: `!hp 150` · `!hp 300 PlayerName`

---

### Slay

```
!slay <player>
```

Kills the target player instantly.

---

### Slap

```
!slap <damage> <player>
```

Deals the specified amount of damage. Kills if damage exceeds current health.

---

### Infinite Money

```
!infmoney <player>
```

Toggles permanent $65 535. Money is restored after every purchase, spawn, and round start. Removed on disconnect or server restart.

---

### Money (one-time)

```
!money <amount> <player>
```

Sets the target player's money once.

---

### Status

```
!status <player>
```

Displays all currently active privileges for the target player (wallhack, invisible, god, speed, infinite money).

---

### Reset All

```
!resetall
```

Removes every privilege from every player at once. Broadcasts to all players (suppressed if SimpleAdmin silent mode is active).

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

## CS2-SimpleAdmin integration

When [CS2-SimpleAdmin](https://github.com/daffyyyy/CS2-SimpleAdmin) is installed alongside this plugin, a **"Wallhack Plugin"** category is automatically added to the `!admin` menu with the following entries:

| Menu entry | Action |
|---|---|
| Wallhack | Player picker → toggle wallhack |
| Invisible | Player picker → toggle invisibility |
| God Mode | Player picker → toggle god mode |
| Speed Boost | Player picker → toggle speed (1.5×) |
| Set HP | HP preset (100 / 150 / 200 / 250 / 300) → player picker |

No extra configuration needed — the integration is detected automatically at startup.

**Silent mode:** if an admin has SimpleAdmin's silent mode active, command confirmations are only shown to the admin, not to the affected player.

---

## Configuration

Auto-generated at first launch:

```
csgo/addons/counterstrikesharp/configs/plugins/WallhackPluginCS2/WallhackPluginCS2.json
```

```json
{
  "ColorR": 255,
  "ColorG": 100,
  "ColorB": 0,
  "ColorCT_R": 0,
  "ColorCT_G": 100,
  "ColorCT_B": 255,
  "CommandPermission": "@css/generic",
  "RconPermission": "@css/rcon",
  "WallhackEnabled": true,
  "InvisibleEnabled": true,
  "InfiniteMoneyEnabled": true,
  "InvisibleRevealOnMove": false,
  "ConfigVersion": 1
}
```

| Key | Description |
|---|---|
| `ColorR/G/B` | RGB glow color for **Terrorist** players |
| `ColorCT_R/G/B` | RGB glow color for **Counter-Terrorist** players |
| `CommandPermission` | Permission flag required for most commands |
| `RconPermission` | Permission flag required for `!rcon` |
| `WallhackEnabled` | Enable or disable the wallhack feature entirely |
| `InvisibleEnabled` | Enable or disable the invisibility feature entirely |
| `InfiniteMoneyEnabled` | Enable or disable the infinite money feature entirely |
| `InvisibleRevealOnMove` | Mode d'invisibilité. `false` (défaut) = invisibilité permanente. `true` = invisible à l'arrêt, révélé en fondu au mouvement / tir / rechargement / dégâts / plant-defuse, avec une jauge HTML centrée indiquant le niveau de visibilité. |

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
