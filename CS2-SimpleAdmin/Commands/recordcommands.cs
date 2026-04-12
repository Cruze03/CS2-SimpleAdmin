using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CS2_SimpleAdmin.Managers;
using CS2_SimpleAdminApi;

namespace CS2_SimpleAdmin;

public partial class CS2_SimpleAdmin
{
    private bool IsRecording = false;

    [RequiresPermissions("@css/root")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnRecordCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player?.IsValid is false) return;

        string? customName = string.IsNullOrEmpty(info.GetArg(1)) ? null : info.GetArg(1);
        bool upload = info.GetArg(2).Equals("true", StringComparison.OrdinalIgnoreCase);

        var fileName = TriggerRecord(
            name: customName,
            upload: upload,
            reason: "manual",
            steamId: player?.SteamID.ToString());

        if (fileName != null)
            info.ReplyToCommand("Recording started. It will capture the delayed CSTV buffer.");
        else
            info.ReplyToCommand("A recording is already in progress.");
    }

    /// <summary>
    /// Starts a CSTV demo recording. After tv_delay seconds the recording is stopped,
    /// optionally uploaded to R2, and an entry is inserted into sa_records.
    /// </summary>
    /// <returns>The recording file path (relative to game dir), or null if already recording.</returns>
    public string? TriggerRecord(
        string? name        = null,
        bool    upload      = false,
        string  reason      = "manual",
        string? steamId     = null,
        int?    penaltyId   = null,
        PenaltyType? penaltyType = null)
    {
        if (IsRecording) return null;

        long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string objectKey   = name ?? $"record_{unixTimestamp}";
        string fileName    = $"records/{objectKey}";

        // Start recording — tv_delay means the stream is currently replaying
        // what happened tv_delay seconds ago, so we record for that duration
        // to capture the incident before stopping.
        Server.ExecuteCommand($"tv_record {fileName}");
        IsRecording = true;

        const float waitTime = 5.0f;

        AddTimer(waitTime, () =>
        {
            Server.ExecuteCommand("tv_stoprecord");
            IsRecording = false;

            var recordManager = new RecordManager(DatabaseProvider);
            bool shouldUpload = upload && Config.RecordSettings.AutoUpload
                                       && !string.IsNullOrEmpty(Config.R2Config.AccountId);

            if (shouldUpload)
            {
                // Full path on disk — CS2 appends .dem automatically
                string localPath = Path.Combine(Server.GameDirectory, "csgo", fileName + ".dem");
                Task.Run(async () =>
                {
                    // Wait for CS2 to finish flushing and closing the .dem file after tv_stoprecord
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    var url = await recordManager.UploadDemoAsync(localPath, objectKey);
                    await recordManager.InsertRecordAsync(objectKey, url, reason, steamId, penaltyId, penaltyType);
                });
            }
            else
            {
                Task.Run(() => recordManager.InsertRecordAsync(objectKey, null, reason, steamId, penaltyId, penaltyType));
            }
        });

        return fileName;
    }
}
