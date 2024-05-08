namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;
public record SessionInfoResult(
	string PlayerCode,
	string ExternalCurrency,
	string ExternalRoundId,
	string GameExternalId,
	DateTimeOffset ExternalCreatedOn,
	DateTimeOffset ExternalClosedOn);
