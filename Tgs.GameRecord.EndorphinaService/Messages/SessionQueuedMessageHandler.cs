using Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;
using Tgs.GameRecord.EndorphinaService.Core.Protos;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Messages;

internal class SessionQueuedMessageHandler : IMessageHandler
{
    private readonly ICommandService<SessionQueuedCommand> _service;
    private readonly ILogger<SessionProcessMessageHandler> _logger;

    public SessionQueuedMessageHandler(
        ICommandService<SessionQueuedCommand> service,
        ILogger<SessionProcessMessageHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var message = SessionProcess.Parser.ParseFrom(data.Span)
            ?? throw new ArgumentException("參數錯誤");

        _logger.LogInformation("SessionQueued Information Received:{LogInfo}", message);

        var command = new SessionQueuedCommand(
            ApiUrl: new Uri(message.ApiUrl),
            MerchantId: message.MerchantId,
            SecretKey: message.SecretKey,
            Token: message.Token);

        await _service
            .ExecuteAsync(command, cancellationToken)
            .ConfigureAwait(false);
    }
}
