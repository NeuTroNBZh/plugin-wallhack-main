using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace WallhackPluginCS2.Commands;

public class CommandStatus
{
    public static void OnStatusCommand(CCSPlayerController? caller, CommandInfo command)
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
            Util.ServerPrintToChat(caller, "Usage: !status <player>");
            return;
        }

        if (!Util.TryResolveSinglePlayer(query, out var target, out var error, includeBots: true) || target == null)
        {
            Util.ServerPrintToChat(caller, error);
            return;
        }

        bool wh    = Globals.Wallhackers.Contains(target);
        bool invis = Globals.InvisiblePlayers.ContainsKey(target);
        bool god   = Globals.GodPlayers.Contains(target);
        bool money = Globals.InfiniteMoneyPlayers.Contains(target);
        bool hasSpeed = Globals.SpeedPlayers.TryGetValue(target, out float speed);

        string speedStr = hasSpeed ? $"{speed:0.##}x" : "OFF";

        Util.ServerPrintToChat(caller, $"=== {target.PlayerName} ===");
        Util.ServerPrintToChat(caller, $"Wallhack: {(wh ? "ON" : "OFF")}  |  Invisible: {(invis ? "ON" : "OFF")}  |  God: {(god ? "ON" : "OFF")}");
        Util.ServerPrintToChat(caller, $"Speed: {speedStr}  |  Infinite money: {(money ? "ON" : "OFF")}");
    }
}
