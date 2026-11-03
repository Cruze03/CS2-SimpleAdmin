using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Dapper;
using Microsoft.Extensions.Logging;

namespace CS2_SimpleAdmin;

internal sealed record MetricsSnapshot(long Day, long Week, long Month, long TwoMonths);

public partial class CS2_SimpleAdmin
{
    private const string MetricsDateFormat = "yyyy-MM-dd";
    private const string MetricsTimezone = "Asia/Kolkata";

    private static readonly TimeZoneInfo MetricsTimeZoneInfo = ResolveMetricsTimeZone();
    private static DateTime MetricsToday => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, MetricsTimeZoneInfo).Date;

    private static readonly HashSet<ulong> MetricsRecordedToday = [];
    private static string _metricsRecordedDate = string.Empty;

    [GameEventHandler]
    public HookResult OnPlayerConnectMetrics(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (!Config.IsCSSPanel || DatabaseProvider == null || ServerId == null) return HookResult.Continue;

        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV) return HookResult.Continue;

        var steamId = player.AuthorizedSteamID?.SteamId64 ?? player.SteamID;
        if (steamId == 0) return HookResult.Continue;

        var date = MetricsToday.ToString(MetricsDateFormat);
        if (_metricsRecordedDate != date)
        {
            _metricsRecordedDate = date;
            MetricsRecordedToday.Clear();
        }

        if (!MetricsRecordedToday.Add(steamId)) return HookResult.Continue;

        var serverId = ServerId.Value;

        Task.Run(async () =>
        {
            try
            {
                await using var connection = await DatabaseProvider.CreateConnectionAsync();
                await connection.ExecuteAsync(DatabaseProvider.GetInsertMetricQuery(), new { serverId, steamId, date });
            }
            catch (Exception ex)
            {
                Logger.LogError($"An error occurred in OnPlayerConnectMetrics: {ex.Message}");
            }
        });

        return HookResult.Continue;
    }

    private static TimeZoneInfo ResolveMetricsTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(MetricsTimezone);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning("Metrics timezone \"{Timezone}\" not found, falling back to UTC: {Message}", MetricsTimezone, ex.Message);
            return TimeZoneInfo.Utc;
        }
    }
}
