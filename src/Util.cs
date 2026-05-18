using System;
using System.Diagnostics.CodeAnalysis;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace WallhackPluginCS2;

public static class Util
{
    public static string? GetPlayerModel(CCSPlayerController player)
    {
        var pawn = player.PlayerPawn?.Value;
        return pawn?.CBodyComponent?.SceneNode?.GetSkeletonInstance()?.ModelState?.ModelName;
    }

    public static bool IsPlayerEntityValid([NotNullWhen(true)] CCSPlayerController? plr) =>
        plr != null &&
        plr.IsValid &&
        !plr.IsHLTV;

    public static bool IsPlayerValid([NotNullWhen(true)] CCSPlayerController? plr) =>
        IsPlayerEntityValid(plr) &&
        plr.PlayerPawn != null &&
        plr.PlayerPawn.IsValid &&
        plr.Connected == PlayerConnectedState.PlayerConnected;

    public static List<CCSPlayerController> GetValidPlayers() =>
        Utilities.GetPlayers().Where(IsPlayerValid).ToList();

    public static List<CCSPlayerController> GetBots() =>
        GetValidPlayers().Where(plr => plr.IsBot).ToList();

    public static List<CCSPlayerController> GetRealPlayers() =>
        GetValidPlayers().Where(plr => !plr.IsBot).ToList();

    public static List<CCSPlayerController> FindPlayerMatches(string query, bool includeBots = true)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new();

        query = query.Trim();

        IEnumerable<CCSPlayerController> players = GetValidPlayers();
        if (!includeBots)
            players = players.Where(p => !p.IsBot);

        var candidates = players.ToList();

        var exact = candidates
            .Where(p => p.PlayerName.Equals(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (exact.Count > 0)
            return exact;

        var startsWith = candidates
            .Where(p => p.PlayerName.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (startsWith.Count > 0)
            return startsWith;

        var wordStarts = candidates
            .Where(p => p.PlayerName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Any(part => part.StartsWith(query, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        if (wordStarts.Count > 0)
            return wordStarts;

        return candidates
            .Where(p => p.PlayerName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public static bool TryResolveSinglePlayer(
        string query,
        out CCSPlayerController? player,
        out string error,
        bool includeBots = true)
    {
        player = null;
        error = string.Empty;

        var matches = FindPlayerMatches(query, includeBots);

        if (matches.Count == 0)
        {
            error = $"No player found matching '{query}'.";
            return false;
        }

        if (matches.Count > 1)
        {
            string names = string.Join(", ", matches.Take(5).Select(p => p.PlayerName));
            if (matches.Count > 5)
                names += ", ...";

            error = $"Multiple matches: {names}. Be more specific.";
            return false;
        }

        player = matches[0];
        return true;
    }

    public static CCSPlayerController? GetPlayerByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var matches = FindPlayerMatches(name);
        return matches.Count == 1 ? matches[0] : null;
    }

    public static CCSPlayerController? GetPlayerByPartialName(string partialName)
    {
        if (string.IsNullOrWhiteSpace(partialName))
            return null;

        var matches = FindPlayerMatches(partialName);
        return matches.Count == 1 ? matches[0] : null;
    }

    public static List<CCSPlayerController> GetPlayersByPartialName(string partialName)
    {
        if (string.IsNullOrWhiteSpace(partialName))
            return new();

        return FindPlayerMatches(partialName);
    }

    public static void ServerPrintToChat(CCSPlayerController player, string message)
    {
        player.PrintToChat($" {ChatColors.Green}[SERVER]{ChatColors.White} {message}");
    }

    public static void Broadcast(string message)
    {
        foreach (var player in GetValidPlayers())
            ServerPrintToChat(player, message);
    }

    public static float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        if (fromMax == fromMin)
            return toMin;

        float normalized = (value - fromMin) / (fromMax - fromMin);
        normalized = Math.Clamp(normalized, 0f, 1f);

        return toMin + normalized * (toMax - toMin);
    }

    public static IEnumerable<CGameSceneNode> GetChildrenRecursive(CGameSceneNode node)
    {
        var child = node.Child;

        while (child != null)
        {
            yield return child;

            foreach (var grandChild in GetChildrenRecursive(child))
                yield return grandChild;

            child = child.NextSibling;
        }
    }
}