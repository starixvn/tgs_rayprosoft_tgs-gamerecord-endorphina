using System.Text;
using System.Text.Json;
using Tgs.GameRecord.Endorphina.Core.DispatchServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Messages;

internal class ScheduleMessageHandler(
    ILogger<ScheduleMessageHandler> logger,
    ICommandService<ScheduleDispatchCommand> service) : IMessageHandler
{
    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);

        var messageModel = JsonSerializer.Deserialize<ScheduleMessageModel>(json);

        if (messageModel == null)
        {
            logger.LogError("Information Received:{LogInfo}", messageModel);
            return ValueTask.CompletedTask;
        }

        logger.LogInformation("Information Received:{LogInfo}", messageModel);

        return service.ExecuteAsync(new ScheduleDispatchCommand(
            ApiUrl: messageModel.ApiUrl,
            MerchantId: messageModel.MerchantId,
            SecretKey: messageModel.SecretKey), cancellationToken);
    }
}
