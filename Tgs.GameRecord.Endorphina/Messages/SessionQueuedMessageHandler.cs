using Tgs.GameRecord.Endorphina.Core.ApplicationServices;
using Tgs.GameRecord.Endorphina.Core.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Messages;

internal class SessionQueuedMessageHandler(
	ICommandService<ScheduleSessionQueuedCommand> service,
	ILogger<SessionProcessMessageHandler> logger) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		var message = SessionProcess.Parser.ParseFrom(data.Span)
			?? throw new ArgumentException("參數錯誤");

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{message}",
			DateTime.Now.ToString("o"),
			subject,
			message);

		var command = new ScheduleSessionQueuedCommand(
			ApiUrl: new Uri(message.ApiUrl),
			MerchantId: message.MerchantId,
			SecretKey: message.SecretKey,
			Token: message.Token);

		return service.ExecuteAsync(command, cancellationToken);
	}
}
