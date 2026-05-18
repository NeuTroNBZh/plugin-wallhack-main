using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using WallhackPluginCS2.Commands;

namespace WallhackPluginCS2.Modules;

public class InfiniteMoney
{
    private const int MaxMoney = 65535;

    private static void GiveMoney(CCSPlayerController player)
    {
        if (!Util.IsPlayerValid(player) || player.InGameMoneyServices == null)
            return;

        player.InGameMoneyServices.Account = MaxMoney;
        Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
    }

    public static HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        foreach (var player in Globals.InfiniteMoneyPlayers.ToList())
            GiveMoney(player);

        return HookResult.Continue;
    }

    public static HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !Globals.InfiniteMoneyPlayers.Contains(player))
            return HookResult.Continue;

        GiveMoney(player);
        return HookResult.Continue;
    }

    public static HookResult OnItemPurchase(EventItemPurchase @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !Globals.InfiniteMoneyPlayers.Contains(player))
            return HookResult.Continue;

        GiveMoney(player);
        return HookResult.Continue;
    }

    public static HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player != null)
            Globals.InfiniteMoneyPlayers.Remove(player);

        return HookResult.Continue;
    }

    public static void Setup()
    {
        Globals.Plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        Globals.Plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn, HookMode.Post);
        Globals.Plugin.RegisterEventHandler<EventItemPurchase>(OnItemPurchase);
        Globals.Plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

        Globals.Plugin.AddCommand("css_infmoney", "Toggle infinite money for a player", CommandInfiniteMoney.OnInfiniteMoneyCommand);
        Globals.Plugin.AddCommand("infmoney", "Toggle infinite money for a player", CommandInfiniteMoney.OnInfiniteMoneyCommand);
    }

    public static void Cleanup()
    {
        Globals.InfiniteMoneyPlayers.Clear();
    }
}
