using Tgs.GameRecord.Endorphina.Core.ApplicationServices;
using Tgs.GameRecord.Endorphina.Core.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Messages;

internal class SessionProcessMessageHandler : IMessageHandler
{
    private readonly ICommandService<ScheduleSessionProcessCommand> _service;
    private readonly ILogger<SessionProcessMessageHandler> _logger;

    public SessionProcessMessageHandler(
        ICommandService<ScheduleSessionProcessCommand> service,
        ILogger<SessionProcessMessageHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var message = SessionProcess.Parser.ParseFrom(data.Span)
            ?? throw new ArgumentException("參數錯誤");

        _logger.LogInformation("Session Process Information Received:{LogInfo}", message);

        var command = new ScheduleSessionProcessCommand(
            ApiUrl: new Uri(message.ApiUrl),
            MerchantId: message.MerchantId,
            SecretKey: message.SecretKey,
            Token: message.Token);

        await _service
            .ExecuteAsync(command, cancellationToken)
            .ConfigureAwait(false);
    }
}
