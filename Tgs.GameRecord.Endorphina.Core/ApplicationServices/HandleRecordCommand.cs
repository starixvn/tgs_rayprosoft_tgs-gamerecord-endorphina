namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;
public record HandleRecordCommand(
	string ExternalRoundId,
	string ExternalGameId,
	string PlayerCode,
	string ExternalCurrency,
	DateTimeOffset ExternalCreatedOn,
	DateTimeOffset ExternalClosedOn,
	decimal Bet,
	decimal Win,
	int Games,
	int Jackpots,
	decimal JackpotWin);
