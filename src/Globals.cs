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

    // ✅ WALLHACK
    public static HashSet<CCSPlayerController> Wallhackers { get; set; } = new();

    public static Dictionary<CCSPlayerController, GlowData> GlowData { get; set; } = new();

    // ✅ INVISIBLE
    public static Dictionary<CCSPlayerController, SoundData> InvisiblePlayers { get; set; } = new();
}