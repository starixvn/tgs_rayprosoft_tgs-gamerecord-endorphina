namespace Tgs.GameRecord.Endorphina.Core.DispatchServices;
public record ScheduleDispatchCommand(
    Uri ApiUrl,
    string MerchantId,
    string SecretKey);
