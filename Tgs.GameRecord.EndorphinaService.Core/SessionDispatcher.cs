using Microsoft.Extensions.Logging;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.GameRecord.Endorphina.Session.Protos;
using Tgs.GameRecord.EndorphinaService.Core.ScheduleWorkerServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core;

internal class SessionDispatcher : ISessionDispatcher
{
    private readonly ILogger<SessionDispatcher> _logger;
    private readonly IGameRecordEndorphinaSessionClient _client;
    private readonly ISessionLock _sessionLock;
    private readonly ICommandService<ScheduleCommand> _scheduleService;

    public SessionDispatcher(
        ILogger<SessionDispatcher> logger,
        IGameRecordEndorphinaSessionClient client,
        ISessionLock sessionLock,
        ICommandService<ScheduleCommand> scheduleService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _sessionLock = sessionLock ?? throw new ArgumentNullException(nameof(sessionLock));
        _scheduleService = scheduleService ?? throw new ArgumentNullException(nameof(scheduleService));
    }

    public async ValueTask ExecuteAsync(
        Uri apiUrl,
        string merchantId,
        string secretKey,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _client
            .QuerySessionsAsync(TokenState.Queued, cancellationToken)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        if (sessions is null)
        {
            _logger.LogError("LogOn:{logOn} Message: The sessions are null.", DateTime.Now.ToString("o"));
            return;
        }

        if (!sessions.Any())
        {
            _logger.LogInformation("LogOn:{logOn} Message: The sessions are empty.", DateTime.Now.ToString("o"));
            return;
        }
        foreach (var session in sessions)
        {
            if (session.Retry == 3)
            {
                await _client
                    .MoveFailedAsync(session.Token, cancellationToken)
                    .ConfigureAwait(false);
                continue;
            }

            if (!await _sessionLock
                .LockAsync(token: session.Token)
                .ConfigureAwait(false))
            {
                _logger.LogWarning("LogOn:{logOn} Message:This game provider Endorphina session has been locked. ({token})",
                    DateTime.Now.ToString("o"),
                    session.Token);
                continue;
            }

            try
            {
                await _client
                    .UpdateSessionProcessingAsync(session.Token, cancellationToken)
                    .ConfigureAwait(false);

                var command = new ScheduleCommand(
                        ApiUrl: apiUrl,
                        MerchantId: merchantId,
                        SecretKey: secretKey,
                        Session: session);

                _logger.LogInformation("LogOn:{logOn} execute the command:{command}", DateTime.Now.ToString("o"), command);

                await _scheduleService
                    .ExecuteAsync(
                        command: command,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                await _client
                    .UpdateSessionCompleteAsync(session.Token, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred in the game provider Endorphina session. ({token})", session.Token);

                await _client
                    .UpdateSessionFailedAsync(session.Token, cancellationToken)
                    .ConfigureAwait(false);
            }
            finally
            {
                _ = await _sessionLock
                     .UnlockAsync(token: session.Token)
                     .ConfigureAwait(false);
            }
        }
    }
}
