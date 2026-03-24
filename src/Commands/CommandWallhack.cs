using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;

namespace Funnies.Commands;

public class CommandWallhack
{
    public static void OnWallhackCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (caller == null || !caller.IsValid)
            return;

        if (string.IsNullOrWhiteSpace(command.ArgString))
        {
            caller.PrintToChat("Usage: !wh <name>");
            return;
        }

        string targetName = command.ArgString.Trim();

        CCSPlayerController? target = null;

        foreach (var player in Utilities.GetPlayers())
        {
            if (player == null || !player.IsValid)
                continue;

            if (player.PlayerName.Contains(targetName, StringComparison.OrdinalIgnoreCase))
            {
                target = player;
                break;
            }
        }

        if (target == null)
        {
            caller.PrintToChat($"Player not found: {targetName}");
            return;
        }

        // ✅ Toggle wallhack
        if (Globals.Wallhackers.Contains(target))
        {
            Globals.Wallhackers.Remove(target);
            caller.PrintToChat($"Wallhack OFF for {target.PlayerName}");
        }
        else
        {
            Globals.Wallhackers.Add(target);
            caller.PrintToChat($"Wallhack ON for {target.PlayerName}");
        }
    }
}