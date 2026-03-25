using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using WallhackPlugin.Commands;
using WallhackPlugin.Models;

namespace WallhackPlugin.Modules;

public class Invisible
{
    private static readonly Dictionary<CEntityInstance, CCSPlayerController> HiddenEntities = new();

    public static void OnPlayerTransmit(CCheckTransmitInfo info, CCSPlayerController viewer)
    {
        if (!Util.IsPlayerValid(viewer))
            return;

        foreach (var (entity, owner) in HiddenEntities)
        {
            if (!entity.IsValid || !Util.IsPlayerEntityValid(owner))
                continue;

            if (owner.Slot != viewer.Slot)
                info.TransmitEntities.Remove(entity);
        }
    }

    public static void OnTick()
    {
        HiddenEntities.Clear();

        foreach (var (owner, _) in Globals.InvisiblePlayers.ToList())
        {
            if (!Util.IsPlayerValid(owner) || !owner.PawnIsAlive)
            {
                Globals.InvisiblePlayers.Remove(owner);
                continue;
            }

            var pawn = owner.PlayerPawn?.Value;
            if (pawn == null || !pawn.IsValid)
            {
                Globals.InvisiblePlayers.Remove(owner);
                continue;
            }

            var data = Globals.InvisiblePlayers[owner];

            HandleReloadReveal(pawn, ref data);

            float alpha = GetAlpha(data);
            byte alphaByte = (byte)Math.Clamp((int)alpha, 0, 255);

            int progress = (int)Util.Map(alpha, 0f, 255f, 0f, 20f);
            owner.PrintToCenterHtml(
                string.Concat(Enumerable.Repeat("&#9608;", progress)) +
                string.Concat(Enumerable.Repeat("&#9617;", 20 - progress))
            );

            ApplyPawnVisuals(pawn, alphaByte);
            ApplyWeaponVisuals(pawn, alphaByte);

            bool hideFromOthers = Server.CurrentTime > data.RevealUntil;

            pawn.EntitySpottedState.Spotted = false;
            pawn.EntitySpottedState.SpottedByMask[0] = 0;

            if (hideFromOthers)
            {
                HiddenEntities[pawn] = owner;

                if (pawn.WeaponServices != null)
                {
                    foreach (var handle in pawn.WeaponServices.MyWeapons)
                    {
                        var weapon = handle.Value;
                        if (weapon == null || !weapon.IsValid)
                            continue;

                        HiddenEntities[weapon] = owner;
                    }
                }
            }

            Globals.InvisiblePlayers[owner] = data;
        }
    }

    public static HookResult OnPlayerSound(EventPlayerSound @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!Util.IsPlayerValid(player))
            return HookResult.Continue;

        SetPlayerInvisibleFor(player, @event.Duration * 2f);

        if (Globals.InvisiblePlayers.TryGetValue(player, out var data))
        {
            data.RevealUntil = Server.CurrentTime + 2.0f;
            Globals.InvisiblePlayers[player] = data;
        }

        return HookResult.Continue;
    }

    public static HookResult OnPlayerShoot(EventBulletImpact @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!Util.IsPlayerValid(player))
            return HookResult.Continue;

        SetPlayerInvisibleFor(player, 0.5f);

        if (Globals.InvisiblePlayers.TryGetValue(player, out var data))
        {
            data.RevealUntil = Server.CurrentTime + 2.0f;
            Globals.InvisiblePlayers[player] = data;
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

        if (Globals.InvisiblePlayers.TryGetValue(player, out var data))
        {
            data.RevealUntil = Server.CurrentTime + 2.0f;
            Globals.InvisiblePlayers[player] = data;
        }

        return HookResult.Continue;
    }

    private static void HandleReloadReveal(CCSPlayerPawn pawn, ref SoundData data)
    {
        var weaponServices = pawn.WeaponServices;
        if (weaponServices == null)
        {
            data.HackyReload = false;
            return;
        }

        var activeWeaponHandle = weaponServices.ActiveWeapon;
        if (!activeWeaponHandle.IsValid)
        {
            data.HackyReload = false;
            return;
        }

        var activeWeapon = activeWeaponHandle.Get()?.As<CCSWeaponBase>();
        if (activeWeapon == null || !activeWeapon.IsValid)
        {
            data.HackyReload = false;
            return;
        }

        if (!activeWeapon.InReload)
        {
            data.HackyReload = false;
            return;
        }

        if (data.HackyReload)
            return;

        var vData = activeWeapon.VData;
        if (vData == null)
            return;

        data.HackyReload = true;
        data.StartTime = Server.CurrentTime;
        data.EndTime = Server.CurrentTime + vData.DisallowAttackAfterReloadStartDuration;
        data.RevealUntil = Math.Max(data.RevealUntil, Server.CurrentTime + 2.0f);
    }

    private static float GetAlpha(SoundData data)
    {
        if (data.EndTime <= Server.CurrentTime)
            return 0f;

        float half = data.StartTime + ((data.EndTime - data.StartTime) / 2f);
        if (Server.CurrentTime <= half)
            return 255f;

        return Util.Map(Server.CurrentTime, half, data.EndTime, 255f, 0f);
    }

    private static void ApplyPawnVisuals(CCSPlayerPawn pawn, byte alpha)
    {
        pawn.Render = Color.FromArgb(alpha, pawn.Render);
        Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");

        pawn.ShadowStrength = 0.0f;
        Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_flShadowStrength");
    }

    private static void ApplyWeaponVisuals(CCSPlayerPawn pawn, byte alpha)
    {
        if (pawn.WeaponServices == null)
            return;

        foreach (var handle in pawn.WeaponServices.MyWeapons)
        {
            var weapon = handle.Value;
            if (weapon == null || !weapon.IsValid)
                continue;

            weapon.Render = Color.FromArgb(alpha, weapon.Render);
            Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_clrRender");

            weapon.ShadowStrength = 0.0f;
            Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_flShadowStrength");
        }
    }

    private static void SetPlayerInvisibleFor(CCSPlayerController? player, float time)
    {
        if (!Util.IsPlayerValid(player) || player == null)
            return;

        if (!Globals.InvisiblePlayers.TryGetValue(player, out var data))
            return;

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
        Globals.Plugin.AddCommand("invisible", "Makes a player invisible", CommandInvisible.OnInvisibleCommand);
        Globals.Plugin.AddCommand("invis", "Makes a player invisible", CommandInvisible.OnInvisibleCommand);
    }

    public static void Cleanup()
    {
        HiddenEntities.Clear();

        foreach (var player in Util.GetValidPlayers())
        {
            var pawn = player.PlayerPawn?.Value;
            if (pawn == null || !pawn.IsValid)
                continue;

            pawn.Render = Color.FromArgb(255, pawn.Render);
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");

            pawn.ShadowStrength = 1.0f;
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_flShadowStrength");

            if (pawn.WeaponServices == null)
                continue;

            foreach (var handle in pawn.WeaponServices.MyWeapons)
            {
                var weapon = handle.Value;
                if (weapon == null || !weapon.IsValid)
                    continue;

                weapon.Render = Color.FromArgb(255, weapon.Render);
                Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_clrRender");

                weapon.ShadowStrength = 1.0f;
                Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_flShadowStrength");
            }
        }

        Globals.InvisiblePlayers.Clear();
    }
}