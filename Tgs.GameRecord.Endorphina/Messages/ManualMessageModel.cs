namespace Tgs.GameRecord.Endorphina.Messages;

public record ManualMessageModel(
	Uri ApiUrl,
	string MerchantId,
	string SecretKey,
	string Token);
