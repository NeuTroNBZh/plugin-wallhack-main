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

    // True once CSS EventRoundStart fires (which is AFTER all breakerandopendoor scans).
    // Used to block glow creation during the round-start entity-scan window.
    private static bool _roundInProgress = false;

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

    public static HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        // Mark round as not in progress so OnPlayerSpawn ignores the next spawn wave.
        // Remove all glow entities now so they are gone before the next round's
        // engine initialization and breakerandopendoor scans.
        _roundInProgress = false;
        PendingGlowSlots.Clear();
        foreach (var player in Globals.GlowData.Keys.ToList())
            RemoveGlow(player);

        return HookResult.Continue;
    }

    public static HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        // CSS EventRoundStart fires AFTER all breakerandopendoor entity scans, so it
        // is safe to create glow entities from here. OnPlayerSpawn fires BEFORE these
        // scans and must not create entities (guarded by _roundInProgress below).
        _roundInProgress = true;
        Globals.Plugin.AddTimer(0.5f, () =>
        {
            foreach (var player in Util.GetValidPlayers().Where(p => p.PawnIsAlive))
                ScheduleGlow(player);
        });

        return HookResult.Continue;
    }

    public static HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!Util.IsPlayerEntityValid(player))
            return HookResult.Continue;

        // At round start, PlayerSpawn fires during the engine's entity-scan window
        // (before EventRoundStart). Creating CDynamicProp entities during that window
        // causes "WriteEnterPVS: GetEntServerClass failed" crashes.
        // OnRoundStart handles glow creation for the round-start spawn wave.
        // This path only handles mid-round spawns (e.g. after death in retake).
        if (_roundInProgress)
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

        Globals.Plugin.AddTimer(0.5f, () =>
        {
            PendingGlowSlots.Remove(player.Slot);

            // CSS timers cannot be cancelled. A timer queued in round N can fire
            // after OnRoundEnd sets _roundInProgress = false, during round N+1's
            // engine scan window. Guard here to prevent CreateGlow in that case.
            if (!_roundInProgress)
                return;

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
        if (modelRelay == null)
            return;

        var glowEntity = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
        if (glowEntity == null)
        {
            // modelRelay is alive in the staging list — spawn then remove it so the
            // engine never tries to enter it into a PVS without a ServerClass.
            modelRelay.DispatchSpawn();
            modelRelay.Remove();
            return;
        }

        // Only non-replicated flags before DispatchSpawn — setting Render / RenderMode
        // before spawning puts network state on a staging entity, which triggers the
        // EF_IN_STAGING_LIST assertion and can lead to the WriteEnterPVS crash.
        modelRelay.Spawnflags = 256;
        glowEntity.Spawnflags = 256;

        modelRelay.SetModel(model);
        glowEntity.SetModel(model);

        modelRelay.DispatchSpawn();
        glowEntity.DispatchSpawn();

        // Register before NextFrame so RemoveGlow can clean up if validation fails.
        Globals.GlowData[player] = new GlowData { GlowEnt = glowEntity, ModelRelay = modelRelay };

        // Apply all visual and glow properties in the next frame.
        // By then both entities are fully registered and out of the staging list,
        // so modifying their network state is safe.
        Server.NextFrame(() =>
        {
            if (!Globals.GlowData.ContainsKey(player) || !modelRelay.IsValid || !glowEntity.IsValid)
            {
                RemoveGlow(player);
                return;
            }

            if (!Util.IsPlayerValid(player) || !player.PawnIsAlive)
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

            modelRelay.Render = Color.Transparent;
            modelRelay.RenderMode = RenderMode_t.kRenderNone;
            glowEntity.Render = Color.FromArgb(1, 0, 0, 0);

            bool isT = player.Team == CsTeam.Terrorist;
            glowEntity.Glow.GlowRange = 5000;
            glowEntity.Glow.GlowRangeMin = 0;
            glowEntity.Glow.GlowColorOverride = isT
                ? Color.FromArgb(255, Globals.Config.R, Globals.Config.G, Globals.Config.B)
                : Color.FromArgb(255, Globals.Config.CTR, Globals.Config.CTG, Globals.Config.CTB);
            glowEntity.Glow.GlowTeam = isT ? (int)CsTeam.CounterTerrorist : (int)CsTeam.Terrorist;
            glowEntity.Glow.GlowType = 3;

            modelRelay.AcceptInput("FollowEntity", livePawn, modelRelay, "!activator");
            glowEntity.AcceptInput("FollowEntity", modelRelay, glowEntity, "!activator");
        });
    }

    public static void Setup()
    {
        Globals.Plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        Globals.Plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        Globals.Plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        Globals.Plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        Globals.Plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn, HookMode.Post);

        Globals.Plugin.AddCommand("css_wh", "Gives a player walls", CommandWallhack.OnWallhackCommand);
        Globals.Plugin.AddCommand("css_wallhack", "Gives a player walls", CommandWallhack.OnWallhackCommand);
        Globals.Plugin.AddCommand("wh", "Gives a player walls", CommandWallhack.OnWallhackCommand);
        Globals.Plugin.AddCommand("wallhack", "Gives a player walls", CommandWallhack.OnWallhackCommand);

        // Hot-reload: a round is already in progress, create glows for alive players.
        Globals.Plugin.AddTimer(0.5f, () =>
        {
            _roundInProgress = true;
            foreach (var player in Util.GetValidPlayers().Where(p => p.PawnIsAlive))
                ScheduleGlow(player);
        });
    }

    public static void Cleanup()
    {
        _roundInProgress = false;
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
