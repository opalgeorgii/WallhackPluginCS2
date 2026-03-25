using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace WallhackPlugin.Commands;

public class CommandInvisible
{
    public static void OnInvisibleCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (caller == null || !caller.IsValid)
            return;

        if (!AdminManager.PlayerHasPermissions(caller, Globals.Config.AdminPermission))
        {
            Util.ServerPrintToChat(caller, "You do not have permission to use this command.");
            return;
        }

        var player = Util.GetPlayerByName(command.ArgString);
        if (player == null)
        {
            if (Util.IsPlayerValid(caller))
                Util.ServerPrintToChat(caller, $"Player {command.ArgString} not found");
            return;
        }

        bool wasInvisible = Globals.InvisiblePlayers.Remove(player);
        var pawn = player.PlayerPawn?.Value;

        if (pawn != null)
        {
            if (wasInvisible)
            {
                pawn.Render = Color.FromArgb(255, pawn.Render);
                Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");

                pawn.ShadowStrength = 1.0f;
                Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_flShadowStrength");
            }

            if (pawn.WeaponServices != null)
            {
                foreach (var weapon in pawn.WeaponServices.MyWeapons)
                {
                    if (weapon.Value != null)
                    {
                        weapon.Value.Render = pawn.Render;
                        Utilities.SetStateChanged(weapon.Value, "CBaseModelEntity", "m_clrRender");

                        weapon.Value.ShadowStrength = 1.0f;
                        Utilities.SetStateChanged(weapon.Value, "CBaseModelEntity", "m_flShadowStrength");
                    }
                }
            }
        }

        if (!wasInvisible)
            Globals.InvisiblePlayers.Add(player, new());

        if (Util.IsPlayerValid(caller))
        {
            string status = wasInvisible ? "now visible" : "now invisible";
            Util.ServerPrintToChat(caller, $"{player.PlayerName} is {status}");
        }
    }
}