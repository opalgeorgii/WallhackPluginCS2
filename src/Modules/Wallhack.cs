using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using WallhackPlugin.Commands;
using WallhackPlugin.Models;

namespace WallhackPlugin.Modules;

public class Wallhack
{
    public static void OnPlayerTransmit(CCheckTransmitInfo info, CCSPlayerController player)
    {
        if (!Util.IsPlayerValid(player))
            return;

        foreach (var (target, data) in Globals.GlowData.ToList())
        {
            if (!Util.IsPlayerValid(target) || !data.GlowEnt.IsValid || !data.ModelRelay.IsValid)
            {
                Globals.GlowData.Remove(target);
                continue;
            }

            if (target.Slot == player.Slot)
            {
                info.TransmitEntities.Remove(data.ModelRelay);
                info.TransmitEntities.Remove(data.GlowEnt);
                continue;
            }

            bool isInvisible = Globals.InvisiblePlayers.ContainsKey(target);
            bool isRevealed = false;

            if (isInvisible && Globals.InvisiblePlayers.TryGetValue(target, out var invisData))
                isRevealed = Server.CurrentTime <= invisData.RevealUntil;

            bool shouldSee =
                Globals.Wallhackers.Contains(player) &&
                target.Team != player.Team &&
                player.Team != CsTeam.Spectator &&
                target.Team != CsTeam.Spectator &&
                (!isInvisible || isRevealed);

            if (shouldSee)
            {
                info.TransmitEntities.Add(data.ModelRelay);
                info.TransmitEntities.Add(data.GlowEnt);
            }
            else
            {
                info.TransmitEntities.Remove(data.ModelRelay);
                info.TransmitEntities.Remove(data.GlowEnt);
            }
        }
    }

    public static HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!Util.IsPlayerValid(player))
            return HookResult.Continue;

        RemoveGlow(player);

        Server.NextFrame(() =>
        {
            if (!Util.IsPlayerValid(player) || !player.PawnIsAlive) return;
            Glow(player);
        });

        return HookResult.Continue;
    }

    public static HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!Util.IsPlayerValid(player))
            return HookResult.Continue;

        RemoveGlow(player);
        Globals.Wallhackers.Remove(player);

        return HookResult.Continue;
    }

    public static HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!Util.IsPlayerValid(player))
            return HookResult.Continue;

        RemoveGlow(player);
        return HookResult.Continue;
    }

    private static void RemoveGlow(CCSPlayerController player)
    {
        if (!Globals.GlowData.TryGetValue(player, out var data))
            return;

        if (data.GlowEnt.IsValid) data.GlowEnt.Remove();
        if (data.ModelRelay.IsValid) data.ModelRelay.Remove();

        Globals.GlowData.Remove(player);
    }

    private static void Glow(CCSPlayerController player)
    {
        var pawn = player.PlayerPawn?.Value;
        if (pawn == null || !pawn.IsValid) return;

        var sceneNode = pawn.CBodyComponent?.SceneNode;
        if (sceneNode == null) return;

        var skeleton = sceneNode.GetSkeletonInstance();
        if (skeleton == null || skeleton.ModelState == null) return;

        string model = skeleton.ModelState.ModelName;
        if (string.IsNullOrEmpty(model)) return;

        var modelRelay = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
        var glowEntity = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");

        if (modelRelay == null || glowEntity == null) return;

        modelRelay.Spawnflags = 256;
        modelRelay.Render = Color.Transparent;
        modelRelay.RenderMode = RenderMode_t.kRenderNone;

        glowEntity.Spawnflags = 256;
        glowEntity.Render = Color.FromArgb(1, 0, 0, 0);

        modelRelay.SetModel(model);
        glowEntity.SetModel(model);

        modelRelay.DispatchSpawn();
        glowEntity.DispatchSpawn();

        modelRelay.AcceptInput("FollowEntity", pawn, null, "!activator");
        glowEntity.AcceptInput("FollowEntity", modelRelay, null, "!activator");

        glowEntity.Glow.GlowRange = 5000;
        glowEntity.Glow.GlowRangeMin = 0;
        glowEntity.Glow.GlowColorOverride = Color.FromArgb(255, Globals.Config.R, Globals.Config.G, Globals.Config.B);

        // THE FIX: Set the GlowTeam to the opposite team.
        // This forces your own game client to hide the glow from you, even if the engine networks it.
        glowEntity.Glow.GlowTeam = player.Team == CsTeam.Terrorist ? (int)CsTeam.CounterTerrorist : (int)CsTeam.Terrorist;

        glowEntity.Glow.GlowType = 3;

        Globals.GlowData[player] = new GlowData
        {
            GlowEnt = glowEntity,
            ModelRelay = modelRelay
        };
    }

    public static void Setup()
    {
        Globals.Plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        Globals.Plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        Globals.Plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn, HookMode.Post);

        Globals.Plugin.AddCommand("css_wh", "Gives a player walls", CommandWallhack.OnWallhackCommand);
        Globals.Plugin.AddCommand("css_wallhack", "Gives a player walls", CommandWallhack.OnWallhackCommand);
    }

    public static void Cleanup()
    {
        foreach (var data in Globals.GlowData.Values)
        {
            if (data.GlowEnt.IsValid) data.GlowEnt.Remove();
            if (data.ModelRelay.IsValid) data.ModelRelay.Remove();
        }

        Globals.GlowData.Clear();
        Globals.Wallhackers.Clear();
    }
}