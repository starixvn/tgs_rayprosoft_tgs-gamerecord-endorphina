namespace Tgs.GameRecord.Endorphina.ViewModels;

public record SessionProcessQueryViewModel(
	string ApiUrl,
	string MerchantId,
	string SecretKey,
	string Token);
