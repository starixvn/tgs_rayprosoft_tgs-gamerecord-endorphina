using Tgs.GameRecord.Endorphina.Core.ApplicationServices;
using Tgs.GameRecord.Endorphina.Core.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Messages;

internal class SessionProcessMessageHandler(
    ICommandService<ScheduleSessionProcessCommand> service,
    ILogger<SessionProcessMessageHandler> logger) : IMessageHandler
{
    public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var message = SessionProcess.Parser.ParseFrom(data.Span)
            ?? throw new ArgumentException("參數錯誤");

        logger.LogInformation("Session Process Information Received:{LogInfo}", message);

        var command = new ScheduleSessionProcessCommand(
            ApiUrl: new Uri(message.ApiUrl),
            MerchantId: message.MerchantId,
            SecretKey: message.SecretKey,
            Token: message.Token);

        await service
            .ExecuteAsync(command, cancellationToken)
            .ConfigureAwait(false);
    }
}
