using Microsoft.Extensions.Logging;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;

internal class ScheduleSessionProcessCommandService : ICommandService<ScheduleSessionProcessCommand>
{
    private readonly ILogger<ScheduleSessionProcessCommandService> _logger;
    private readonly IGameInterfaceEndorphinaClient _client;
    private readonly IGameRecordEndorphinaSessionClient _sessionClient;
    private readonly ISessionLock _sessionLock;
    private readonly ICommandService<GamePullRecordCommand> _service;

    public ScheduleSessionProcessCommandService(
        ILogger<ScheduleSessionProcessCommandService> logger,
        IGameInterfaceEndorphinaClient client,
        IGameRecordEndorphinaSessionClient sessionClient,
        ISessionLock sessionLock,
        ICommandService<GamePullRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _sessionClient = sessionClient ?? throw new ArgumentNullException(nameof(sessionClient));
        _sessionLock = sessionLock ?? throw new ArgumentNullException(nameof(sessionLock));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(ScheduleSessionProcessCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await _sessionLock
                    .LockAsync(token: command.Token)
                    .ConfigureAwait(false))
            {
                _logger.LogWarning("LogOn:{logOn} Message:This game provider Endorphina session has been locked. ({token})",
                    DateTime.Now.ToString("o"),
                    command.Token);
                return;
            }

            await _sessionClient
               .UpdateSessionProcessingAsync(command.Token, cancellationToken)
               .ConfigureAwait(false);

            var sessionInfo = await _client
                .GameSessionInfoAsync(
                    command.ApiUrl,
                    command.MerchantId,
                    command.SecretKey,
                    command.Token,
                    cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false)
                ?? throw new ArgumentException("sessionInfo");

            var sessionStatistics = await _client
                .GameSessionStatisticsAsync(
                    command.ApiUrl,
                    command.MerchantId,
                    command.SecretKey,
                    command.Token,
                    cancellationToken)
                .ConfigureAwait(false)
                ?? throw new ArgumentException("sessionStatistics");

            var recordCommand = new GamePullRecordCommand(
                ExternalRoundId: sessionInfo.Id,
                GameExternalId: sessionInfo.GameName,
                PlayerCode: sessionInfo.Username,
                ExternalCurrency: sessionInfo.Currency,
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

            await _sessionClient
                .UpdateSessionCompleteAsync(command.Token, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await _sessionClient
                .UpdateSessionRetryAsync(command.Token, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogError(ex, "Schedule Session Process({token})處理發生例外", command.Token);
        }
        finally
        {
            _ = await _sessionLock
                .UnlockAsync(token: command.Token)
                .ConfigureAwait(false);
        }
    }
}
