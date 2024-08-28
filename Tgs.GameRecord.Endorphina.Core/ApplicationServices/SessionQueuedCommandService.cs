using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameRecord.Endorphina.Common;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;

internal class SessionQueuedCommandService(
	ILogger<SessionQueuedCommandService> logger,
	IGameInterfaceEndorphinaClient client,
	IMessageSender messageSender) : ICommandService<SessionQueuedCommand>
{
	public async ValueTask ExecuteAsync(SessionQueuedCommand command, CancellationToken cancellationToken = default)
	{
		var sessionInfo = await client.GameSessionInfoAsync(
			apiUrl: command.ApiUrl,
			merchantId: command.MerchantId,
			secretKey: command.SecretKey,
			token: command.Token,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (sessionInfo is null)
		{
			logger.LogWarning("{logWarning}",
				JsonSerializer.Serialize(new
				{
					LogOn = DateTime.Now.ToString("u"),
					Data = new
					{
						ApiUrl = command.ApiUrl.ToString(),
						command.MerchantId,
						command.SecretKey,
						command.Token
					},
					Message = "This data does not exist.(此數據不存在.)"
				}));
			return;
		}

		if (sessionInfo.Closed.ToDateTimeOffset() == DateTimeOffset.UnixEpoch)
		{
			logger.LogWarning("{logWarning}",
				JsonSerializer.Serialize(new
				{
					LogOn = DateTime.Now.ToString("u"),
					Data = new
					{
						ApiUrl = command.ApiUrl.ToString(),
						command.MerchantId,
						command.SecretKey,
						command.Token
					},
					Message = "This game has not yet been settled.(此遊戲尚未結算.)"
				}));
			return;
		}

		if (sessionInfo.ErrorsPending != 0)
		{
			logger.LogWarning("{logWarning}",
				JsonSerializer.Serialize(new
				{
					LogOn = DateTime.Now.ToString("u"),
					Data = new
					{
						ApiUrl = command.ApiUrl.ToString(),
						command.MerchantId,
						command.SecretKey,
						command.Token
					},
					Message = "Please contact endorphina support for resolution.(請聯絡 endorphina 支援人員尋求解決方案)"
				}));
			return;
		}

		await messageSender.PublishAsync(
			subject: EndorphinaNatsConst.SessionProcessPublish,
			data: new Common.Protos.SessionProcess
			{
				ApiUrl = command.ApiUrl.ToString(),
				MerchantId = command.MerchantId,
				SecretKey = command.SecretKey,
				Token = command.Token
			},
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}
}
