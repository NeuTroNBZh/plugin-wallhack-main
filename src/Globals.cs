using System;
using CounterStrikeSharp.API.Core;
using WallhackPluginCS2.Models;

namespace WallhackPluginCS2;

public static class Globals
{
    private static WallhackConfig? _config;
    public static WallhackConfig Config
    {
        get => _config ?? throw new InvalidOperationException("Globals.Config not initialized");
        set => _config = value;
    }

    private static WallhackPluginCS2Core? _plugin;
    public static WallhackPluginCS2Core Plugin
    {
        get => _plugin ?? throw new InvalidOperationException("Globals.Plugin not initialized");
        set => _plugin = value;
    }

    public static HashSet<CCSPlayerController> Wallhackers { get; } = new();

    public static Dictionary<CCSPlayerController, GlowData> GlowData { get; } = new();

    public static HashSet<CCSPlayerController> InvisiblePlayers { get; } = new();

    public static Dictionary<CCSPlayerController, SoundData> InvisibleReveal { get; } = new();

    public static HashSet<CCSPlayerController> InfiniteMoneyPlayers { get; } = new();

    public static HashSet<CCSPlayerController> GodPlayers { get; } = new();

    public static Dictionary<CCSPlayerController, float> SpeedPlayers { get; } = new();

    public static void Reset()
    {
        Wallhackers.Clear();
        GlowData.Clear();
        InvisiblePlayers.Clear();
        InvisibleReveal.Clear();
        InfiniteMoneyPlayers.Clear();
        GodPlayers.Clear();
        SpeedPlayers.Clear();
    }
}
