using System;
using System.Diagnostics.CodeAnalysis;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace WallhackPlugin;

public static class Util
{
    public static string? GetPlayerModel(CCSPlayerController player)
    {
        var pawn = player.PlayerPawn?.Value;
        return pawn?.CBodyComponent?.SceneNode
                   ?.GetSkeletonInstance()?.ModelState?.ModelName;
    }

    public static bool IsPlayerValid([NotNullWhen(true)] CCSPlayerController? plr) =>
        plr != null &&
        plr.IsValid &&
        plr.PlayerPawn != null &&
        plr.PlayerPawn.IsValid &&
        plr.Connected == PlayerConnectedState.PlayerConnected &&
        !plr.IsHLTV;

    public static List<CCSPlayerController> GetValidPlayers() =>
        Utilities.GetPlayers().Where(IsPlayerValid).ToList();

    public static List<CCSPlayerController> GetBots()
    {
        var players = GetValidPlayers();
        return players.Where(plr => plr.IsBot).ToList();
    }

    public static List<CCSPlayerController> GetRealPlayers()
    {
        var players = GetValidPlayers();
        return players.Where(plr => !plr.IsBot).ToList();
    }

    public static CCSPlayerController? GetPlayerByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var players = GetValidPlayers();

        return players.FirstOrDefault(x => x.PlayerName.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?? players.FirstOrDefault(x => x.PlayerName.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    public static CCSPlayerController? GetPlayerByPartialName(string partialName)
    {
        if (string.IsNullOrWhiteSpace(partialName))
            return null;

        return GetValidPlayers()
            .FirstOrDefault(x => x.PlayerName.Contains(partialName, StringComparison.OrdinalIgnoreCase));
    }

    public static List<CCSPlayerController> GetPlayersByPartialName(string partialName)
    {
        if (string.IsNullOrWhiteSpace(partialName))
            return new();

        return GetValidPlayers()
            .Where(x => x.PlayerName.Contains(partialName, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public static void ServerPrintToChat(CCSPlayerController player, string message)
    {
        player.PrintToChat($" {ChatColors.Green}[SERVER]{ChatColors.White} {message}");
    }

    public static void Broadcast(string message)
    {
        foreach (var player in GetValidPlayers())
        {
            ServerPrintToChat(player, message);
        }
    }

    public static float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        if (fromMax == fromMin) return toMin;
        float normalized = (value - fromMin) / (fromMax - fromMin);
        normalized = Math.Clamp(normalized, 0f, 1f);
        return toMin + normalized * (toMax - toMin);
    }

    public static IEnumerable<CGameSceneNode> GetChildrenRecursive(CGameSceneNode node)
    {
        var child = node.Child;

        while (child != null)
        {
            yield return child;

            foreach (var grandChild in GetChildrenRecursive(child))
                yield return grandChild;

            child = child.NextSibling;
        }
    }
}