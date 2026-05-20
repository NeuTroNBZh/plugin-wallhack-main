using CounterStrikeSharp.API.Core;
using WallhackPluginCS2.Commands;

namespace WallhackPluginCS2.Modules;

public class Speed
{
    internal const float DefaultMultiplier = 1.5f;

    public static void OnTick()
    {
        foreach (var (player, multiplier) in Globals.SpeedPlayers.ToList())
        {
            if (!Util.IsPlayerValid(player) || !player.PawnIsAlive)
                continue;

            var pawn = player.PlayerPawn?.Value;
            if (pawn == null || !pawn.IsValid)
                continue;

            pawn.VelocityModifier = multiplier;
        }
    }

    public static HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player != null)
            Globals.SpeedPlayers.Remove(player);

        return HookResult.Continue;
    }

    public static void Setup()
    {
        Globals.Plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

        Globals.Plugin.AddCommand("css_speed", "Toggle speed boost for a player", CommandSpeed.OnSpeedCommand);
        Globals.Plugin.AddCommand("speed", "Toggle speed boost for a player", CommandSpeed.OnSpeedCommand);
    }

    public static void Cleanup()
    {
        Globals.SpeedPlayers.Clear();
    }
}
