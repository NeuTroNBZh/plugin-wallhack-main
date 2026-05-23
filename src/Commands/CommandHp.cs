using System;
using System.Globalization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace WallhackPluginCS2.Commands;

public class CommandHp
{
    public static void OnHpCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (!Util.IsPlayerValid(caller))
            return;

        if (!AdminManager.PlayerHasPermissions(caller, Globals.Config.AdminPermission))
        {
            Util.ServerPrintToChat(caller, "You do not have permission to use this command.");
            return;
        }

        var tokens = command.ArgString.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0 || !int.TryParse(tokens[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int hp) || hp < 1)
        {
            Util.ServerPrintToChat(caller, "Usage: !hp <amount> [player]  (e.g. !hp 150  or  !hp 200 player)");
            return;
        }

        hp = Math.Clamp(hp, 1, 65535);

        CCSPlayerController target = caller!;
        if (tokens.Length >= 2)
        {
            if (!Util.TryResolveSinglePlayer(tokens[1], out var found, out var error, includeBots: true) || found == null)
            {
                Util.ServerPrintToChat(caller, error);
                return;
            }
            target = found;
        }

        SetHp(caller, target, hp);
    }

    public static void SetHp(CCSPlayerController? admin, CCSPlayerController target, int hp)
    {
        if (!Util.IsPlayerValid(target) || !target.PawnIsAlive)
            return;

        var pawn = target.PlayerPawn?.Value;
        if (pawn == null || !pawn.IsValid)
            return;

        hp = Math.Clamp(hp, 1, 65535);
        pawn.Health = hp;
        if (hp > pawn.MaxHealth)
            pawn.MaxHealth = hp;
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");

        bool isSelf = admin?.Slot == target.Slot;
        bool silent = admin != null && SimpleAdminBridge.IsAdminSilent(admin);

        if (isSelf)
        {
            Util.ServerPrintToChat(target, $"HP set to {hp}.");
        }
        else
        {
            if (admin != null)
                Util.ServerPrintToChat(admin, $"HP set to {hp} for {target.PlayerName}.");
            if (!silent)
                Util.ServerPrintToChat(target, $"Your HP has been set to {hp}.");
        }
    }
}
