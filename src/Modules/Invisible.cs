using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Funnies.Commands;
using Funnies.Models;

namespace Funnies.Modules;

public class Invisible
{
    private static Dictionary<CEntityInstance, CCSPlayerController> _entities = new();

    public static void OnPlayerTransmit(CCheckTransmitInfo info, CCSPlayerController player)
    {
        var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault();
        if (gameRules == null) return;

        foreach (var (entity, owner) in _entities)
        {
            if (!Util.IsPlayerValid(owner))
                continue;

            if (owner != player)
                info.TransmitEntities.Remove(entity);
        }

        if (gameRules.GameRules?.WarmupPeriod == true) return;

        var c4s = Utilities.FindAllEntitiesByDesignerName<CC4>("weapon_c4");
        if (c4s.Any())
        {
            var c4 = c4s.First();
            if (player.Team != CsTeam.Terrorist && !gameRules.GameRules!.BombPlanted && !c4.IsPlantingViaUse && !gameRules.GameRules!.BombDropped)
                info.TransmitEntities.Remove(c4);
            else
                info.TransmitEntities.Add(c4);
        }
    }

    public static void OnTick()
    {
        _entities.Clear();

        foreach (var invis in Globals.InvisiblePlayers)
        {
            if (!Util.IsPlayerValid(invis.Key)) continue;

            var pawn = invis.Key.PlayerPawn?.Value;
            if (pawn == null || !pawn.IsValid) continue;

            var weaponServices = pawn.WeaponServices;

            if (weaponServices != null)
            {
                var activeWeapon = weaponServices.ActiveWeapon;
                if (activeWeapon.IsValid)
                {
                    var weaponInstance = activeWeapon.Get();
                    if (weaponInstance != null)
                    {
                        var currentWeapon = weaponInstance.As<CCSWeaponBase>();
                        if (currentWeapon != null && currentWeapon.IsValid)
                        {
                            var vData = currentWeapon.VData;
                            if (vData != null && currentWeapon.InReload && !invis.Value.HackyReload)
                            {
                                var data = Globals.InvisiblePlayers[invis.Key];
                                data.HackyReload = true;
                                Globals.InvisiblePlayers[invis.Key] = data;
                                SetPlayerInvisibleFor(invis.Key, vData.DisallowAttackAfterReloadStartDuration);
                            }
                        }
                    }
                }
            }

            float alpha = 255f;
            var half = Server.CurrentTime + ((invis.Value.StartTime - Server.CurrentTime) / 2);

            if (half < Server.CurrentTime)
                alpha = invis.Value.EndTime < Server.CurrentTime
                    ? 0
                    : Util.Map(Server.CurrentTime, half, invis.Value.EndTime, 255, 0);

            int progress = (int)Util.Map(alpha, 0, 255, 0, 20);

            invis.Key.PrintToCenterHtml(
                string.Concat(Enumerable.Repeat("&#9608;", progress)) +
                string.Concat(Enumerable.Repeat("&#9617;", 20 - progress))
            );

            pawn.Render = Color.FromArgb((int)alpha, pawn.Render);
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");

            pawn.ShadowStrength = alpha < 128f ? 1.0f : 0.0f;
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_flShadowStrength");

            foreach (var weapon in pawn.WeaponServices!.MyWeapons)
            {
                var w = weapon.Value!;

                w.Render = Color.FromArgb((int)alpha, pawn.Render);
                Utilities.SetStateChanged(w, "CBaseModelEntity", "m_clrRender");

                w.ShadowStrength = alpha < 128f ? 1.0f : 0.0f;
                Utilities.SetStateChanged(w, "CBaseModelEntity", "m_flShadowStrength");
            }

            if (alpha == 0)
            {
                pawn.EntitySpottedState.Spotted = false;
                pawn.EntitySpottedState.SpottedByMask[0] = 0;

                _entities[pawn] = invis.Key;

                foreach (var weapon in pawn.WeaponServices!.MyWeapons)
                    _entities[weapon.Value!] = invis.Key;

                var data = Globals.InvisiblePlayers[invis.Key];
                data.HackyReload = false;
                Globals.InvisiblePlayers[invis.Key] = data;
            }
        }
    }

    public static HookResult OnPlayerSound(EventPlayerSound @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!Util.IsPlayerValid(player))
            return HookResult.Continue;

        SetPlayerInvisibleFor(player, @event.Duration * 2);

        if (Globals.InvisiblePlayers.TryGetValue(player!, out var data))
        {
            data.RevealUntil = Server.CurrentTime + 2.0f;
            Globals.InvisiblePlayers[player!] = data;
        }

        return HookResult.Continue;
    }

    public static HookResult OnPlayerShoot(EventBulletImpact @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!Util.IsPlayerValid(player))
            return HookResult.Continue;

        SetPlayerInvisibleFor(player, 0.5f);

        if (Globals.InvisiblePlayers.TryGetValue(player!, out var data))
        {
            data.RevealUntil = Server.CurrentTime + 2.0f;
            Globals.InvisiblePlayers[player!] = data;
        }

        return HookResult.Continue;
    }

    public static HookResult OnPlayerStartPlant(EventBombBeginplant @event, GameEventInfo info)
    {
        SetPlayerInvisibleFor(@event.Userid, 1f);
        return HookResult.Continue;
    }

    public static HookResult OnPlayerStartDefuse(EventBombBegindefuse @event, GameEventInfo info)
    {
        SetPlayerInvisibleFor(@event.Userid, 1f);
        return HookResult.Continue;
    }

    public static HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!Util.IsPlayerValid(player))
            return HookResult.Continue;

        SetPlayerInvisibleFor(player, 0.5f);

        if (Globals.InvisiblePlayers.TryGetValue(player!, out var data))
        {
            data.RevealUntil = Server.CurrentTime + 2.0f;
            Globals.InvisiblePlayers[player!] = data;
        }

        return HookResult.Continue;
    }

    private static void SetPlayerInvisibleFor(CCSPlayerController? player, float time)
    {
        if (!Util.IsPlayerValid(player)) return;
        if (!Globals.InvisiblePlayers.TryGetValue(player!, out var data)) return;

        data.StartTime = Server.CurrentTime;
        data.EndTime = Server.CurrentTime + time;

        Globals.InvisiblePlayers[player!] = data;
    }

    public static void Setup()
    {
        Globals.Plugin.RegisterEventHandler<EventBombBeginplant>(OnPlayerStartPlant);
        Globals.Plugin.RegisterEventHandler<EventBulletImpact>(OnPlayerShoot);
        Globals.Plugin.RegisterEventHandler<EventPlayerSound>(OnPlayerSound);
        Globals.Plugin.RegisterEventHandler<EventBombBegindefuse>(OnPlayerStartDefuse);
        Globals.Plugin.RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);

        Globals.Plugin.AddCommand("css_invisible", "Makes a player invisible", CommandInvisible.OnInvisibleCommand);
        Globals.Plugin.AddCommand("css_invis", "Makes a player invisible", CommandInvisible.OnInvisibleCommand);
    }

    public static void Cleanup()
    {
        _entities.Clear();

        foreach (var player in Util.GetValidPlayers())
        {
            var pawn = player.PlayerPawn?.Value;
            if (pawn == null) continue;

            pawn.Render = Color.FromArgb(255, pawn.Render);
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");
            pawn.ShadowStrength = 1.0f;
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_flShadowStrength");

            foreach (var weapon in pawn.WeaponServices!.MyWeapons)
            {
                weapon.Value!.Render = pawn.Render;
                weapon.Value.ShadowStrength = 1.0f;
                Utilities.SetStateChanged(weapon.Value, "CBaseModelEntity", "m_clrRender");
                Utilities.SetStateChanged(weapon.Value, "CBaseModelEntity", "m_flShadowStrength");
            }
        }

        Globals.InvisiblePlayers.Clear();
    }
}