using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameRecord.Endorphina.Core.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;

internal class ScheduleSessionQueuedCommandService(
	ILogger<ScheduleSessionQueuedCommandService> logger,
	IGameInterfaceEndorphinaClient client,
	IMessageSender messageSender) : ICommandService<ScheduleSessionQueuedCommand>
{
	public async ValueTask ExecuteAsync(ScheduleSessionQueuedCommand command, CancellationToken cancellationToken = default)
	{
		var sessionInfo = await client
			.GameSessionInfoAsync(
				command.ApiUrl,
				command.MerchantId,
				command.SecretKey,
				command.Token,
				cancellationToken)
			.ConfigureAwait(false);

		if (sessionInfo == null)
		{
			logger.LogInformation("No Session Info (Session Id:{token})", command.Token);
			return;
		}

		if (sessionInfo.Closed.ToDateTimeOffset() == DateTimeOffset.UnixEpoch)
		{
			logger.LogInformation("Session Id({token}) is Not Expired", command.Token);
			return;
		}

		if (sessionInfo.ErrorsPending != 0)
		{
			logger.LogWarning("Please contact endorphina support for resolution (Session Id:{token})", command.Token);
			return;
		}

		var request = new SessionProcess
		{
			ApiUrl = command.ApiUrl.ToString(),
			MerchantId = command.MerchantId,
			SecretKey = command.SecretKey,
			Token = command.Token
		};

		await messageSender.PublishAsync<ReadOnlyMemory<byte>>(
			subject: "gp.gamerecord.endor.session.process",
			request.ToByteArray(),
			cancellationToken)
			.ConfigureAwait(false);
	}
}
