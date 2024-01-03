using Microsoft.Extensions.Logging;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;

internal class SessionInfoGetService(
    IGameInterfaceEndorphinaClient client,
    ILogger<SessionInfoGetService> logger) : IGetService<SessionInfoQuery, SessionInfoResult?>
{
    public async ValueTask<SessionInfoResult?> GetAsync(SessionInfoQuery query, CancellationToken cancellationToken = default)
    {
        var sessionInfo = await client
                .GameSessionInfoAsync(
                    query.ApiUrl,
                    query.MerchantId,
                    query.SecretKey,
                    query.Token,
                    cancellationToken)
                .ConfigureAwait(false);

        if (sessionInfo == null)
        {
            logger.LogInformation("No Session Info (Session Id:{token})", query.Token);
            return null;
        }

        if (sessionInfo.ErrorsPending != 0)
        {
            logger.LogInformation("Please contact endorphina support for resolution (Session Id:{token})", query.Token);
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
