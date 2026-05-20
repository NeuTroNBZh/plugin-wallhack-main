using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace WallhackPluginCS2.Commands;

public class CommandGod
{
    public static void OnGodCommand(CCSPlayerController? caller, CommandInfo command)
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
            Util.ServerPrintToChat(caller, "Usage: !god <player>");
            return;
        }

        if (!Util.TryResolveSinglePlayer(query, out var target, out var error, includeBots: true) || target == null)
        {
            Util.ServerPrintToChat(caller, error);
            return;
        }

        if (Globals.GodPlayers.Remove(target))
        {
            Util.ServerPrintToChat(caller, $"God mode OFF for {target.PlayerName}.");
            Util.ServerPrintToChat(target, "God mode has been removed.");
        }
        else
        {
            Globals.GodPlayers.Add(target);
            Util.ServerPrintToChat(caller, $"God mode ON for {target.PlayerName}.");
            Util.ServerPrintToChat(target, "You now have god mode!");
        }
    }
}
