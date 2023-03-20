namespace Tgs.GameRecord.EndorphinaService.Messages;

public record ScheduleMessageModel(
    Uri ApiUrl,
    string MerchantId,
    string SecretKey);
