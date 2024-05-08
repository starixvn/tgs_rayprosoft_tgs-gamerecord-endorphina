using Microsoft.Extensions.Logging;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;

internal class ManualSessionProcessCommandService(
	ILogger<ManualSessionProcessCommandService> logger,
	IGameInterfaceEndorphinaClient client,
	ISessionLock sessionLock,
	ICommandService<GamePullRecordCommand> service) : ICommandService<ManualSessionProcessCommand>
{
	public async ValueTask ExecuteAsync(ManualSessionProcessCommand command, CancellationToken cancellationToken = default)
	{
		try
		{
			if (!await sessionLock
					.LockAsync(token: command.Token)
					.ConfigureAwait(false))
			{
				logger.LogWarning("LogOn:{logOn} Message:This game provider Endorphina session has been locked. ({token})",
					DateTime.Now.ToString("o"),
					command.Token);
				return;
			}

			var sessionInfo = await client
				.GameSessionInfoAsync(
					command.ApiUrl,
					command.MerchantId,
					command.SecretKey,
					command.Token,
					cancellationToken)
				.ConfigureAwait(continueOnCapturedContext: false)
				?? throw new ArgumentException("sessionInfo");

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

			var sessionStatistics = await client
				.GameSessionStatisticsAsync(
					command.ApiUrl,
					command.MerchantId,
					command.SecretKey,
					command.Token,
					cancellationToken)
				.ConfigureAwait(false)
				?? throw new ArgumentException("sessionStatistics");

			var recordCommand = new GamePullRecordCommand(
				ExternalRoundId: sessionInfo.Id,
				GameExternalId: sessionInfo.GameName,
				PlayerCode: sessionInfo.Username,
				ExternalCurrency: sessionInfo.Currency,
				ExternalCreatedOn: sessionInfo.Created.ToDateTimeOffset(),
				ExternalClosedOn: sessionInfo.Closed.ToDateTimeOffset(),
				Bet: sessionStatistics.Bet.ToDecimal().GetValueOrDefault(0.0m),
				Win: sessionStatistics.Win.ToDecimal().GetValueOrDefault(0.0m),
				Games: sessionStatistics.Games,
				Jackpots: sessionStatistics.Jackpots,
				JackpotWin: sessionStatistics.JackpotWin);

			await service
				.ExecuteAsync(recordCommand, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Manual Session Process({token})處理發生例外", command.Token);
		}
		finally
		{
			_ = await sessionLock
				.UnlockAsync(token: command.Token)
				.ConfigureAwait(false);
		}
	}
}
