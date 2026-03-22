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

            // 🔥 HARD BLOCK FIRST (before any logic)
            if (target == player)
            {
                info.TransmitEntities.Remove(data.ModelRelay);
                info.TransmitEntities.Remove(data.GlowEnt);
                continue;
            }

            bool shouldSee =
                Globals.Wallhackers.Contains(player) &&
                target.Team != player.Team &&
                player.Team != CsTeam.Spectator &&
                target.Team != CsTeam.Spectator;

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

        if (player.Team < CsTeam.Terrorist)
            return HookResult.Continue;

        RemoveGlow(player);

        Console.WriteLine($"[WH DEBUG] Scheduling Glow for {player.PlayerName}");

        Globals.Plugin.AddTimer(0.3f, () =>
        {
            if (!Util.IsPlayerValid(player)) return;

            var pawn = player.PlayerPawn?.Value;
            if (pawn == null || !pawn.IsValid) return;

            // ✅ correct place for alive check
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
        Globals.Wallhackers.Remove(player!);

        return HookResult.Continue;
    }

    public static HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (!Util.IsPlayerValid(player))
            return HookResult.Continue;

        // ✅ FULL CLEANUP
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
        if (Globals.Wallhackers.Contains(player))
        {
            // don't create glow for wallhack user
            return;
        }

        if (Globals.GlowData.ContainsKey(player))
        {
            Console.WriteLine("[WH DEBUG] Glow already exists (blocked duplicate)");
            return;
        }

        if (!Util.IsPlayerValid(player) || player.PlayerPawn == null || !player.PlayerPawn.IsValid)
        {
            Console.WriteLine("[WH DEBUG] Glow() aborted - invalid pawn");
            return;
        }

        // ✅ GET MODEL FIRST (CRITICAL)
        string model = player.PlayerPawn?.Value?.CBodyComponent?.SceneNode?
            .GetSkeletonInstance().ModelState.ModelName ?? "";

        if (string.IsNullOrEmpty(model))
        {
            Console.WriteLine("[WH DEBUG] Model NOT ready, retrying...");
            Globals.Plugin.AddTimer(0.1f, () => Glow(player));
            return;
        }

        Console.WriteLine($"[WH] Creating glow for {player.PlayerName}");

        var glowEntity = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
        var modelRelay = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");

        if (glowEntity == null || modelRelay == null)
            return;

        // ✅ SET MODEL BEFORE SPAWN
        modelRelay.Spawnflags = 256;
        modelRelay.Render = Color.Transparent;
        modelRelay.RenderMode = RenderMode_t.kRenderNone;
        modelRelay.SetModel(model);

        glowEntity.Spawnflags = 256;
        glowEntity.Render = Color.FromArgb(1, 0, 0, 0);
        glowEntity.SetModel(model);

        // ✅ NOW SPAWN (correct order)
        modelRelay.DispatchSpawn();
        glowEntity.DispatchSpawn();

        // glow settings
        glowEntity.Glow.GlowRange = 5000;
        glowEntity.Glow.GlowRangeMin = 0;
        glowEntity.Glow.GlowColorOverride = Color.FromArgb(255, Globals.Config.R, Globals.Config.G, Globals.Config.B);
        glowEntity.Glow.GlowTeam = -1;
        glowEntity.Glow.GlowType = 3;

        var pawn = player.PlayerPawn!.Value;

        if (pawn == null || !pawn.IsValid)
            return;

        // follow safely
        Server.NextWorldUpdate(() =>
        {
            if (!modelRelay.IsValid || !glowEntity.IsValid) return;
            if (!pawn.IsValid) return;

            modelRelay.AcceptInput("FollowEntity", pawn, null, "!activator");
            glowEntity.AcceptInput("FollowEntity", modelRelay, null, "!activator");
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
            Server.NextWorldUpdate(() =>
            {
                if (entity.Value.GlowEnt.IsValid)
                    entity.Value.GlowEnt.Remove();

                if (entity.Value.ModelRelay.IsValid)
                    entity.Value.ModelRelay.Remove();
            });
        }

        Globals.GlowData.Clear();
        Globals.Wallhackers.Clear();
    }
}