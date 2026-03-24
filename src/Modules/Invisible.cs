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
    private static List<CEntityInstance> _entities = new();

    public static void OnPlayerTransmit(CCheckTransmitInfo info, CCSPlayerController player)
    {
        if (!Util.IsPlayerValid(player))
            return;

        foreach (var invis in Globals.InvisiblePlayers)
        {
            var owner = invis.Key;
            if (!Util.IsPlayerValid(owner)) continue;

            var pawn = owner.PlayerPawn?.Value;
            if (pawn == null) continue;

            // If viewer should not see the invisible player
            if (player != owner && player.Team != CsTeam.Spectator)
            {
                info.TransmitEntities.Remove(pawn);

                if (pawn.WeaponServices != null)
                {
                    foreach (var weapon in pawn.WeaponServices.MyWeapons)
                    {
                        if (weapon.Value != null)
                            info.TransmitEntities.Remove(weapon.Value);
                    }
                }
            }
        }

        // Keep your C4 logic if needed
        var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault();
        if (gameRules == null) return;

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
            var player = invis.Key;
            var data = invis.Value;

            if (!Util.IsPlayerValid(player)) continue;

            var pawn = player.PlayerPawn?.Value;
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
                            if (vData != null && currentWeapon.InReload && !data.HackyReload)
                            {
                                data.HackyReload = true;
                                Globals.InvisiblePlayers[player] = data;
                                SetPlayerInvisibleFor(player, vData.DisallowAttackAfterReloadStartDuration);
                            }
                        }
                    }
                }
            }

            // Alpha calculation
            float alpha = 255f;
            var half = Server.CurrentTime + ((data.StartTime - Server.CurrentTime) / 2);
            if (half < Server.CurrentTime)
                alpha = data.EndTime < Server.CurrentTime ? 0 : Util.Map(Server.CurrentTime, half, data.EndTime, 255, 0);

            int progress = (int)Util.Map(alpha, 0, 255, 0, 20);

            // Update progress bar in center
            player.PrintToCenterHtml(
                string.Concat(Enumerable.Repeat("&#9608;", progress)) +
                string.Concat(Enumerable.Repeat("&#9617;", 20 - progress))
            );

            // Player render & shadow
            pawn.Render = Color.FromArgb((int)alpha, pawn.Render);
            pawn.ShadowStrength = alpha == 0 ? 0.0f : 1.0f;
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_flShadowStrength");

            // Weapons render & shadow
            if (weaponServices != null)
            {
                foreach (var weapon in weaponServices.MyWeapons)
                {
                    var wpn = weapon.Value;
                    if (wpn == null) continue;

                    if (alpha == 0)
                    {
                        // Fully invisible
                        wpn.Render = Color.FromArgb(0, 0, 0, 0);
                        wpn.ShadowStrength = 0.0f;
                        _entities.Add(wpn); // hide from transmit
                    }
                    else
                    {
                        // Fading / visible
                        wpn.Render = Color.FromArgb((int)alpha, pawn.Render);
                        wpn.ShadowStrength = alpha < 128f ? 1.0f : 0.0f;
                        _entities.Remove(wpn);
                    }

                    Utilities.SetStateChanged(wpn, "CBaseModelEntity", "m_clrRender");
                    Utilities.SetStateChanged(wpn, "CBaseModelEntity", "m_flShadowStrength");
                }
            }

            // Fully invisible -> mark pawn to remove
            if (alpha == 0)
                _entities.Add(pawn);
            else
                _entities.Remove(pawn);

            // Reset hacky reload flag
            if (alpha == 0)
            {
                data.HackyReload = false;
                Globals.InvisiblePlayers[player] = data;
            }
        }
    }

    public static HookResult OnPlayerSound(EventPlayerSound @event, GameEventInfo info)
    {
        SetPlayerInvisibleFor(@event.Userid, @event.Duration * 2);
        return HookResult.Continue;
    }

    public static HookResult OnPlayerShoot(EventBulletImpact @event, GameEventInfo info)
    {
        SetPlayerInvisibleFor(@event.Userid, 0.5f);
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
        SetPlayerInvisibleFor(@event.Userid, 0.5f);
        return HookResult.Continue;
    }

    private static void SetPlayerInvisibleFor(CCSPlayerController? player, float time)
    {
        if (!Util.IsPlayerValid(player)) return;
        if (!Globals.InvisiblePlayers.TryGetValue(player, out var data)) return;

        data.StartTime = Server.CurrentTime;
        data.EndTime = Server.CurrentTime + time;

        Globals.InvisiblePlayers[player] = data;
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
            pawn.ShadowStrength = 1.0f;
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_flShadowStrength");

            var weaponServices = pawn.WeaponServices;
            if (weaponServices != null)
            {
                foreach (var weapon in weaponServices.MyWeapons)
                {
                    var wpn = weapon.Value;
                    if (wpn == null) continue;

                    wpn.Render = Color.FromArgb(255, pawn.Render);
                    wpn.ShadowStrength = 1.0f;

                    Utilities.SetStateChanged(wpn, "CBaseModelEntity", "m_clrRender");
                    Utilities.SetStateChanged(wpn, "CBaseModelEntity", "m_flShadowStrength");
                }
            }
        }

        Globals.InvisiblePlayers.Clear();
    }
}