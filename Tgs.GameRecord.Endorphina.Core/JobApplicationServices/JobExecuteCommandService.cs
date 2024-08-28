using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameRecord.Endorphina.Common;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Core.JobApplicationServices;

internal class JobExecuteCommandService(
	ILogger<JobExecuteCommandService> logger,
	IMessageSender messageSender) : ICommandService<JobExecuteCommand>
{
	public ValueTask ExecuteAsync(JobExecuteCommand command, CancellationToken cancellationToken = default)
	{
		var message = new EndorphinaGameRecordScheduleMessage
		{
			ApiUrl = command.ApiUrl,
			MerchantId = command.MerchantId,
			SecretKey = command.SecretKey
		};

		logger.LogInformation("{logInformation}",
			JsonSerializer.Serialize(new
			{
				LogOn = DateTime.Now.ToString("u"),
				Subject = EndorphinaNatsConst.SchedulePublish,
				Data = new
				{
					message.ApiUrl,
					message.MerchantId,
					message.SecretKey
				},
				Message = "This message publishing handler."
			}));

		return messageSender
			.PublishAsync(
			subject: EndorphinaNatsConst.SchedulePublish,
			data: JsonSerializer.SerializeToUtf8Bytes(message),
			cancellationToken: cancellationToken);
	}
}
