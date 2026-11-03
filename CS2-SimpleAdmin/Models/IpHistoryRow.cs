namespace CS2_SimpleAdmin.Models;

public record IpHistoryRow
{
    public long Steamid { get; init; }
    public string? Name { get; init; }
    public uint Address { get; init; }
    public DateTime Used_at { get; init; }
}
