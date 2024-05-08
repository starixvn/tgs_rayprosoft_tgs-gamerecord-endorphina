namespace Tgs.GameRecord.Endorphina.ViewModels;

public record SessionQueuedQueryViewModel(
	string ApiUrl,
	string MerchantId,
	string SecretKey,
	string Token);
