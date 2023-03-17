namespace Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;
public record ManualSessionProcessCommand(
    Uri ApiUrl,
    string MerchantId,
    string SecretKey,
    string Token);
