using System.Text.Json;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.Endorphina.Core.Protos;
using Tgs.GameRecord.Endorphina.Messages;
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
	/// <remarks>
	/// <b>Example</b><br />
	///
	///     Post
	///         {
	///             "apiUrl": "https://test.endorphina.network",
	///             "merchantId": "1494",
	///             "secretKey": "9BC9E97FFCD74B16B87BD6FA82C8C2AF",
	///             "token": "AA8BB60F5A134264BE838CF6EBA7C8F1"
	///         }
	/// </remarks>
	[HttpPost("SessionProcess")]
	public async ValueTask SessionProcessAsync(
		[FromServices] IMessageSender sender,
		SessionProcessQueryViewModel source)
	{
		var request = new SessionProcess()
		{
			ApiUrl = source.ApiUrl,
			MerchantId = source.MerchantId,
			SecretKey = source.SecretKey,
			Token = source.Token
		};

		await sender.PublishAsync(
			subject: "gp.gamerecord.endor.session.process",
			data: request.ToByteArray(),
			cancellationToken: HttpContext.RequestAborted)
		.ConfigureAwait(false);
	}

	/// <summary>
	/// Sessions the queued asynchronous.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	/// <remarks>
	/// <b>Example</b><br />
	///
	///     Post
	///         {
	///             "apiUrl": "https://test.endorphina.network",
	///             "merchantId": "1494",
	///             "secretKey": "9BC9E97FFCD74B16B87BD6FA82C8C2AF",
	///             "token": "AA8BB60F5A134264BE838CF6EBA7C8F1"
	///         }
	/// </remarks>
	[HttpPost("SessionQueued")]
	public async ValueTask SessionQueuedAsync(
		[FromServices] IMessageSender sender,
		SessionProcessQueryViewModel source)
	{
		var request = new SessionProcess()
		{
			ApiUrl = source.ApiUrl,
			MerchantId = source.MerchantId,
			SecretKey = source.SecretKey,
			Token = source.Token
		};

		await sender.PublishAsync(
			subject: "gp.gamerecord.endor.session.queued",
			data: request.ToByteArray(),
			cancellationToken: HttpContext.RequestAborted)
		.ConfigureAwait(false);
	}

	/// <summary>
	/// Manuals the asynchronous.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	/// <remarks>
	/// <b>Example</b><br />
	///
	///     Post
	///         {
	///             "apiUrl": "https://test.endorphina.network",
	///             "merchantId": "1494",
	///             "secretKey": "9BC9E97FFCD74B16B87BD6FA82C8C2AF",
	///             "token": "AA8BB60F5A134264BE838CF6EBA7C8F1"
	///         }
	/// </remarks>
	[HttpPost("Manual")]
	public async ValueTask ManualAsync(
		[FromServices] IMessageSender sender,
		ManualQueryViewModel source)
	{
		var request = new ManualMessageModel(
			ApiUrl: new Uri(source.ApiUrl),
			MerchantId: source.MerchantId,
			SecretKey: source.SecretKey,
			Token: source.Token
		);

		await sender.PublishAsync(
			subject: "gp.gamerecord.manual.endor",
			data: JsonSerializer.SerializeToUtf8Bytes(request),
			cancellationToken: HttpContext.RequestAborted)
		.ConfigureAwait(false);
	}
}
