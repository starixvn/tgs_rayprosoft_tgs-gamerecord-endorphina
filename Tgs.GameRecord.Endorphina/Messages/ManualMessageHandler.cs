using System.Text;
using System.Text.Json;
using Tgs.GameRecord.Endorphina.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Messages;

internal class ManualMessageHandler(
    ILogger<ManualMessageHandler> logger,
    ICommandService<ManualSessionProcessCommand> service) : IMessageProcessor
{
    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);
        var message = JsonSerializer.Deserialize<ManualMessageModel>(json);

        if (message == null)
        {
            logger.LogError("Information Received:{LogInfo}", message);
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new ManualMessageResponse(false)));
        }

        logger.LogInformation("Information Received:{LogInfo}", message);

        await service.ExecuteAsync(new ManualSessionProcessCommand(
            ApiUrl: message.ApiUrl,
            MerchantId: message.MerchantId,
            SecretKey: message.SecretKey,
            Token: message.Token), cancellationToken).ConfigureAwait(false);

        var response = new ManualMessageResponse(true);
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
    }
}
