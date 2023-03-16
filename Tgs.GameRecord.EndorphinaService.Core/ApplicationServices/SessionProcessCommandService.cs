using Microsoft.Extensions.Logging;
using Tgs.GameRecord.Endorphina.Session.Client;
using Tgs.GameRecord.EndorphinaService.Core.WorkerServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.EndorphinaService.Core.ApplicationServices;

internal class SessionProcessCommandService : ICommandService<SessionProcessCommand>
{
    private readonly ICommandService<WorkCommand> _workService;
    private readonly IGameRecordEndorphinaSessionClient _client;
    private readonly ILogger<SessionProcessCommandService> _logger;

    public SessionProcessCommandService(
        ICommandService<WorkCommand> workService,
        IGameRecordEndorphinaSessionClient client,
        ILogger<SessionProcessCommandService> logger)
    {
        _workService = workService ?? throw new ArgumentNullException(nameof(workService));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask ExecuteAsync(SessionProcessCommand command, CancellationToken cancellationToken = default)
    {
        await _client
            .UpdateSessionProcessingAsync(command.Token, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("LogOn:{logOn} execute the command:{command}", DateTime.Now.ToString("o"), command);

        await _workService
            .ExecuteAsync(
                command: new WorkCommand(
                ApiUrl: command.ApiUrl,
                MerchantId: command.MerchantId,
                SecretKey: command.SecretKey,
                Token: command.Token),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await _client
            .UpdateSessionCompleteAsync(command.Token, cancellationToken)
            .ConfigureAwait(false);
    }
}
