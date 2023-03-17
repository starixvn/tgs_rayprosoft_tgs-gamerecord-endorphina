namespace Tgs.GameRecord.EndorphinaService.Messages;

public record ManualMessageModel(
    Uri ApiUrl,
    string MerchantId,
    string SecretKey,
    string Token);
