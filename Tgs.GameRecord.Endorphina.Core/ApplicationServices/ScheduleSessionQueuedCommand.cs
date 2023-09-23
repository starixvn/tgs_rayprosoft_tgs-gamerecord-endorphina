namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;
public record ScheduleSessionQueuedCommand(
    Uri ApiUrl,
    string MerchantId,
    string SecretKey,
    string Token);
