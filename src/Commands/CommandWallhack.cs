using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace Funnies.Commands;

public class CommandWallhack
{
    public static void OnWallhackCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (caller == null || !Util.IsPlayerValid(caller))
            return;

        var target = caller;

        if (Globals.Wallhackers.Contains(target))
        {
            Globals.Wallhackers.Remove(target);
            caller.PrintToChat("Wallhack OFF");
        }
        else
        {
            Globals.Wallhackers.Add(target);
            caller.PrintToChat("Wallhack ON");
        }
    }
}
