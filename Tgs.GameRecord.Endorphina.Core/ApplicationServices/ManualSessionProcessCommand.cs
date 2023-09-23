namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;
public record ManualSessionProcessCommand(
    Uri ApiUrl,
    string MerchantId,
    string SecretKey,
    string Token);
