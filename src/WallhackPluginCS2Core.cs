using System;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using WallhackPluginCS2.Commands;
using WallhackPluginCS2.Modules;

namespace WallhackPluginCS2;

public class WallhackConfig : BasePluginConfig
{
    // Terrorist glow color
    [JsonPropertyName("ColorR")]
    public byte R { get; set; } = 255;

    [JsonPropertyName("ColorG")]
    public byte G { get; set; } = 100;

    [JsonPropertyName("ColorB")]
    public byte B { get; set; } = 0;

    // Counter-Terrorist glow color
    [JsonPropertyName("ColorCT_R")]
    public byte CTR { get; set; } = 0;

    [JsonPropertyName("ColorCT_G")]
    public byte CTG { get; set; } = 100;

    [JsonPropertyName("ColorCT_B")]
    public byte CTB { get; set; } = 255;

    [JsonPropertyName("CommandPermission")]
    public string AdminPermission { get; set; } = "@css/generic";

    [JsonPropertyName("RconPermission")]
    public string RconPermission { get; set; } = "@css/rcon";

    [JsonPropertyName("WallhackEnabled")]
    public bool WallhackEnabled { get; set; } = true;

    [JsonPropertyName("InvisibleEnabled")]
    public bool InvisibleEnabled { get; set; } = true;

    [JsonPropertyName("InfiniteMoneyEnabled")]
    public bool InfiniteMoneyEnabled { get; set; } = true;

    [JsonPropertyName("InvisibleRevealOnMove")]
    public bool InvisibleRevealOnMove { get; set; } = false;
}

public class WallhackPluginCS2Core : BasePlugin, IPluginConfig<WallhackConfig>
{
    public override string ModuleName => "Wallhack Plugin CS2";
    public override string ModuleVersion => "2.3.0";
    public override string ModuleAuthor => "NeuTroNBZh";

    public WallhackConfig Config { get; set; } = new();

    public override void Load(bool hotReload)
    {
        Globals.Plugin = this;

        if (hotReload)
            Globals.Reset();

        RegisterListener<Listeners.CheckTransmit>(OnCheckTransmit);
        RegisterListener<Listeners.OnTick>(OnTick);

        AddCommand("css_money",    "Gives a player money",              CommandMoney.OnMoneyCommand);
        AddCommand("money",        "Gives a player money",              CommandMoney.OnMoneyCommand);
        AddCommand("css_rcon",     "Runs a command",                    CommandRcon.OnRconCommand);
        AddCommand("rcon",         "Runs a command",                    CommandRcon.OnRconCommand);
        AddCommand("css_slay",     "Kill a player",                     CommandSlay.OnSlayCommand);
        AddCommand("slay",         "Kill a player",                     CommandSlay.OnSlayCommand);
        AddCommand("css_slap",     "Slap a player for damage",          CommandSlap.OnSlapCommand);
        AddCommand("slap",         "Slap a player for damage",          CommandSlap.OnSlapCommand);
        AddCommand("css_status",   "Show active privileges for player", CommandStatus.OnStatusCommand);
        AddCommand("status",       "Show active privileges for player", CommandStatus.OnStatusCommand);
        AddCommand("css_resetall", "Remove all player privileges",      CommandResetAll.OnResetAllCommand);
        AddCommand("resetall",     "Remove all player privileges",      CommandResetAll.OnResetAllCommand);
        AddCommand("css_hp",       "Set HP for a player",               CommandHp.OnHpCommand);
        AddCommand("hp",           "Set HP for a player",               CommandHp.OnHpCommand);

        try
        {
            if (Config.WallhackEnabled)
                Wallhack.Setup();

            if (Config.InvisibleEnabled)
                Invisible.Setup();

            if (Config.InfiniteMoneyEnabled)
                InfiniteMoney.Setup();

            God.Setup();
            Speed.Setup();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error setting up modules");
        }

        Logger.LogInformation(
            "WallhackPluginCS2 v{Version} loaded | WH:{WH} Invis:{Invis} Money:{Money}",
            ModuleVersion,
            Config.WallhackEnabled,
            Config.InvisibleEnabled,
            Config.InfiniteMoneyEnabled
        );
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        SimpleAdminBridge.TryInit();
        SimpleAdminBridge.RegisterMenus();
    }

    public override void Unload(bool hotReload)
    {
        SimpleAdminBridge.UnregisterMenus();

        try
        {
            if (Config.WallhackEnabled)
                Wallhack.Cleanup();

            if (Config.InvisibleEnabled)
                Invisible.Cleanup();

            if (Config.InfiniteMoneyEnabled)
                InfiniteMoney.Cleanup();

            God.Cleanup();
            Speed.Cleanup();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during cleanup");
        }
    }

    public void OnTick()
    {
        if (Config.InvisibleEnabled)
            Invisible.OnTick();

        God.OnTick();
        Speed.OnTick();
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
        Config.R   = ClampByte(config.R);
        Config.G   = ClampByte(config.G);
        Config.B   = ClampByte(config.B);
        Config.CTR = ClampByte(config.CTR);
        Config.CTG = ClampByte(config.CTG);
        Config.CTB = ClampByte(config.CTB);

        Config.AdminPermission     = config.AdminPermission;
        Config.RconPermission      = config.RconPermission;
        Config.WallhackEnabled     = config.WallhackEnabled;
        Config.InvisibleEnabled    = config.InvisibleEnabled;
        Config.InfiniteMoneyEnabled = config.InfiniteMoneyEnabled;
        Config.InvisibleRevealOnMove = config.InvisibleRevealOnMove;

        Globals.Config = Config;
    }

    private static byte ClampByte(byte value) => Math.Clamp(value, (byte)0, (byte)255);
}
