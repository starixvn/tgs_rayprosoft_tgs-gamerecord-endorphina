namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;
public record ScheduleSessionProcessCommand(
	Uri ApiUrl,
	string MerchantId,
	string SecretKey,
	string Token);
