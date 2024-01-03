using Microsoft.Extensions.Logging;
using Tgs.GameInterface.EndorphinaService.Client;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Endorphina.Core.ApplicationServices;

internal class ScheduleSessionProcessCommandService(
    ILogger<ScheduleSessionProcessCommandService> logger,
    IGameInterfaceEndorphinaClient client,
    IGameRecordEndorphinaSessionClient sessionClient,
    ISessionLock sessionLock,
    ICommandService<GamePullRecordCommand> service) : ICommandService<ScheduleSessionProcessCommand>
{
    public async ValueTask ExecuteAsync(ScheduleSessionProcessCommand command, CancellationToken cancellationToken = default)
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

            await sessionClient
               .UpdateSessionProcessingAsync(command.Token, cancellationToken)
               .ConfigureAwait(false);

            var sessionInfo = await client
                .GameSessionInfoAsync(
                    command.ApiUrl,
                    command.MerchantId,
                    command.SecretKey,
                    command.Token,
                    cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false)
                ?? throw new ArgumentException("sessionInfo");

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

            await sessionClient
                .UpdateSessionCompleteAsync(command.Token, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await sessionClient
                .UpdateSessionRetryAsync(command.Token, cancellationToken)
                .ConfigureAwait(false);

            logger.LogError(ex, "Schedule Session Process({token})處理發生例外", command.Token);
        }
        finally
        {
            _ = await sessionLock
                .UnlockAsync(token: command.Token)
                .ConfigureAwait(false);
        }
    }
}
