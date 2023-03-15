namespace Tgs.GameRecord.EndorphinaService.Core;
public record GameRecordManualCommand(
    Uri ApiUrl,
    string MerchantId,
    string SecretKey,
    string Token);
