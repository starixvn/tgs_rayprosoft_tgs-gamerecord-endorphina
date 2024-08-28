using System.Text.Json;
using Tgs.GameRecord.Endorphina.Core;
using Tgs.GameRecord.Endorphina.Core.ApplicationServices;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Messages;

internal class SessionQueuedMessageHandler(
	ILogger<SessionProcessMessageHandler> logger,
	ISessionQueuedDispatcher dispatcher) : IMessageHandler<Common.Protos.SessionProcess>
{
	public ValueTask HandleAsync(string subject, Common.Protos.SessionProcess data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("{logInformation}",
			JsonSerializer.Serialize(new
			{
				LogOn = DateTime.Now.ToString("u"),
				Subject = subject,
				Data = new
				{
					data.ApiUrl,
					data.MerchantId,
					data.SecretKey,
					data.Token
				},
				Message = "This message receiving handler."
			}));

		return dispatcher.PushCommandAsync(
			command: new SessionQueuedCommand(
				ApiUrl: new Uri(data.ApiUrl),
				MerchantId: data.MerchantId,
				SecretKey: data.SecretKey,
				Token: data.Token),
			cancellationToken: cancellationToken);
	}
}
