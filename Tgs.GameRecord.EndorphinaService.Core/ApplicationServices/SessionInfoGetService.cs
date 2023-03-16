using Microsoft.Extensions.Logging;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;

internal class SessionInfoGetService : IGetService<SessionInfoQuery, SessionInfoResult?>
{
    private readonly IGameInterfaceEndorphinaClient _client;
    private readonly ILogger<SessionInfoGetService> _logger;

    public SessionInfoGetService(
        IGameInterfaceEndorphinaClient client,
        ILogger<SessionInfoGetService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<SessionInfoResult?> GetAsync(SessionInfoQuery query, CancellationToken cancellationToken = default)
    {
        var sessionInfo = await _client
                .GameSessionInfoAsync(
                    query.ApiUrl,
                    query.MerchantId,
                    query.SecretKey,
                    query.Token,
                    cancellationToken)
                .ConfigureAwait(false);

        if (sessionInfo == null)
        {
            _logger.LogInformation("No Session Info (Session Id:{token})", query.Token);
            return null;
        }

        if (sessionInfo.ErrorsPending != 0)
        {
            _logger.LogInformation("Please contact endorphina support for resolution (Session Id:{token})", query.Token);
            return null;
        }

        return new SessionInfoResult(
            PlayerCode: sessionInfo.Username,
            ExternalCurrency: sessionInfo.Currency,
            ExternalRoundId: sessionInfo.Id,
            GameExternalId: sessionInfo.GameName,
            ExternalCreatedOn: sessionInfo.Created.ToDateTimeOffset(),
            ExternalClosedOn: sessionInfo.Closed.ToDateTimeOffset());
    }
}
