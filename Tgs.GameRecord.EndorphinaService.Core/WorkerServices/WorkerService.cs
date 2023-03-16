using Microsoft.Extensions.Logging;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core.WorkerServices;

internal class WorkerService : ICommandService<WorkCommand>
{
    private readonly ILogger<WorkerService> _logger;
    private readonly IGameInterfaceEndorphinaClient _client;
    private readonly IGameRecordEndorphinaSessionClient _sessionClient;
    private readonly ICommandService<GamePullRecordCommand> _service;

    public WorkerService(
        ILogger<WorkerService> logger,
        IGameInterfaceEndorphinaClient client,
        IGameRecordEndorphinaSessionClient sessionClient,
        ICommandService<GamePullRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _sessionClient = sessionClient ?? throw new ArgumentNullException(nameof(sessionClient));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(WorkCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessionInfo = await _client
                .GameSessionInfoAsync(
                    command.ApiUrl,
                    command.MerchantId,
                    command.SecretKey,
                    command.Token,
                    cancellationToken)
                .ConfigureAwait(false);

            if (sessionInfo == null)
            {
                _logger.LogInformation("No Session Info (Session Id:{token})", command.Token);
                return;
            }

            if (sessionInfo.ErrorsPending != 0)
            {
                _logger.LogInformation("Please contact endorphina support for resolution (Session Id:{token})", command.Token);
                return;
            }

            var sessionStatistics = await _client
                .GameSessionStatisticsAsync(
                    command.ApiUrl,
                    command.MerchantId,
                    command.SecretKey,
                    command.Token,
                    cancellationToken)
                .ConfigureAwait(false);

            var recordCommand = new GamePullRecordCommand(
                ExternalRoundId: sessionInfo.Id,
                GameExternalId: sessionInfo.GameName,
                PlayerCode: sessionInfo.Username,
                Currency: sessionInfo.Currency,
                ExternalCreatedOn: sessionInfo.Created.ToDateTimeOffset(),
                ExternalClosedOn: sessionInfo.Closed.ToDateTimeOffset(),
                Bet: sessionStatistics.Bet.ToDecimal().GetValueOrDefault(0.0m),
                Win: sessionStatistics.Win.ToDecimal().GetValueOrDefault(0.0m),
                Games: sessionStatistics.Games,
                Jackpots: sessionStatistics.Jackpots,
                JackpotWin: sessionStatistics.JackpotWin);

            await _service
                .ExecuteAsync(recordCommand, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await _sessionClient.UpdateSessionRetryAsync(command.Token, cancellationToken).ConfigureAwait(false);
            _logger.LogError(ex, "Session({token})處理發生例外", command.Token);
        }
    }
}
