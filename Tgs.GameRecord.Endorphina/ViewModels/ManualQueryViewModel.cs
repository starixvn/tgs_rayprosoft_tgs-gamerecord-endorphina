namespace Tgs.GameRecord.Endorphina.ViewModels;

public record ManualQueryViewModel(
    string ApiUrl,
    string MerchantId,
    string SecretKey,
    string Token);
