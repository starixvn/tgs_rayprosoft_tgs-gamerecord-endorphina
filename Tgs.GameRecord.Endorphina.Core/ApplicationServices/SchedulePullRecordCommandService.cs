using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameRecord.Endorphina.Common;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.GameRecord.Endorphina.Session.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;

internal class SchedulePullRecordCommandService(
	ILogger<SchedulePullRecordCommandService> logger,
	IGameRecordEndorphinaSessionClient client,
	IMessageSender messageSender) : ICommandService<SchedulePullRecordCommand>
{
	public async ValueTask ExecuteAsync(SchedulePullRecordCommand command, CancellationToken cancellationToken = default)
	{
		var sessions = client.QuerySessionsAsync(
			state: TokenState.Queued,
			merchantId: command.MerchantId,
			cancellationToken: cancellationToken);

		await foreach (var session in sessions)
		{
			if (session.Retry >= 3)
			{
				await client.MoveFailedAsync(
					session.Token,
					cancellationToken)
					.ConfigureAwait(false);

				logger.LogWarning("{logWarning}",
					JsonSerializer.Serialize(new
					{
						LogOn = DateTime.Now.ToString("u"),
						Data = new
						{
							ApiUrl = command.ApiUrl.ToString(),
							command.MerchantId,
							command.SecretKey,
							session.Token
						},
						Message = "Move Failed session."
					}));
				continue;
			}

			await messageSender.PublishAsync(
				subject: EndorphinaNatsConst.SessionQueuedPublish,
				data: new Common.Protos.SessionProcess
				{
					ApiUrl = command.ApiUrl.ToString(),
					MerchantId = command.MerchantId,
					SecretKey = command.SecretKey,
					Token = session.Token
				},
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
	}
}
