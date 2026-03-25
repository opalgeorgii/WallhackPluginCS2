# Wallhack Plugin

## Overview

This plugin recreates the **1v5 with wallhack** and **Invisible Man** style gameplay for **Counter-Strike 2**, inspired by videos from **dima_wallhacks** and **renyan**.

This code was heavily inspired by the original [FunnyPlugin](https://github.com/robieless/FunnyPlugin), but it was **reworked, fixed, and cleaned up** so the main features work much more smoothly in practice.

For a full CS2 server installation guide, see: [autoconfigcopier](https://github.com/opalgeorgii/autoconfigcopier)

---

## What was fixed and reworked

Compared to the original inspiration, this version includes major fixes and improvements:

- fixed **RCON**, which previously did not work correctly
- fixed multiple bugs that could lead to **server crashes**
- improved **invisibility rendering and behavior**
- fixed cases where invisible players still showed:
  - shadows
  - weapons
  - grenades
  - knives
- improved command handling and usability
- added command aliases
- added partial player name matching
- improved permission handling
- reduced unnecessary overhead and improved general plugin behavior

---

## Installation

1. Install [CounterStrikeSharp](https://docs.cssharp.dev/docs/guides/getting-started.html) on your server.
2. Download the plugin from the **Releases** page of this repository.
3. Place it in:

```text
server/game/csgo/addons/counterstrikesharp/plugins/WallhackPlugin
```

4. Launch the server once so the plugin generates its config file.

---

## Admin setup

To use the commands, add players as admins in:

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
If 2 or more players match, type more letters until the name becomes unique.

Examples:

```text
!wh a
!wallhack ava
```

### Invisibility

- `!invis <playername>`
- `!invisible <playername>`

Partial player names work here too.

Examples:

```text
!invis a
!invisible ava
```

### Money

- `!money <amount> <playername>`

Partial player names work here too.

Example:

```text
!money 16000 ava
```

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

- Player-based commands support partial name matching.
- If multiple players match the same partial name, enter more letters.
- The `Enemy: <name>` target-ID text is controlled client-side. Players who want it hidden need to disable it on their own client.
- The current wallhack implementation works, but on some setups the helper-entity method may still print engine assertions in the server console when players spawn or respawn.

---

## Support the project

If this repository helped you and you want to support development, you can add any of these links here later:

- **Monobank Jar:** `https://send.monobank.ua/jar/YOUR_JAR_ID`
- **Patreon:** `https://patreon.com/YOUR_NAME`
- **WayForPay payment link/page:** `YOUR_LINK_HERE`
- **LiqPay payment link/page:** `YOUR_LINK_HERE`
- **Fondy payment link/page:** `YOUR_LINK_HERE`

---

## Credits

- Original inspiration: [robieless/FunnyPlugin](https://github.com/robieless/FunnyPlugin)
- Server setup guide: [opalgeorgii/autoconfigcopier](https://github.com/opalgeorgii/autoconfigcopier)

---

## Contact

If you find bugs or want to suggest improvements, open an issue in this repository.
