using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Core.JobApplicationServices;

internal class JobExecuteCommandService(
	ILogger<JobExecuteCommandService> logger,
	IMessageSender messageSender) : ICommandService<JobExecuteCommand>
{
	public ValueTask ExecuteAsync(JobExecuteCommand command, CancellationToken cancellationToken = default)
	{
		var parameters = new
		{
			command.ApiUrl,
			command.MerchantId,
			command.SecretKey
		};

		logger.LogDebug("LogOn:{logOn} Parameters:{parameters}", DateTime.Now.ToString("u"), parameters);

		return messageSender
			.PublishAsync(
			subject: "gp.gamerecord.schedule.endor",
			data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(parameters)),
			cancellationToken: cancellationToken);
	}
}
