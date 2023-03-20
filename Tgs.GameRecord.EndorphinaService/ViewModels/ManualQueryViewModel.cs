namespace Tgs.GameRecord.EndorphinaService.ViewModels;

public record ManualQueryViewModel(
    string ApiUrl,
    string MerchantId,
    string SecretKey,
    string Token);
