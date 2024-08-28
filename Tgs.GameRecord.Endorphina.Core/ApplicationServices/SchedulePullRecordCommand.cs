namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;
public record SchedulePullRecordCommand(
	Uri ApiUrl,
	string MerchantId,
	string SecretKey);
