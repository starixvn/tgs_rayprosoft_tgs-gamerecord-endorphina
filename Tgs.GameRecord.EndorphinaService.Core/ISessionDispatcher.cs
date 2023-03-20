namespace Tgs.GameRecord.EndorphinaService.Core;

public interface ISessionDispatcher
{
    ValueTask ExecuteAsync(
        Uri apiUrl,
        string merchantId,
        string secretKey,
        CancellationToken cancellationToken = default);
}
