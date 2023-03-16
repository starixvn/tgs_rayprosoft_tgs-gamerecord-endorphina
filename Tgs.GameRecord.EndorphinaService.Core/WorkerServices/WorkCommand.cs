using Tgs.GameRecord.Endorphina.Session.Protos;

namespace Tgs.GameRecord.EndorphinaService.Core.WorkerServices;
public record WorkCommand(
    Uri ApiUrl,
    string MerchantId,
    string SecretKey,
    string Token);
