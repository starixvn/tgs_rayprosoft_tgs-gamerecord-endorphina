namespace Tgs.GameRecord.EndorphinaService.ViewModels;

public record SessionProcessQueryViewModel(
    string ApiUrl,
    string MerchantId,
    string SecretKey,
    string Token);
