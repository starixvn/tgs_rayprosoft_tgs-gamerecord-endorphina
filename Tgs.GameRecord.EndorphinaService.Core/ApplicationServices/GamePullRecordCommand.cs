using Tgs.GameRecord.Endorphina.Session.Protos;

namespace Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;
public record GamePullRecordCommand(    
    string ExternalRoundId,
    string GameExternalId,
    string PlayerCode,
    string ExternalCurrency,    
    DateTimeOffset ExternalCreatedOn,
    DateTimeOffset ExternalClosedOn,
    decimal Bet,
    decimal Win,
    int Games,
    int Jackpots,
    decimal JackpotWin);
