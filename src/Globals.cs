using CounterStrikeSharp.API.Core;
using Funnies.Models;

namespace Funnies;

public static class Globals
{
    // ✅ Config
    public static FunniesConfig Config { get; set; } = null!;

    // ✅ Main plugin instance
#pragma warning disable CS8618
    public static FunniesPlugin Plugin;
#pragma warning restore CS8618

    // ✅ Wallhack players (FAST lookup)
    public static HashSet<CCSPlayerController> Wallhackers { get; set; } = new();

    // ✅ Glow entities per player
    public static Dictionary<CCSPlayerController, GlowData> GlowData { get; set; } = new();

    // ✅ (Optional) invisible players system (keep if you use it)
    public static Dictionary<CCSPlayerController, SoundData> InvisiblePlayers { get; set; } = new();
}