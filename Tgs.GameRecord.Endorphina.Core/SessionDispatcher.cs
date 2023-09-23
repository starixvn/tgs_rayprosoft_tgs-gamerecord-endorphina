using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.GameRecord.Endorphina.Core.Protos;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.GameRecord.Endorphina.Session.Protos;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Core;

internal class SessionDispatcher : ISessionDispatcher
{
    private readonly ILogger<SessionDispatcher> _logger;
    private readonly IMessageSender _messageSender;
    private readonly IGameRecordEndorphinaSessionClient _client;

    public SessionDispatcher(
        ILogger<SessionDispatcher> logger,
        IMessageSender messageSender,
        IGameRecordEndorphinaSessionClient client)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask ExecuteAsync(
        Uri apiUrl,
        string merchantId,
        string secretKey,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _client
            .QuerySessionsAsync(TokenState.Queued, merchantId, cancellationToken)
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
            if (session.Retry >= 3)
            {
                await _client
                    .MoveFailedAsync(session.Token, cancellationToken)
                    .ConfigureAwait(false);

                _logger.LogWarning("Move Failed session {token}", session.Token);
                continue;
            }

            var request = new SessionProcess
            {
                ApiUrl = apiUrl.ToString(),
                MerchantId = merchantId,
                SecretKey = secretKey,
                Token = session.Token,
            };

            await _messageSender
                .PublishAsync(subject: "gp.gamerecord.endor.session.queued", request.ToByteArray(), cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
