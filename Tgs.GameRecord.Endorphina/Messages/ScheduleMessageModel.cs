namespace Tgs.GameRecord.Endorphina.Messages;

public record ScheduleMessageModel(
	Uri ApiUrl,
	string MerchantId,
	string SecretKey);
