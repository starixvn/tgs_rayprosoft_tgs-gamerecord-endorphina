namespace Tgs.GameRecord.EndorphinaService.Core;
public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;
    public double LockMinute { get; set; } = 1;
}
