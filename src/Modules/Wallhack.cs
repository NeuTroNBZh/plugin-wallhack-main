using System.Drawing;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using WallhackPluginCS2.Commands;
using WallhackPluginCS2.Models;

namespace WallhackPluginCS2.Modules;

public class Wallhack
{
    private static readonly HashSet<int> PendingGlowSlots = new();

    public static void OnPlayerTransmit(CCheckTransmitInfo info, CCSPlayerController viewer)
    {
        if (!Util.IsPlayerValid(viewer))
            return;

        foreach (var (target, data) in Globals.GlowData.ToList())
        {
            if (!Util.IsPlayerEntityValid(target) || !data.GlowEnt.IsValid || !data.ModelRelay.IsValid)
            {
                RemoveGlow(target);
                continue;
            }

            if (target.Slot == viewer.Slot || !target.PawnIsAlive)
            {
                info.TransmitEntities.Remove(data.ModelRelay);
                info.TransmitEntities.Remove(data.GlowEnt);
                continue;
            }

            bool isInvisible = Globals.InvisiblePlayers.TryGetValue(target, out var invisData);
            bool isRevealed = !isInvisible || Server.CurrentTime <= invisData.RevealUntil;

            bool shouldSee =
                Globals.Wallhackers.Contains(viewer) &&
                viewer.Team != CsTeam.Spectator &&
                target.Team != CsTeam.Spectator &&
                target.Team != viewer.Team &&
                isRevealed;

            if (shouldSee)
            {
                info.TransmitEntities.Add(data.ModelRelay);
                info.TransmitEntities.Add(data.GlowEnt);
            }
            else
            {
                info.TransmitEntities.Remove(data.ModelRelay);
                info.TransmitEntities.Remove(data.GlowEnt);
            }
        }
    }

    public static HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!Util.IsPlayerEntityValid(player))
            return HookResult.Continue;

        ScheduleGlow(player);
        return HookResult.Continue;
    }

    public static HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null)
            return HookResult.Continue;

        PendingGlowSlots.Remove(player.Slot);
        RemoveGlow(player);
        Globals.Wallhackers.Remove(player);
        Globals.InvisiblePlayers.Remove(player);

        return HookResult.Continue;
    }

    public static HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null)
            return HookResult.Continue;

        PendingGlowSlots.Remove(player.Slot);
        RemoveGlow(player);

        return HookResult.Continue;
    }

    private static void ScheduleGlow(CCSPlayerController player)
    {
        PendingGlowSlots.Remove(player.Slot);
        RemoveGlow(player);

        if (!PendingGlowSlots.Add(player.Slot))
            return;

        Globals.Plugin.AddTimer(0.20f, () =>
        {
            PendingGlowSlots.Remove(player.Slot);

            if (!Util.IsPlayerValid(player) || !player.PawnIsAlive)
                return;

            RemoveGlow(player);
            CreateGlow(player);
        });
    }

    private static void RemoveGlow(CCSPlayerController player)
    {
        if (!Globals.GlowData.TryGetValue(player, out var data))
            return;

        if (data.GlowEnt.IsValid)
            data.GlowEnt.Remove();

        if (data.ModelRelay.IsValid)
            data.ModelRelay.Remove();

        Globals.GlowData.Remove(player);
    }

    private static void CreateGlow(CCSPlayerController player)
    {
        if (Globals.GlowData.ContainsKey(player))
            return;

        var pawn = player.PlayerPawn?.Value;
        if (pawn == null || !pawn.IsValid || !player.PawnIsAlive)
            return;

        string? model = Util.GetPlayerModel(player);
        if (string.IsNullOrWhiteSpace(model) || !model.EndsWith(".vmdl", StringComparison.OrdinalIgnoreCase))
            return;

        var modelRelay = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
        var glowEntity = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");

        if (modelRelay == null || glowEntity == null)
            return;

        modelRelay.Spawnflags = 256;
        modelRelay.Render = Color.Transparent;
        modelRelay.RenderMode = RenderMode_t.kRenderNone;

        glowEntity.Spawnflags = 256;
        glowEntity.Render = Color.FromArgb(1, 0, 0, 0);

        modelRelay.SetModel(model);
        glowEntity.SetModel(model);

        modelRelay.DispatchSpawn();
        glowEntity.DispatchSpawn();

        bool isT = player.Team == CsTeam.Terrorist;
        glowEntity.Glow.GlowRange = 5000;
        glowEntity.Glow.GlowRangeMin = 0;
        glowEntity.Glow.GlowColorOverride = isT
            ? Color.FromArgb(255, Globals.Config.R, Globals.Config.G, Globals.Config.B)
            : Color.FromArgb(255, Globals.Config.CTR, Globals.Config.CTG, Globals.Config.CTB);
        glowEntity.Glow.GlowTeam = isT ? (int)CsTeam.CounterTerrorist : (int)CsTeam.Terrorist;
        glowEntity.Glow.GlowType = 3;

        Globals.GlowData[player] = new GlowData
        {
            GlowEnt = glowEntity,
            ModelRelay = modelRelay
        };

        Server.NextFrame(() =>
        {
            if (!Util.IsPlayerValid(player) || !player.PawnIsAlive)
            {
                RemoveGlow(player);
                return;
            }

            if (!modelRelay.IsValid || !glowEntity.IsValid)
            {
                RemoveGlow(player);
                return;
            }

            var livePawn = player.PlayerPawn?.Value;
            if (livePawn == null || !livePawn.IsValid)
            {
                RemoveGlow(player);
                return;
            }

            modelRelay.AcceptInput("FollowEntity", livePawn, modelRelay, "!activator");
            glowEntity.AcceptInput("FollowEntity", modelRelay, glowEntity, "!activator");
        });
    }

    public static void Setup()
    {
        Globals.Plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        Globals.Plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        Globals.Plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn, HookMode.Post);

        Globals.Plugin.AddCommand("css_wh", "Gives a player walls", CommandWallhack.OnWallhackCommand);
        Globals.Plugin.AddCommand("css_wallhack", "Gives a player walls", CommandWallhack.OnWallhackCommand);
        Globals.Plugin.AddCommand("wh", "Gives a player walls", CommandWallhack.OnWallhackCommand);
        Globals.Plugin.AddCommand("wallhack", "Gives a player walls", CommandWallhack.OnWallhackCommand);

        Globals.Plugin.AddTimer(0.50f, () =>
        {
            foreach (var player in Util.GetValidPlayers().Where(p => p.PawnIsAlive))
                ScheduleGlow(player);
        });
    }

    public static void Cleanup()
    {
        PendingGlowSlots.Clear();

        foreach (var data in Globals.GlowData.Values)
        {
            if (data.GlowEnt.IsValid)
                data.GlowEnt.Remove();

            if (data.ModelRelay.IsValid)
                data.ModelRelay.Remove();
        }

        Globals.GlowData.Clear();
        Globals.Wallhackers.Clear();
    }
}