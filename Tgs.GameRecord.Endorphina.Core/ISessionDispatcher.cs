namespace Tgs.GameRecord.Endorphina.Core;

public interface ISessionDispatcher
{
	ValueTask ExecuteAsync(
		Uri apiUrl,
		string merchantId,
		string secretKey,
		CancellationToken cancellationToken = default);
}
