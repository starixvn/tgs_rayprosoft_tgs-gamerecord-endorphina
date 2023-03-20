using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameRecord.EndorphinaService.Core.Protos;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;

internal class ScheduleSessionQueuedCommandService : ICommandService<ScheduleSessionQueuedCommand>
{
    private readonly ILogger<ScheduleSessionQueuedCommandService> _logger;
    private readonly IGameInterfaceEndorphinaClient _client;
    private readonly IMessageSender _messageSender;

    public ScheduleSessionQueuedCommandService(
        ILogger<ScheduleSessionQueuedCommandService> logger,
        IGameInterfaceEndorphinaClient client,
        IMessageSender messageSender)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    public async ValueTask ExecuteAsync(ScheduleSessionQueuedCommand command, CancellationToken cancellationToken = default)
    {
        var sessionInfo = await _client
            .GameSessionInfoAsync(
                command.ApiUrl,
                command.MerchantId,
                command.SecretKey,
                command.Token,
                cancellationToken)
            .ConfigureAwait(false);

        if (sessionInfo == null)
        {
            _logger.LogInformation("No Session Info (Session Id:{token})", command.Token);
            return;
        }

        if (sessionInfo.ErrorsPending != 0)
        {
            _logger.LogInformation("Please contact endorphina support for resolution (Session Id:{token})", command.Token);
            return;
        }

        var request = new SessionProcess
        {
            ApiUrl = command.ApiUrl.ToString(),
            MerchantId = command.MerchantId,
            SecretKey = command.SecretKey,
            Token = command.Token
        };

        await _messageSender
            .PublishAsync(subject: "gp.gamerecord.endor.session.process", request.ToByteArray(), cancellationToken)
            .ConfigureAwait(false);
    }
}
