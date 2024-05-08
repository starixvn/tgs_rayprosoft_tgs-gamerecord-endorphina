using System.Text;
using System.Text.Json;
using Tgs.GameRecord.Endorphina.Core.DispatchServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Messages;

internal class ScheduleMessageHandler(
	ILogger<ScheduleMessageHandler> logger,
	ICommandService<ScheduleDispatchCommand> service) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		var json = Encoding.UTF8.GetString(data.Span);
		var message = JsonSerializer.Deserialize<ScheduleMessageModel>(json)
			?? throw new NullReferenceException("The request data resolves to a null value");

		logger.LogInformation("LogOn:{logOn} Subject:{subject} Information Request:{message}",
			DateTime.Now.ToString("o"),
			subject,
			message);

		return service.ExecuteAsync(new ScheduleDispatchCommand(
			ApiUrl: message.ApiUrl,
			MerchantId: message.MerchantId,
			SecretKey: message.SecretKey), cancellationToken);
	}
}
