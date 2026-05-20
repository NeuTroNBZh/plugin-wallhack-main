using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace WallhackPluginCS2.Commands;

public class CommandSlap
{
    public static void OnSlapCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (!Util.IsPlayerValid(caller))
            return;

        if (!AdminManager.PlayerHasPermissions(caller, Globals.Config.AdminPermission))
        {
            Util.ServerPrintToChat(caller, "You do not have permission to use this command.");
            return;
        }

        var args = command.ArgString.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (args.Length < 2 || !int.TryParse(args[0], out int damage) || damage < 0)
        {
            Util.ServerPrintToChat(caller, "Usage: !slap <damage> <player>");
            return;
        }

        string query = args[1].Trim();

        if (!Util.TryResolveSinglePlayer(query, out var target, out var error, includeBots: true) || target == null)
        {
            Util.ServerPrintToChat(caller, error);
            return;
        }

        if (!target.PawnIsAlive)
        {
            Util.ServerPrintToChat(caller, $"{target.PlayerName} is already dead.");
            return;
        }

        var pawn = target.PlayerPawn?.Value;
        if (pawn == null || !pawn.IsValid)
            return;

        int newHealth = pawn.Health - damage;
        if (newHealth <= 0)
        {
            pawn.CommitSuicide(false, true);
        }
        else
        {
            pawn.Health = newHealth;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        }

        Util.ServerPrintToChat(caller, $"Slapped {target.PlayerName} for {damage} damage.");
    }
}
