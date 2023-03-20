using Microsoft.Extensions.Logging;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;

internal class ManualSessionProcessCommandService : ICommandService<ManualSessionProcessCommand>
{
    private readonly ILogger<ManualSessionProcessCommandService> _logger;
    private readonly IGameInterfaceEndorphinaClient _client;
    private readonly ISessionLock _sessionLock;
    private readonly ICommandService<GamePullRecordCommand> _service;

    public ManualSessionProcessCommandService(
        ILogger<ManualSessionProcessCommandService> logger,
        IGameInterfaceEndorphinaClient client,
        ISessionLock sessionLock,
        ICommandService<GamePullRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _sessionLock = sessionLock ?? throw new ArgumentNullException(nameof(sessionLock));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(ManualSessionProcessCommand command, CancellationToken cancellationToken = default)
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Manual Session Process({token})處理發生例外", command.Token);
        }
        finally
        {
            _ = await _sessionLock
                .UnlockAsync(token: command.Token)
                .ConfigureAwait(false);
        }
    }
}
