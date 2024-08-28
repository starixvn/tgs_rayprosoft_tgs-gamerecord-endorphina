namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;
public record SessionProcessCommand(
	Uri ApiUrl,
	string MerchantId,
	string SecretKey,
	string Token);
