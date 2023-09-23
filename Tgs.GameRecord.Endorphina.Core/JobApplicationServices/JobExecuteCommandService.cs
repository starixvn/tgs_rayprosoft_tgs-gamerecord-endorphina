using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Core.JobApplicationServices;

internal class JobExecuteCommandService : ICommandService<JobExecuteCommand>
{
    private readonly ILogger<JobExecuteCommandService> _logger;
    private readonly IMessageSender _messageSender;

    public JobExecuteCommandService(
        ILogger<JobExecuteCommandService> logger,
        IMessageSender messageSender)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    public ValueTask ExecuteAsync(JobExecuteCommand command, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            command.ApiUrl,
            command.MerchantId,
            command.SecretKey
        };

        _logger.LogDebug("LogOn:{logOn} Parameters:{parameters}", DateTime.Now.ToString("u"), parameters);

        return _messageSender
            .PublishAsync(
            subject: "gp.gamerecord.schedule.endor",
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(parameters)),
            cancellationToken: cancellationToken);
    }
}
