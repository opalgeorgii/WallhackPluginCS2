using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using Funnies.Commands;
using Funnies.Modules;

namespace Funnies;

public class FunniesConfig : BasePluginConfig
{
    [JsonPropertyName("ColorR")]
    public byte R { get; set; } = 255;

    [JsonPropertyName("ColorG")]
    public byte G { get; set; } = 0;

    [JsonPropertyName("ColorB")]
    public byte B { get; set; } = 0;

    [JsonPropertyName("CommandPermission")]
    public string AdminPermission { get; set; } = "@css/generic";

    [JsonPropertyName("RconPermission")]
    public string RconPermission { get; set; } = "@css/rcon";

    [JsonPropertyName("WallhackEnabled")]
    public bool WallhackEnabled { get; set; } = true;

    [JsonPropertyName("InvisibleEnabled")]
    public bool InvisibleEnabled { get; set; } = true;
}

public class FunniesPlugin : BasePlugin, IPluginConfig<FunniesConfig>
{
    public override string ModuleName => "Funny plugin";
    public override string ModuleVersion => "0.0.1";

    public FunniesConfig Config { get; set; } = new();

    public override void Load(bool hotReload)
    {
        Globals.Plugin = this;

        RegisterListener<Listeners.CheckTransmit>(OnCheckTransmit);

        if (Config.InvisibleEnabled)
            RegisterListener<Listeners.OnTick>(OnTick);

        AddCommand("css_money", "Gives a player money", CommandMoney.OnMoneyCommand);
        AddCommand("css_rcon", "Runs a command", CommandRcon.OnRconCommand);

#if DEBUG
        AddCommand("css_debug", "Debug command", CommandDebug.OnDebugCommand);
#endif

        if (Config.InvisibleEnabled)
            Invisible.Setup();

        if (Config.WallhackEnabled)
            Wallhack.Setup();
    }

    public override void Unload(bool hotReload)
    {
        if (hotReload)
        {
            if (Config.InvisibleEnabled)
                Invisible.Cleanup();

            if (Config.WallhackEnabled)
                Wallhack.Cleanup();
        }
    }

    public void OnTick()
    {
        if (Config.InvisibleEnabled)
            Invisible.OnTick();
    }

    public void OnCheckTransmit(CCheckTransmitInfoList infoList)
    {
        foreach ((CCheckTransmitInfo info, CCSPlayerController? player) in infoList)
        {
            if (!Util.IsPlayerValid(player)) continue;

            if (Config.WallhackEnabled)
                Wallhack.OnPlayerTransmit(info, player!);

            if (Config.InvisibleEnabled)
                Invisible.OnPlayerTransmit(info, player!);
        }
    }

    public void OnConfigParsed(FunniesConfig config)
    {
        Config = config;
        Globals.Config = config;
    }
}