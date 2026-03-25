using System;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using WallhackPlugin.Commands;
using Microsoft.Extensions.Logging;
using WallhackPlugin.Modules;

namespace WallhackPlugin;

public class WallhackConfig : BasePluginConfig
{
    [JsonPropertyName("ColorR")]
    public byte R { get; set; } = 255;

    [JsonPropertyName("ColorG")]
    public byte G { get; set; } = 0;

    [JsonPropertyName("ColorB")]
    public byte B { get; set; } = 128;

    [JsonPropertyName("CommandPermission")]
    public string AdminPermission { get; set; } = "@css/generic";

    [JsonPropertyName("RconPermission")]
    public string RconPermission { get; set; } = "@css/rcon";

    [JsonPropertyName("WallhackEnabled")]
    public bool WallhackEnabled { get; set; } = true;

    [JsonPropertyName("InvisibleEnabled")]
    public bool InvisibleEnabled { get; set; } = true;
}

public class WallhackPluginCore : BasePlugin, IPluginConfig<WallhackConfig>
{
    public override string ModuleName => "Wallhack Plugin";
    public override string ModuleVersion => "1.0.0";

    public WallhackConfig Config { get; set; } = new();

    private const string CommandMoneyName = "css_money";
    private const string CommandRconName = "css_rcon";

    public override void Load(bool hotReload)
    {
        Globals.Plugin = this;

        if (hotReload)
            Globals.Reset();

        RegisterListener<Listeners.CheckTransmit>(OnCheckTransmit);

        if (Config.InvisibleEnabled)
            RegisterListener<Listeners.OnTick>(OnTick);

        AddCommand(CommandMoneyName, "Gives a player money", CommandMoney.OnMoneyCommand);
        AddCommand(CommandRconName, "Runs a command", CommandRcon.OnRconCommand);

        try
        {
            if (Config.InvisibleEnabled)
                Invisible.Setup();

            if (Config.WallhackEnabled)
                Wallhack.Setup();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error setting up modules");
        }

        Logger.LogInformation(
            "WallhackPlugin loaded | Wallhack: {0}, Invisible: {1}",
            Config.WallhackEnabled,
            Config.InvisibleEnabled
        );
    }

    public override void Unload(bool hotReload)
    {
        try
        {
            if (Config.InvisibleEnabled)
                Invisible.Cleanup();

            if (Config.WallhackEnabled)
                Wallhack.Cleanup();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during cleanup");
        }

        Logger.LogInformation(
            "WallhackPlugin unloaded | Wallhack: {0}, Invisible: {1}",
            Config.WallhackEnabled,
            Config.InvisibleEnabled
        );
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
            if (!Util.IsPlayerValid(player))
                continue;

            if (Config.WallhackEnabled)
                Wallhack.OnPlayerTransmit(info, player!);

            if (Config.InvisibleEnabled)
                Invisible.OnPlayerTransmit(info, player!);
        }
    }

    public void OnConfigParsed(WallhackConfig config)
    {
        Config.R = ClampByte(config.R);
        Config.G = ClampByte(config.G);
        Config.B = ClampByte(config.B);

        Config.AdminPermission = config.AdminPermission;
        Config.RconPermission = config.RconPermission;
        Config.WallhackEnabled = config.WallhackEnabled;
        Config.InvisibleEnabled = config.InvisibleEnabled;

        Globals.Config = Config;
    }

    private static byte ClampByte(byte value) => Math.Clamp(value, (byte)0, (byte)255);
}