namespace Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;
public record SessionQueuedCommand(
    Uri ApiUrl,
    string MerchantId,
    string SecretKey,
    string Token);
