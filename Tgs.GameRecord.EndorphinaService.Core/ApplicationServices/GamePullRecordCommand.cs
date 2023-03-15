using Tgs.GameRecord.Endorphina.Session.Protos;

namespace Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;
public record GamePullRecordCommand(
    Uri ApiUrl,
    string MerchantId,
    string SecretKey,
    Session Session);
