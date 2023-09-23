namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;
public record SessionInfoQuery(
    Uri ApiUrl,
    string MerchantId,
    string SecretKey,
    string Token);
