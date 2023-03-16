using System.Text;
using System.Text.Json;
using Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;
using Tgs.GameRecord.EndorphinaService.Core.Protos;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Messages;

internal class SessionQueuedMessageHandler : IMessageHandler
{
    private readonly ICommandService<SessionProcessCommand> _service;
    private readonly ILogger<SessionQueuedMessageHandler> _logger;

    public SessionQueuedMessageHandler(
        ICommandService<SessionProcessCommand> service,
        ILogger<SessionQueuedMessageHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);
        var message = JsonSerializer.Deserialize<SessionProcess>(json);

        if (message == null)
        {
            _logger.LogError("Information Received:{LogInfo}", message);
            return;
        }

        _logger.LogInformation("Information Received:{LogInfo}", message);

        var command = new SessionProcessCommand(
            ApiUrl: new Uri(message.ApiUrl),
            MerchantId: message.MerchantId,
            SecretKey: message.SecretKey,
            Token: message.Token);

        await _service
            .ExecuteAsync(command, cancellationToken)
            .ConfigureAwait(false);
    }
}
