using System.Text;
using System.Text.Json;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.GameRecord.EndorphinaService.Core.WorkerServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Messages;

internal class SessionQueuedMessageHandler : IMessageHandler
{
    private readonly ICommandService<WorkCommand> _scheduleService;
    private readonly ILogger<SessionQueuedMessageHandler> _logger;
    private readonly IGameRecordEndorphinaSessionClient _client;

    public SessionQueuedMessageHandler(
        ICommandService<WorkCommand> scheduleService,
        ILogger<SessionQueuedMessageHandler> logger,
        IGameRecordEndorphinaSessionClient client)
    {
        _scheduleService = scheduleService ?? throw new ArgumentNullException(nameof(scheduleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);

        var messageModel = JsonSerializer.Deserialize<SessionQueuedMessageModel>(json);

        if (messageModel == null)
        {
            _logger.LogError("Information Received:{LogInfo}", messageModel);
            return;
        }

        _logger.LogInformation("Information Received:{LogInfo}", messageModel);

        await _client
                    .UpdateSessionProcessingAsync(messageModel.Token, cancellationToken)
                    .ConfigureAwait(false);

        var command = new WorkCommand(
                ApiUrl: messageModel.ApiUrl,
                MerchantId: messageModel.MerchantId,
                SecretKey: messageModel.SecretKey,
                Token: messageModel.Token);

        _logger.LogInformation("LogOn:{logOn} execute the command:{command}", DateTime.Now.ToString("o"), command);

        await _scheduleService
            .ExecuteAsync(
                command: command,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await _client
            .UpdateSessionCompleteAsync(messageModel.Token, cancellationToken)
            .ConfigureAwait(false);
    }
}
