using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.Endorphina.Common;
using Tgs.GameRecord.Endorphina.ViewModels;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Controllers;

[Route("[controller]")]
[ApiController]
public class SessionController : ControllerBase
{
	/// <summary>
	/// Sessions the process asynchronous.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("process")]
	public ValueTask SessionProcessAsync(
		[FromServices] IMessageSender sender,
		SessionProcessQueryViewModel source)
		=> sender.PublishAsync(
			subject: EndorphinaNatsConst.SessionProcessPublish,
			data: new Common.Protos.SessionProcess()
			{
				ApiUrl = source.ApiUrl.ToString(),
				MerchantId = source.MerchantId,
				SecretKey = source.SecretKey,
				Token = source.Token
			},
			cancellationToken: HttpContext.RequestAborted);

	/// <summary>
	/// Sessions the queued asynchronous.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("queued")]
	public ValueTask SessionQueuedAsync(
		[FromServices] IMessageSender sender,
		SessionProcessQueryViewModel source)
		=> sender.PublishAsync(
			subject: EndorphinaNatsConst.SessionQueuedPublish,
			data: new Common.Protos.SessionProcess()
			{
				ApiUrl = source.ApiUrl.ToString(),
				MerchantId = source.MerchantId,
				SecretKey = source.SecretKey,
				Token = source.Token
			},
			cancellationToken: HttpContext.RequestAborted);

}
