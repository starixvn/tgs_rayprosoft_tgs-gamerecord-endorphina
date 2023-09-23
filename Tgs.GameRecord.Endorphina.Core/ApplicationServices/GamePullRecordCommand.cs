namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;
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
