namespace Tgs.GameRecord.EndorphinaService.Core;
public record GameRecordScheduleCommand(
    Uri ApiUrl,
    string MerchantId,
    string SecretKey,
    string Token);
