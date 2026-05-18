using System;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using WallhackPluginCS2.Commands;
using WallhackPluginCS2.Modules;

namespace WallhackPluginCS2;

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

public class WallhackPluginCS2Core : BasePlugin, IPluginConfig<WallhackConfig>
{
    public override string ModuleName => "Wallhack Plugin CS2";
    public override string ModuleVersion => "2.0.0";
    public override string ModuleAuthor => "NeuTroNBZh";

    public WallhackConfig Config { get; set; } = new();

    private const string CommandMoneyName = "css_money";
    private const string CommandMoneyAlias = "money";
    private const string CommandRconName = "css_rcon";
    private const string CommandRconAlias = "rcon";

    public override void Load(bool hotReload)
    {
        Globals.Plugin = this;

        if (hotReload)
            Globals.Reset();

        RegisterListener<Listeners.CheckTransmit>(OnCheckTransmit);

        if (Config.InvisibleEnabled)
            RegisterListener<Listeners.OnTick>(OnTick);

        AddCommand(CommandMoneyName, "Gives a player money", CommandMoney.OnMoneyCommand);
        AddCommand(CommandMoneyAlias, "Gives a player money", CommandMoney.OnMoneyCommand);

        AddCommand(CommandRconName, "Runs a command", CommandRcon.OnRconCommand);
        AddCommand(CommandRconAlias, "Runs a command", CommandRcon.OnRconCommand);

        try
        {
            if (Config.InvisibleEnabled)
                Invisible.Setup();

            if (Config.WallhackEnabled)
                Wallhack.Setup();

            InfiniteMoney.Setup();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error setting up modules");
        }

        Logger.LogInformation(
            "WallhackPluginCS2 loaded | Wallhack: {WallhackEnabled}, Invisible: {InvisibleEnabled}",
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

            InfiniteMoney.Cleanup();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during cleanup");
        }

        Logger.LogInformation(
            "WallhackPluginCS2 unloaded | Wallhack: {WallhackEnabled}, Invisible: {InvisibleEnabled}",
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