using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
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
            Util.ServerPrintToChat(caller, "Usage: !wh <player_name> | !wallhack <player_name>");
            return;
        }

        string targetName = command.ArgString.Trim();

        var matchingPlayers = Util.GetPlayersByPartialName(targetName)
            .Where(p => !p.IsBot)
            .ToList();

        if (matchingPlayers.Count == 0)
        {
            Util.ServerPrintToChat(caller, $"No valid player found matching '{targetName}'.");
            return;
        }

        if (matchingPlayers.Count > 1)
        {
            string names = string.Join(", ", matchingPlayers.Select(p => p.PlayerName));
            Util.ServerPrintToChat(caller, $"Multiple matches: {names}. Be more specific.");
            return;
        }

        var target = matchingPlayers[0];

        if (Globals.Wallhackers.Contains(target))
        {
            Globals.Wallhackers.Remove(target);
            Util.ServerPrintToChat(caller, $"Wallhack OFF for {target.PlayerName}");
        }
        else
        {
            Globals.Wallhackers.Add(target);
            Util.ServerPrintToChat(caller, $"Wallhack ON for {target.PlayerName}");
        }
    }
}