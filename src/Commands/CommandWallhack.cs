using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Funnies.Modules;
using Funnies.Models;

namespace Funnies.Commands;

public class CommandWallhack
{
    public static void OnWallhackCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (caller == null || !Util.IsPlayerValid(caller))
            return;

        var target = caller; // you can extend later for other players

        // ✅ Prevent duplicate glow
        if (Globals.GlowData.ContainsKey(target))
        {
            Console.WriteLine("[WH DEBUG] Glow already exists, skipping command");
        }
        else
        {
            Console.WriteLine("[WH DEBUG] Command triggered glow creation");

            // safe delayed creation
            Server.NextWorldUpdate(() =>
            {
                Server.NextWorldUpdate(() =>
                {
                    Server.NextWorldUpdate(() =>
                    {
                        if (!Globals.GlowData.ContainsKey(target))
                        {
                            _ = typeof(Wallhack);
                        }
                    });
                });
            });
        }

        // ✅ Toggle wallhack ability
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