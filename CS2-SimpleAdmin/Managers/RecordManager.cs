using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using CS2_SimpleAdmin.Database;
using CS2_SimpleAdminApi;
using Dapper;
using Microsoft.Extensions.Logging;

namespace CS2_SimpleAdmin.Managers;

internal class RecordManager(IDatabaseProvider? databaseProvider)
{
    /// <summary>
    /// Uploads a demo file to Cloudflare R2 and returns the public URL, or null on failure.
    /// </summary>
    public async Task<string?> UploadDemoAsync(string localFilePath, string objectKey)
    {
        var cfg = CS2_SimpleAdmin.Instance.Config.R2Config;

        if (string.IsNullOrEmpty(cfg.AccountId) || string.IsNullOrEmpty(cfg.AccessKeyId))
        {
            CS2_SimpleAdmin._logger?.LogWarning("R2 upload skipped: R2Config is not configured.");
            return null;
        }

        if (!File.Exists(localFilePath))
        {
            CS2_SimpleAdmin._logger?.LogWarning("R2 upload skipped: file not found at {Path}", localFilePath);
            return null;
        }

        try
        {
            var credentials = new BasicAWSCredentials(cfg.AccessKeyId, cfg.SecretAccessKey);
            var s3Config = new AmazonS3Config
            {
                ServiceURL = $"https://{cfg.AccountId}.r2.cloudflarestorage.com",
                ForcePathStyle = true,
                // R2 does not use AWS region signing — us-east-1 is the conventional placeholder
                AuthenticationRegion = "auto"
            };

            using var s3Client = new AmazonS3Client(credentials, s3Config);

            var s3Key = $"{cfg.Prefix.TrimEnd('/')}/{objectKey}.dem";

            using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = cfg.BucketName,
                Key = s3Key,
                InputStream = fileStream,
                // R2 does not support STREAMING-AWS4-HMAC-SHA256-PAYLOAD-TRAILER;
                // disabling payload signing sends a standard non-chunked request.
                DisablePayloadSigning = true
            });

            // Build the public URL from the configured base URL
            // var publicUrl = string.IsNullOrEmpty(cfg.PublicBaseUrl)
            //     ? null
            //     : $"{cfg.PublicBaseUrl.TrimEnd('/')}/{s3Key}";

            CS2_SimpleAdmin._logger?.LogInformation("Demo uploaded to R2: {Key}", s3Key);
            return s3Key;
        }
        catch (Exception ex)
        {
            CS2_SimpleAdmin._logger?.LogError(ex, "Failed to upload demo {Key} to R2", objectKey);
            return null;
        }
    }

    /// <summary>
    /// Inserts a record entry into sa_records.
    /// </summary>
    public async Task InsertRecordAsync(
        string name,
        string? url,
        string reason,
        string? steamId,
        int? penaltyId,
        PenaltyType? penaltyType)
    {
        if (databaseProvider == null) return;

        try
        {
            await using var connection = await databaseProvider.CreateConnectionAsync();
            var sql = databaseProvider.GetInsertRecordQuery();

            await connection.ExecuteScalarAsync<int?>(sql, new
            {
                name,
                url,
                reason,
                steamId,
                penaltyId,
                penaltyType = (int?)penaltyType,
                created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                serverId = CS2_SimpleAdmin.ServerId
            });
        }
        catch (Exception ex)
        {
            CS2_SimpleAdmin._logger?.LogError(ex, "Failed to insert sa_records entry for {Name}", name);
        }
    }
}
