namespace Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;
public record SessionInfoQuery(
    Uri ApiUrl,
    string MerchantId,
    string SecretKey,
    string Token);
