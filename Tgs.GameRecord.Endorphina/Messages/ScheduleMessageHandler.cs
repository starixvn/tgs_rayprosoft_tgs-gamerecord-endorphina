using System.Text;
using System.Text.Json;
using Tgs.GameRecord.Endorphina.Common;
using Tgs.GameRecord.Endorphina.Core;
using Tgs.GameRecord.Endorphina.Core.ApplicationServices;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Messages;

internal class ScheduleMessageHandler(
	ILogger<ScheduleMessageHandler> logger,
	ISchedulePullRecordDispatcher dispatcher) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		var json = Encoding.UTF8.GetString(data.Span);
		var message = JsonSerializer.Deserialize<EndorphinaGameRecordScheduleMessage>(json)
			?? throw new NullReferenceException("The request data resolves to a null value");

		logger.LogInformation("{logInformation}",
			JsonSerializer.Serialize(new
			{
				LogOn = DateTime.Now.ToString("u"),
				Subject = subject,
				Data = new
				{
					message.ApiUrl,
					message.MerchantId,
					message.SecretKey
				},
				Message = "This message receiving handler."
			}));

		return dispatcher.PushCommandAsync(
			command: new SchedulePullRecordCommand(
				ApiUrl: message.ApiUrl,
				MerchantId: message.MerchantId,
				SecretKey: message.SecretKey),
			cancellationToken: cancellationToken);
	}
}
