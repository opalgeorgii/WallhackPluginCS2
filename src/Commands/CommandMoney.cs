using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace Funnies.Commands;

public class CommandMoney
{
    public static void OnMoneyCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (caller == null || !caller.IsValid)
            return;

        if (!AdminManager.PlayerHasPermissions(caller, Globals.Config.AdminPermission))
        {
            Util.ServerPrintToChat(caller, "You do not have permission to use this command.");
            return;
        }

        if (string.IsNullOrWhiteSpace(command.ArgString))
        {
            Util.ServerPrintToChat(caller!, "Usage: !css_money <amount> <player>");
            return;
        }

        var args = command.ArgString.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (args.Length < 2 || !int.TryParse(args[0], out int money))
        {
            Util.ServerPrintToChat(caller!, "Usage: !css_money <amount> <player>");
            return;
        }

        string name = args[1];
        var player = Util.GetPlayerByName(name);

        if (player == null)
        {
            Util.ServerPrintToChat(caller!, $"Player {name} not found");
            return;
        }

        if (player.InGameMoneyServices == null)
        {
            Util.ServerPrintToChat(caller!, $"Cannot modify money for {player.PlayerName}");
            return;
        }

        player.InGameMoneyServices.Account = money;
        Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");

        if (Util.IsPlayerValid(caller))
            Util.ServerPrintToChat(caller!, $"Set {player.PlayerName}'s money to ${money}");

        Console.WriteLine($"{caller?.PlayerName ?? "Console"} set {player.PlayerName}'s money to ${money}");
    }
}