namespace Tgs.GameRecord.EndorphinaService.Core.ManualWorkerServices;
public record ManualCommand(
    Uri ApiUrl,
    string MerchantId,
    string SecretKey,
    string Token,
    string PlayerCode);
