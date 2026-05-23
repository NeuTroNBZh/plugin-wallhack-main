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
            Toggle(caller!, caller!);
            return;
        }

        if (!Util.TryResolveSinglePlayer(query, out var target, out var error, includeBots: true) || target == null)
        {
            Util.ServerPrintToChat(caller, error);
            return;
        }

        Toggle(caller!, target);
    }

    public static void Toggle(CCSPlayerController caller, CCSPlayerController target)
    {
        bool isSelf = caller.Slot == target.Slot;
        bool silent = SimpleAdminBridge.IsAdminSilent(caller);
        bool activated = !Globals.GodPlayers.Remove(target);

        if (activated)
            Globals.GodPlayers.Add(target);

        if (isSelf)
        {
            Util.ServerPrintToChat(caller, activated ? "God mode ON." : "God mode OFF.");
        }
        else
        {
            Util.ServerPrintToChat(caller, activated
                ? $"God mode ON for {target.PlayerName}."
                : $"God mode OFF for {target.PlayerName}.");
            if (!silent)
                Util.ServerPrintToChat(target, activated
                    ? "You now have god mode!"
                    : "God mode has been removed.");
        }
    }
}
