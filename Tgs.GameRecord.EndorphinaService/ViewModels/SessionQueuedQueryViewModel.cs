namespace Tgs.GameRecord.EndorphinaService.ViewModels;

public record SessionQueuedQueryViewModel(
    string ApiUrl,
    string MerchantId,
    string SecretKey,
    string Token);
