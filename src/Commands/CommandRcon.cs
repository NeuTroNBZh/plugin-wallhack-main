using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace WallhackPluginCS2.Commands;

public class CommandRcon
{
    public static void OnRconCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (caller == null || !caller.IsValid)
            return;

        if (!AdminManager.PlayerHasPermissions(caller, Globals.Config.RconPermission))
        {
            Util.ServerPrintToChat(caller, "You do not have permission to use this command.");
            return;
        }

        if (string.IsNullOrWhiteSpace(command.ArgString))
        {
            Util.ServerPrintToChat(caller, "Usage: !rcon <command>");
            return;
        }

        string cmd = command.ArgString.Trim();

        string[] blockedCommands = { "quit", "exit", "restart" };
        if (blockedCommands.Any(b => cmd.StartsWith(b, StringComparison.OrdinalIgnoreCase)))
        {
            Util.ServerPrintToChat(caller, "This command is blocked.");
            return;
        }

        Server.ExecuteCommand(cmd);

        Util.ServerPrintToChat(caller, $"Executed: {cmd}");
    }
}