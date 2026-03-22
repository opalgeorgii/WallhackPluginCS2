using System.Diagnostics.CodeAnalysis;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace Funnies;

public static class Util
{
    // Returns the player's model string
    public static string GetPlayerModel(CCSPlayerController player)
    {
        // Be careful: SceneNode or CBodyComponent might be null
        return player.PlayerPawn!.Value!.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;
    }

    // Checks if a player is valid
    public static bool IsPlayerValid([NotNullWhen(true)] CCSPlayerController? plr) =>
        plr != null &&
        plr.IsValid &&
        plr.PlayerPawn != null &&
        plr.PlayerPawn.IsValid &&
        plr.Connected == PlayerConnectedState.PlayerConnected &&
        !plr.IsHLTV;

    // Returns all valid players
    public static List<CCSPlayerController> GetValidPlayers() =>
        Utilities.GetPlayers().Where(IsPlayerValid).ToList();

    // Returns all valid bots
    public static List<CCSPlayerController> GetBots() =>
        GetValidPlayers().Where(plr => plr.IsBot).ToList();

    // Returns all valid real players
    public static List<CCSPlayerController> GetRealPlayers() =>
        GetValidPlayers().Where(plr => !plr.IsBot).ToList();

    // Maps a float from one range to another
    public static float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        float normalized = (value - fromMin) / (fromMax - fromMin);
        return toMin + normalized * (toMax - toMin);
    }

    // Get player by exact name
    public static CCSPlayerController? GetPlayerByName(string name)
    {
        return GetValidPlayers().FirstOrDefault(x => x.PlayerName == name);
    }

    // Sends a server message to a specific player
    public static void ServerPrintToChat(CCSPlayerController player, string message)
    {
        player.PrintToChat($" {ChatColors.Green}[SERVER]{ChatColors.White} {message}");
    }

    // Recursively get all children of a game scene node
    public static List<CGameSceneNode> GetChildrenRecursive(CGameSceneNode gameSceneNode)
    {
        List<CGameSceneNode> result = new();
        var currentChild = gameSceneNode.Child;

        while (currentChild != null)
        {
            result.Add(currentChild);
            result.AddRange(GetChildrenRecursive(currentChild));
            currentChild = currentChild.NextSibling;
        }

        return result;
    }
}