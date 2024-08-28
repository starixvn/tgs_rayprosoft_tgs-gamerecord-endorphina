using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;

internal class SessionProcessCommandService(
	ILogger<SessionProcessCommandService> logger,
	IGameInterfaceEndorphinaClient client,
	IGameRecordEndorphinaSessionClient sessionClient,
	ICommandService<HandleRecordCommand> service) : ICommandService<SessionProcessCommand>
{
	public async ValueTask ExecuteAsync(SessionProcessCommand command, CancellationToken cancellationToken = default)
	{
		try
		{
			await sessionClient.UpdateSessionProcessingAsync(
				token: command.Token,
				cancellationToken: cancellationToken)
			   .ConfigureAwait(false);

			var sessionInfo = await client.GameSessionInfoAsync(
				apiUrl: command.ApiUrl,
				merchantId: command.MerchantId,
				secretKey: command.SecretKey,
				token: command.Token,
				cancellationToken: cancellationToken)
				.ConfigureAwait(continueOnCapturedContext: false)
				?? throw new ArgumentException("sessionInfo");

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

			await sessionClient.UpdateSessionCompleteAsync(
				token: command.Token,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			await sessionClient.UpdateSessionRetryAsync(
				token: command.Token,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			logger.LogError(ex, "{logWarning}",
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
