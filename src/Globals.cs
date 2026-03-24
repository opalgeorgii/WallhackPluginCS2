using System;
using CounterStrikeSharp.API.Core;
using Funnies.Models;

namespace Funnies;

public static class Globals
{
    private static FunniesConfig? _config;
    public static FunniesConfig Config
    {
        get => _config ?? throw new InvalidOperationException("Globals.Config not initialized");
        set => _config = value;
    }

    private static FunniesPlugin? _plugin;
    public static FunniesPlugin Plugin
    {
        get => _plugin ?? throw new InvalidOperationException("Globals.Plugin not initialized");
        set => _plugin = value;
    }

    public static HashSet<CCSPlayerController> Wallhackers { get; } = new();

    public static Dictionary<CCSPlayerController, GlowData> GlowData { get; } = new();

    public static Dictionary<CCSPlayerController, SoundData> InvisiblePlayers { get; } = new();

    public static void Reset()
    {
        Wallhackers.Clear();
        GlowData.Clear();
        InvisiblePlayers.Clear();
    }
}