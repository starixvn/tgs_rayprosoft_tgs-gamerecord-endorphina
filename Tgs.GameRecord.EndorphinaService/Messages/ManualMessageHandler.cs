using System.Text;
using System.Text.Json;
using Tgs.GameRecord.EndorphinaService.Core.WorkerServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Messages;

internal class ManualMessageHandler : IMessageProcessor
{
    private readonly ILogger<ManualMessageHandler> _logger;
    private readonly ICommandService<WorkCommand> _service;

    public ManualMessageHandler(
        ILogger<ManualMessageHandler> logger,
        ICommandService<WorkCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);

        var request = JsonSerializer.Deserialize<ManualMessageModel>(json)!;

        _logger.LogInformation("Information Received:{LogInfo}", request);

        await _service.ExecuteAsync(new WorkCommand(
            ApiUrl: request.ApiUrl,
            MerchantId: request.MerchantId,
            SecretKey: request.SecretKey,
            Token: request.Token), cancellationToken).ConfigureAwait(false);

        var response = new ManualMessageResponse(true);
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
    }
}
