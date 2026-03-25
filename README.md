# Wallhack Plugin

[![Donate](https://img.shields.io/badge/Support-Donatello-blue)](https://donatello.to/opalgeorgii)

## Overview

This plugin recreates the **1v5 wallhack** and **Invisible Man** style gameplay for **Counter-Strike 2**, inspired by videos from **dima_wallhacks** and **renyan**.

This project was heavily inspired by the original [FunnyPlugin](https://github.com/robieless/FunnyPlugin), but it was reworked and fixed to improve stability, command handling, rendering behavior, permissions, and overall usability.

For a full CS2 server installation and setup guide, see: [autoconfigcopier](https://github.com/opalgeorgii/autoconfigcopier)

---

## Main improvements and fixes

Compared to the original inspiration, this version includes major fixes and reworks such as:

- fixed **RCON**, which previously was not working correctly
- fixed multiple bugs that could lead to **server crashes**
- improved **invisibility rendering**
- fixed cases where invisible players still showed:
  - world shadows
  - weapons
  - grenades
  - knives
- improved command usability with:
  - command aliases
  - partial name matching
  - better admin permission handling
- improved general plugin structure and behavior

---

## Installation

1. Install [CounterStrikeSharp](https://docs.cssharp.dev/docs/guides/getting-started.html) on your server.
2. Download this plugin from the **Releases** page.
3. Put the plugin files into:

```text
server/game/csgo/addons/counterstrikesharp/plugins/WallhackPlugin
```

4. Launch the server once so the plugin generates its config.

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
server/game/csgo/addons/counterstrikesharp/configs/plugins/WallhackPlugin/WallhackPlugin.json
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
- The target-name HUD text (`Enemy: <name>`) is controlled client-side, so players who want it hidden need to disable it on their own client.

---

## Support

If you wish to support me, you can donate here:

**[Donate via Donatello](https://donatello.to/opalgeorgii)**

---

## Credits

- Original inspiration: [robieless/FunnyPlugin](https://github.com/robieless/FunnyPlugin)
- Server setup guide: [opalgeorgii/autoconfigcopier](https://github.com/opalgeorgii/autoconfigcopier)

---

## Contact

If you find bugs or want to suggest improvements, open an issue in the repository.
