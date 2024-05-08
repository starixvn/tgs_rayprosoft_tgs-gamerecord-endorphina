namespace Tgs.GameRecord.Endorphina.Core.JobApplicationServices;

public record JobExecuteCommand(
	Uri ApiUrl,
	string MerchantId,
	string SecretKey);
