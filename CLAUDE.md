# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

A Counter-Strike 2 server plugin built on [CounterStrikeSharp](https://docs.cssharp.dev/) that provides wallhack and invisibility features for players with admin permissions.

## Build Commands

```bash
dotnet build               # Debug build
dotnet build -c Release    # Release build
dotnet clean               # Clean artifacts
```

The output is a `.dll` deployed to the CS2 server under `addons/counterstrikesharp/plugins/WallhackPluginCS2/`. There are no automated tests; validation requires a live CS2 server running CounterStrikeSharp.

## Architecture

The plugin is a single C# library (`net8.0`) with one external dependency: `CounterStrikeSharp.API` (v1.0.364).

### Entry Point & Global State

**`WallhackPluginCS2Core.cs`** — inherits `BasePlugin` and `IPluginConfig<WallhackConfig>`. Registers all listeners/commands on `Load()` and cleanly tears them down on `Unload()` (hot-reload safe).

**`Globals.cs`** — central mutable state:
- `Wallhackers`: `HashSet` of players with wallhack active
- `GlowData`: `Dictionary<player, GlowData>` — paired prop entities for glow rendering
- `InvisiblePlayers`: `Dictionary<player, SoundData>` — fade timing state per invisible player

### Two Feature Modules

**`Modules/Wallhack.cs`** — Glow-through-walls:
- On player spawn, creates two paired `CDynamicProp` entities per enemy: a hidden `ModelRelay` (position anchor) and a visible `GlowEnt` that follows it.
- Uses the `OnCheckTransmit` listener to selectively transmit glow entities only to wallhackers looking at enemies (not teammates).
- When an invisible enemy makes noise or reloads, sets a `RevealUntil` timestamp so wallhackers temporarily see their glow.

**`Modules/Invisible.cs`** — Per-tick alpha blending:
- Each server tick: reads `SoundData.StartTime`/`EndTime` to compute a linear alpha curve (stays at 255 for the first half of the fade window, then interpolates to 0).
- Applies computed alpha to the player pawn and all equipped weapons.
- Uses `OnCheckTransmit` to remove the pawn and weapon entities from the transmit list for non-owner viewers.
- Action events (shoot, sound, reload, damage, plant/defuse) each trigger a new fade sequence by updating `SoundData`.

### Commands

All commands validate `@css/generic` (or `@css/rcon` for RCON) and use `Util.TryResolveSinglePlayer()` for fuzzy name matching.

| Command | File | Effect |
|---|---|---|
| `!wh <player>` | `Commands/CommandWallhack.cs` | Toggle wallhack |
| `!invis <player>` | `Commands/CommandInvisible.cs` | Toggle invisibility |
| `!money <amount> <player>` | `Commands/CommandMoney.cs` | Set player money |
| `!rcon <command>` | `Commands/CommandRcon.cs` | Execute server command (blocks `quit`/`exit`/`restart`) |

### Key Utilities (`Util.cs`)

- `FindPlayerMatches(query)` — four-pass fuzzy match: exact → starts-with → word-starts → contains
- `Map(value, inMin, inMax, outMin, outMax)` — linear interpolation used for alpha fades
- `IsPlayerValid()` / `IsPlayerEntityValid()` — entity validity checks used throughout

## Configuration

Auto-generated JSON on first launch at `configs/plugins/WallhackPluginCS2/WallhackPluginCS2.json`:

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

## Key Design Constraints

- **`OnCheckTransmit` is the visibility control mechanism** — it runs per-player per-entity per frame; keep logic inside it minimal and allocation-free.
- **Hot-reload**: `Load(hotReload: true)` must be safe. All cleanup (entity removal, alpha reset, dictionary clears) happens in `Unload()`.
- **Entity following**: Glow entities use the `FollowEntity` input rather than per-tick position updates.
- **Reload reveal**: When an invisible player reloads, `InReload` on the active weapon triggers a `RevealUntil` window computed from `VData` reload duration.
