using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Funnies.Commands;
using Funnies.Models;

namespace Funnies.Modules;

public class Wallhack
{
    public static void OnPlayerTransmit(CCheckTransmitInfo info, CCSPlayerController player)
    {
        if (!Util.IsPlayerValid(player))
            return;

        foreach (var (target, data) in Globals.GlowData)
        {
            if (!Util.IsPlayerValid(target))
                continue;

            if (!data.GlowEnt.IsValid || !data.ModelRelay.IsValid)
            {
                info.TransmitEntities.Remove(data.ModelRelay);
                info.TransmitEntities.Remove(data.GlowEnt);
                continue;
            }

            if (target == player)
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

        Globals.Plugin.AddTimer(0.5f, () =>
        {
            if (!Util.IsPlayerValid(player)) return;
            if (!player.PawnIsAlive) return;
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

        if (data.GlowEnt.IsValid)
            data.GlowEnt.Remove();

        if (data.ModelRelay.IsValid)
            data.ModelRelay.Remove();

        Globals.GlowData.Remove(player);
    }

    private static void Glow(CCSPlayerController player)
    {
        var pawn = player.PlayerPawn?.Value;
        if (pawn == null || !pawn.IsValid) return;

        var sceneNode = pawn.CBodyComponent?.SceneNode;
        if (sceneNode == null) return;

        var skeleton = sceneNode.GetSkeletonInstance();
        if (skeleton == null) return;

        string model = skeleton.ModelState.ModelName;
        if (string.IsNullOrEmpty(model)) return;

        if (Globals.GlowData.ContainsKey(player)) return;

        var glowEntity = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
        var modelRelay = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
        if (glowEntity == null || modelRelay == null) return;

        // Setup spawn flags before spawn
        modelRelay.Spawnflags = 256;
        modelRelay.Render = Color.Transparent;
        modelRelay.RenderMode = RenderMode_t.kRenderNone;

        glowEntity.Spawnflags = 256;
        glowEntity.Render = Color.FromArgb(1, 0, 0, 0);

        modelRelay.DispatchSpawn();
        glowEntity.DispatchSpawn();

        // Delay everything until after spawn
        Globals.Plugin.AddTimer(0.1f, () =>
        {
            if (!pawn.IsValid || !glowEntity.IsValid || !modelRelay.IsValid) return;

            // Safe to set model now
            modelRelay.SetModel(model);
            glowEntity.SetModel(model);

            // Follow chain
            modelRelay.AcceptInput("FollowEntity", pawn, null, "!activator");
            glowEntity.AcceptInput("FollowEntity", modelRelay, null, "!activator");

            // Glow setup
            glowEntity.Glow.GlowRange = 5000;
            glowEntity.Glow.GlowRangeMin = 0;
            glowEntity.Glow.GlowColorOverride =
                Color.FromArgb(255, Globals.Config.R, Globals.Config.G, Globals.Config.B);
            glowEntity.Glow.GlowTeam = -1;
            glowEntity.Glow.GlowType = 3;
        });

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
        foreach (var entity in Globals.GlowData)
        {
            if (entity.Value.GlowEnt.IsValid)
                entity.Value.GlowEnt.Remove();

            if (entity.Value.ModelRelay.IsValid)
                entity.Value.ModelRelay.Remove();
        }

        Globals.GlowData.Clear();
        Globals.Wallhackers.Clear();
    }
}