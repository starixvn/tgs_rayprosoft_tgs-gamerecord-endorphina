using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;

internal class ManualPullRecordCommandService(
	ILogger<ManualPullRecordCommandService> logger,
	IGameInterfaceEndorphinaClient client,
	ICommandService<HandleRecordCommand> service) : ICommandService<ManualPullRecordCommand>
{
	public async ValueTask ExecuteAsync(ManualPullRecordCommand command, CancellationToken cancellationToken = default)
	{
		try
		{
			var sessionInfo = await client.GameSessionInfoAsync(
				apiUrl: command.ApiUrl,
				merchantId: command.MerchantId,
				secretKey: command.SecretKey,
				token: command.Token,
				cancellationToken: cancellationToken)
				.ConfigureAwait(continueOnCapturedContext: false)
				?? throw new ArgumentException("sessionInfo");

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

			var sessionStatistics = await client.GameSessionStatisticsAsync(
				apiUrl: command.ApiUrl,
				merchantId: command.MerchantId,
				secretKey: command.SecretKey,
				token: command.Token,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? throw new ArgumentException("sessionStatistics");

			var recordCommand = new HandleRecordCommand(
				ExternalRoundId: sessionInfo.Id,
				ExternalGameId: sessionInfo.GameName,
				PlayerCode: sessionInfo.Username,
				ExternalCurrency: sessionInfo.Currency,
				ExternalCreatedOn: sessionInfo.Created.ToDateTimeOffset(),
				ExternalClosedOn: sessionInfo.Closed.ToDateTimeOffset(),
				Bet: sessionStatistics.Bet.ToDecimal().GetValueOrDefault(0.0m),
				Win: sessionStatistics.Win.ToDecimal().GetValueOrDefault(0.0m),
				Games: sessionStatistics.Games,
				Jackpots: sessionStatistics.Jackpots,
				JackpotWin: sessionStatistics.JackpotWin);

			await service.ExecuteAsync(
				command: recordCommand,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "{logError}",
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
					Message = "An exception occurs."
				}));
			throw;
		}
	}
}
