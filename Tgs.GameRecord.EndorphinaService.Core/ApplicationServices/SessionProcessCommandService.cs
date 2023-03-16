using Microsoft.Extensions.Logging;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;

internal class SessionProcessCommandService : ICommandService<SessionProcessCommand>
{
    private readonly ILogger<SessionProcessCommandService> _logger;
    private readonly IGetService<SessionInfoQuery, SessionInfoResult?> _getService;
    private readonly IGameInterfaceEndorphinaClient _client;
    private readonly IGameRecordEndorphinaSessionClient _sessionClient;
    private readonly ISessionLock _sessionLock;
    private readonly ICommandService<GamePullRecordCommand> _service;

    public SessionProcessCommandService(
        ILogger<SessionProcessCommandService> logger,
        IGetService<SessionInfoQuery, SessionInfoResult?> getService,
        IGameInterfaceEndorphinaClient client,
        IGameRecordEndorphinaSessionClient sessionClient,
        ISessionLock sessionLock,
        ICommandService<GamePullRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _getService = getService ?? throw new ArgumentNullException(nameof(getService));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _sessionClient = sessionClient ?? throw new ArgumentNullException(nameof(sessionClient));
        _sessionLock = sessionLock ?? throw new ArgumentNullException(nameof(sessionLock));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(SessionProcessCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessionInfo = await _getService
                .GetAsync(new SessionInfoQuery(
                    command.ApiUrl,
                    command.MerchantId,
                    command.SecretKey,
                    command.Token),
                    cancellationToken)
                .ConfigureAwait(false);

            if (sessionInfo is null)
                return;

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

            var sessionStatistics = await _client
                .GameSessionStatisticsAsync(
                    command.ApiUrl,
                    command.MerchantId,
                    command.SecretKey,
                    command.Token,
                    cancellationToken)
                .ConfigureAwait(false);

            var recordCommand = new GamePullRecordCommand(
                ExternalRoundId: sessionInfo.ExternalRoundId,
                GameExternalId: sessionInfo.GameExternalId,
                PlayerCode: sessionInfo.PlayerCode,
                ExternalCurrency: sessionInfo.ExternalCurrency,
                ExternalCreatedOn: sessionInfo.ExternalCreatedOn,
                ExternalClosedOn: sessionInfo.ExternalClosedOn,
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

            _logger.LogError(ex, "Session Process({token})處理發生例外", command.Token);
        }
        finally
        {
            _ = await _sessionLock
                .UnlockAsync(token: command.Token)
                .ConfigureAwait(false);
        }
    }
}
