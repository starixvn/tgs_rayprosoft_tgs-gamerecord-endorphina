namespace Tgs.GameRecord.EndorphinaService.Core.DispatchServices;
public record ScheduleDispatchCommand(
    Uri ApiUrl,
    string MerchantId,
    string SecretKey);
