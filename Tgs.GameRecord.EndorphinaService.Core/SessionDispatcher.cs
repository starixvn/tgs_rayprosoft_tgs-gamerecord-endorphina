using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.GameRecord.Endorphina.Session.Protos;
using Tgs.GameRecord.EndorphinaService.Core.Protos;
using Tgs.GameRecord.EndorphinaService.Core.WorkerServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core;

internal class SessionDispatcher : ISessionDispatcher
{
    private readonly ILogger<SessionDispatcher> _logger;
    private readonly IMessageSender _messageSender;
    private readonly IGameRecordEndorphinaSessionClient _client;
    private readonly ISessionLock _sessionLock;
    private readonly ICommandService<WorkCommand> _workService;

    public SessionDispatcher(
        ILogger<SessionDispatcher> logger,
        IMessageSender messageSender,
        IGameRecordEndorphinaSessionClient client,
        ISessionLock sessionLock,
        ICommandService<WorkCommand> workService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _sessionLock = sessionLock ?? throw new ArgumentNullException(nameof(sessionLock));
        _workService = workService ?? throw new ArgumentNullException(nameof(workService));
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
                var request = new SessionQueuedRequest
                {
                    ApiUrl = apiUrl.ToString(),
                    MerchantId = merchantId,
                    SecretKey = secretKey,
                    Token = session.Token,
                };

                await _messageSender.PublishAsync(
                    subject: "gp.gamerecord.session.endor",
                    data: request.ToByteArray(), cancellationToken).ConfigureAwait(false);
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
