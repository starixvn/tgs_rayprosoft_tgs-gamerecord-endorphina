namespace Tgs.GameRecord.EndorphinaService.Messages;

public record SessionQueuedMessageModel(
    Uri ApiUrl,
    string MerchantId,
    string SecretKey,
    string Token);
