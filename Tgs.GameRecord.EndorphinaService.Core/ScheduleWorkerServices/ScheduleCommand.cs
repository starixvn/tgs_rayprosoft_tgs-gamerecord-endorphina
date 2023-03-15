using Tgs.GameRecord.Endorphina.Session.Protos;

namespace Tgs.GameRecord.EndorphinaService.Core.ScheduleWorkerServices;
public record ScheduleCommand(
    Uri ApiUrl,
    string MerchantId,
    string SecretKey,
    Session Session);
