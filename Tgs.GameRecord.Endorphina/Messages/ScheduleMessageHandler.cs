using System.Text;
using System.Text.Json;
using Tgs.GameRecord.Endorphina.Core.DispatchServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Messages;

internal class ScheduleMessageHandler : IMessageHandler
{
    private readonly ILogger<ScheduleMessageHandler> _logger;
    private readonly ICommandService<ScheduleDispatchCommand> _service;

    public ScheduleMessageHandler(
        ILogger<ScheduleMessageHandler> logger,
        ICommandService<ScheduleDispatchCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);

        var messageModel = JsonSerializer.Deserialize<ScheduleMessageModel>(json);

        if (messageModel == null)
        {
            _logger.LogError("Information Received:{LogInfo}", messageModel);
            return ValueTask.CompletedTask;
        }

        _logger.LogInformation("Information Received:{LogInfo}", messageModel);

        return _service.ExecuteAsync(new ScheduleDispatchCommand(
            ApiUrl: messageModel.ApiUrl,
            MerchantId: messageModel.MerchantId,
            SecretKey: messageModel.SecretKey), cancellationToken);
    }
}
