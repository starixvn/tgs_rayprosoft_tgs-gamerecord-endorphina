namespace Tgs.GameRecord.EndorphinaService.Core;
public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;
    public double IntervalMinutes { get; set; } = 1;
    public double IntervalMinutesMax { get; set; } = 60;
    public double IntervalMinutesMinimum { get; set; } = 1;
    public double LockMinute { get; set; } = 1;
}
