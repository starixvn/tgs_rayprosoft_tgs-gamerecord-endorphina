using System.Text;
using System.Text.Json;
using Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Messages;

internal class ManualMessageHandler : IMessageProcessor
{
    private readonly ILogger<ManualMessageHandler> _logger;
    private readonly ICommandService<ManualSessionProcessCommand> _service;

    public ManualMessageHandler(
        ILogger<ManualMessageHandler> logger,
        ICommandService<ManualSessionProcessCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);
        var message = JsonSerializer.Deserialize<ManualMessageModel>(json);

        if (message == null)
        {
            _logger.LogError("Information Received:{LogInfo}", message);
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new ManualMessageResponse(false)));
        }

        _logger.LogInformation("Information Received:{LogInfo}", message);

        await _service.ExecuteAsync(new ManualSessionProcessCommand(
            ApiUrl: message.ApiUrl,
            MerchantId: message.MerchantId,
            SecretKey: message.SecretKey,
            Token: message.Token), cancellationToken).ConfigureAwait(false);

        var response = new ManualMessageResponse(true);
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
    }
}
