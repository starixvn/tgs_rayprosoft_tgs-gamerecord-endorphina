namespace Tgs.GameRecord.Endorphina.Core;
public record GameRecordInfoOptions
{
	public string GameProviderCode { get; set; } = default!;
	public string GameProviderConfiguration { get; init; } = default!;
	public string HangfirePrefix { get; set; } = "gamerecord-endor-hangfire:";
	public string HangfireLocking { get; set; } = "gamerecord-endor-locking:";
}
