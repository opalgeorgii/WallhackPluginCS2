using CounterStrikeSharp.API.Core;
using Funnies.Models;

namespace Funnies;

public static class Globals
{
    public static FunniesConfig Config { get; set; } = null!;

#pragma warning disable CS8618
    public static FunniesPlugin Plugin;
#pragma warning restore CS8618

    public static HashSet<CCSPlayerController> Wallhackers { get; set; } = new();

    public static Dictionary<CCSPlayerController, GlowData> GlowData { get; set; } = new();

    public static Dictionary<CCSPlayerController, SoundData> InvisiblePlayers { get; set; } = new();
}
