using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace WallhackPluginCS2.Commands;

public class CommandInfiniteMoney
{
    private const int MaxMoney = 65535;

    public static void OnInfiniteMoneyCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (!Util.IsPlayerValid(caller))
            return;

        if (!AdminManager.PlayerHasPermissions(caller, Globals.Config.AdminPermission))
        {
            Util.ServerPrintToChat(caller, "You do not have permission to use this command.");
            return;
        }

        string query = command.ArgString.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            Util.ServerPrintToChat(caller, "Usage: !infmoney <player>");
            return;
        }

        if (!Util.TryResolveSinglePlayer(query, out var player, out var error, includeBots: false) || player == null)
        {
            Util.ServerPrintToChat(caller, error);
            return;
        }

        if (Globals.InfiniteMoneyPlayers.Remove(player))
        {
            Util.ServerPrintToChat(caller, $"{player.PlayerName} no longer has infinite money.");
            Util.ServerPrintToChat(player, "Your infinite money privilege has been removed.");
        }
        else
        {
            Globals.InfiniteMoneyPlayers.Add(player);

            if (player.InGameMoneyServices != null)
            {
                player.InGameMoneyServices.Account = MaxMoney;
                Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
            }

            Util.ServerPrintToChat(caller, $"{player.PlayerName} now has infinite money.");
            Util.ServerPrintToChat(player, "You now have infinite money!");
        }
    }
}
