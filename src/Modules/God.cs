using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using WallhackPluginCS2.Commands;

namespace WallhackPluginCS2.Modules;

public class God
{
    public static void OnTick()
    {
        foreach (var player in Globals.GodPlayers.ToList())
        {
            if (!Util.IsPlayerValid(player) || !player.PawnIsAlive)
                continue;

            var pawn = player.PlayerPawn?.Value;
            if (pawn == null || !pawn.IsValid || pawn.Health >= pawn.MaxHealth)
                continue;

            pawn.Health = pawn.MaxHealth;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        }
    }

    public static HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player != null)
            Globals.GodPlayers.Remove(player);

        return HookResult.Continue;
    }

    public static void Setup()
    {
        Globals.Plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

        Globals.Plugin.AddCommand("css_god", "Toggle god mode for a player", CommandGod.OnGodCommand);
        Globals.Plugin.AddCommand("god", "Toggle god mode for a player", CommandGod.OnGodCommand);
    }

    public static void Cleanup()
    {
        Globals.GodPlayers.Clear();
    }
}
