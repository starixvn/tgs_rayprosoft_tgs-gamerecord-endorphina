namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;
public record ManualPullRecordCommand(
	Uri ApiUrl,
	string MerchantId,
	string SecretKey,
	string Token);
